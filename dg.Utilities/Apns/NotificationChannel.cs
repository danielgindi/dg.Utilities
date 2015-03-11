using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Net;

namespace dg.Utilities.Apns
{
    public class NotificationChannel : IDisposable
    {
        static TraceSwitch generalSwitch = new TraceSwitch("Apns", "Apns Trace Switch");

        private TcpClient _apnsClient;
        private SslStream _apnsStream;
        private ManualResetEvent _manualResetEvent = new ManualResetEvent(false);
        private X509Certificate _certificate;
        private X509CertificateCollection _certificates;

        public string P12File { get; set; }
        public string P12FilePassword { get; set; }


        // Default configurations for Apns
        private const string ProductionHost = "gateway.push.apple.com";
        private const string SandboxHost = "gateway.sandbox.push.apple.com";
        private const int NotificationPort = 2195;

        // Default configurations for Feedback Service
        private const string ProductionFeedbackHost = "feedback.push.apple.com";
        private const string SandboxFeedbackHost = "feedback.sandbox.push.apple.com";
        private const int FeedbackPort = 2196;

        private bool _connected = false;
        private bool _connectionReturnedError = false;
        private int _current = 0;

        private readonly string _host;
        private readonly string _feedbackHost;

        private List<NotificationPayload> _notifications = new List<NotificationPayload>();
        private List<NotificationDeliveryError> _errors = null;

        #region IDisposable members
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Disconnect();
                if (_manualResetEvent != null)
                {
                    _manualResetEvent.Dispose();
                    _manualResetEvent = null;
                }
            }
            // Now clean up Native Resources (Pointers)
        }
        #endregion

        public NotificationChannel(bool useSandbox, string p12File, string p12FilePassword)
        {
            if (useSandbox)
            {
                _host = SandboxHost;
                _feedbackHost = SandboxFeedbackHost;
            }
            else
            {
                _host = ProductionHost;
                _feedbackHost = ProductionFeedbackHost;
            }

            //Load Certificates in to collection.
            _certificate = string.IsNullOrEmpty(p12FilePassword) ?
                new X509Certificate2(p12File, (string)null, X509KeyStorageFlags.MachineKeySet) :
                new X509Certificate2(p12File, p12FilePassword, X509KeyStorageFlags.MachineKeySet);
            if (_certificate == null)
            {
                _certificate = string.IsNullOrEmpty(p12FilePassword) ?
                    new X509Certificate2(p12File, (string)null, X509KeyStorageFlags.UserKeySet) :
                    new X509Certificate2(p12File, p12FilePassword, X509KeyStorageFlags.UserKeySet);
            }
            _certificates = new X509CertificateCollection { _certificate };
        }

        public List<NotificationDeliveryError> SendToApple(List<NotificationPayload> queue)
        {
            _errors = new List<NotificationDeliveryError>();
            _current = 0;

            if (generalSwitch.TraceInfo)
            {
                Debug.WriteLine("Apns: Payload queue received.");
            }

            _notifications = queue;
            if (queue.Count < 8999)
            {
                while (!SendQueueToApple())
                {
                    SendQueueToApple();
                }
            }
            else
            {
                const int pageSize = 8999;
                int numberOfPages = (queue.Count / pageSize) + (queue.Count % pageSize == 0 ? 0 : 1);
                int currentPage = 0;
                int skipAmount;

                while (currentPage < numberOfPages)
                {
                    skipAmount = currentPage * pageSize;
                    _notifications = queue.GetRange(skipAmount, queue.Count - skipAmount >= pageSize ? pageSize : queue.Count - skipAmount);
                    while (!SendQueueToApple())
                    {
                        SendQueueToApple();
                    }
                    currentPage++;
                }
            }
            //Close the connection
            Disconnect();
            return _errors;
        }

        private bool SendQueueToApple()
        {
            _connectionReturnedError = false;
            _manualResetEvent.Reset();

            int PayloadId = 0;
            MyAsyncInfo info = null;
            IAsyncResult ar = null;
            NotificationPayload payload;
            byte[] payloadBytes;
            bool didConnect = false;

            while (!_connectionReturnedError && _current < _notifications.Count)
            {
                if (!_connected)
                {
                    ConnectResult res = Connect(_host, NotificationPort, _certificates);

                    if (res == ConnectResult.SSLError)
                    {
                        return true; // Allegedly finished sending. Do not report "errors" as these can cause removing tokens from DB
                    }

                    if (res == ConnectResult.Failed)
                    {
                        _connectionReturnedError = true;
                        break;
                    }

                    info = new MyAsyncInfo(_apnsStream);
                    didConnect = true;
                    ar = _apnsStream.BeginRead(info.ReadBuffer, 0, 6, OnAsyncRead, info);
                }
                payload = _notifications[_current];
                payloadBytes = null;
                try
                {
                    payload.PayloadId = PayloadId++;
                    payloadBytes = GeneratePayload(payload);
                }
                catch (Exception ex)
                {
                    if (generalSwitch.TraceError)
                    {
                        Debug.WriteLine("Apns: ERROR: An error occurred on generating payload for device token {0} - {1}", payload.DeviceToken, ex.Message);
                    }

                    _current++;
                    continue;
                }

                if (payloadBytes != null && !_connectionReturnedError)
                {
                    try
                    {
                        _apnsStream.Write(payloadBytes);
                        if (generalSwitch.TraceInfo)
                        {
                            Debug.WriteLine("Apns: Notification successfully sent to Apns server for Device Token : " + payload.DeviceToken);
                        }

                        _current++;
                    }
                    catch (System.Exception ex)
                    {
                        if (generalSwitch.TraceError)
                        {
                            Debug.WriteLine("Apns: ERROR: An error occurred on sending payload for device token {0} - {1}", payload.DeviceToken, ex.Message);
                        }
                    }
                }
                else
                {
                    _current++;
                }
            }

            if (ar != null && !ar.IsCompleted)
            {
                // Give Apple a chance to let us know something went wrong
                ar.AsyncWaitHandle.WaitOne(500);
                if (!ar.IsCompleted)
                {
                    // Dispose the channel, which will force the async callback,
                    // resulting in an ObjectDisposedException on EndRead.
                    _apnsStream.Dispose();
                }
            }
            if (didConnect)
            {
                _manualResetEvent.WaitOne();
            }
            return !_connectionReturnedError;
        }

        private void OnAsyncRead(IAsyncResult ar)
        {
            int payLoadIndex = 0;
            try
            {
                MyAsyncInfo info = ar.AsyncState as MyAsyncInfo;
                if (info.MyStream.EndRead(ar) == 6 && info.ReadBuffer[0] == 8)
                {
                    DeliveryErrorType error = (DeliveryErrorType)info.ReadBuffer[1];
                    _connectionReturnedError = true;
                    int index = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(info.ReadBuffer, 2));
                    NotificationPayload faultedNotification = _notifications[index];
                    _errors.Add(new NotificationDeliveryError(error, faultedNotification));
                    _current = index + 1;
                    _connected = false;
                    if (generalSwitch.TraceError)
                    {
                        Debug.WriteLine("Apns: ERROR: Apple rejected payload for device token : " + faultedNotification.DeviceToken);
                        Debug.WriteLine("Apns: ERROR: Apple Error code : " + error.ToString());
                        Debug.WriteLine("Apns: ERROR: Connection terminated by Apple.");
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                // This is how we "cancel" the asynchronous read, so just make sure
                // the channel must reconnect to try again.
                _connected = false;
            }
            catch (Exception ex)
            {
                _connected = false;
                _connectionReturnedError = true;
                if (generalSwitch.TraceError)
                {
                    Debug.WriteLine("Apns: ERROR: An error occurred while reading Apple response for token {0} - {1}", _notifications[payLoadIndex].DeviceToken, ex.Message);
                }
            }
            _manualResetEvent.Set();
        }

        private enum ConnectResult
        {
            Success,
            Failed,
            SSLError
        }

        private ConnectResult Connect(string host, int port, X509CertificateCollection certificates)
        {
            Disconnect();

            if (generalSwitch.TraceInfo)
            {
                Debug.WriteLine("Apns: Connecting to apple server.");
            }

            try
            {
                _apnsClient = new TcpClient();
                _apnsClient.Connect(host, port);
            }
            catch (SocketException ex)
            {
                if (generalSwitch.TraceError)
                {
                    Debug.WriteLine("Apns: ERROR: An error occurred while connecting to Apns servers - " + ex.Message);
                }
                return ConnectResult.Failed;
            }

            bool sslOpened = OpenSslStream(host, certificates);

            if (sslOpened)
            {
                _connected = true;
                if (generalSwitch.TraceInfo)
                {
                    Debug.WriteLine("Apns: Connected.");
                }
                return ConnectResult.Success;
            }

            return ConnectResult.SSLError;
        }

        private void Disconnect()
        {
            try
            {
                if (_apnsClient != null)
                {
                    _apnsClient.Close();
                    _apnsClient = null;
                }
                if (_apnsStream != null)
                {
                    _apnsStream.Close();
                    _apnsStream.Dispose();
                    _apnsStream = null;
                }
                _connected = false;
                if (generalSwitch.TraceInfo)
                {
                    Debug.WriteLine("Apns: Disconnected.");
                }
            }
            catch (Exception ex)
            {
                if (generalSwitch.TraceError)
                {
                    Debug.WriteLine("Apns: ERROR: An error occurred while disconnecting. - " + ex.Message);
                }
            }
        }

        private bool OpenSslStream(string host, X509CertificateCollection certificates)
        {
            if (generalSwitch.TraceInfo)
            {
                Debug.WriteLine("Apns: Creating SSL connection.");
            }
            _apnsStream = new SslStream(_apnsClient.GetStream(), false, validateServerCertificate, SelectLocalCertificate);

            try
            {
                _apnsStream.AuthenticateAsClient(host, certificates, System.Security.Authentication.SslProtocols.Tls, false);
            }
            catch (System.Security.Authentication.AuthenticationException ex)
            {
                if (generalSwitch.TraceError)
                {
                    Debug.WriteLine("Apns: ERROR: " + ex.Message + ". Inner: " + ex.InnerException.Message);
                }
                if (ex.InnerException != null && ex.InnerException.Message.IndexOf(@"revoked", StringComparison.InvariantCultureIgnoreCase) != -1)
                {
                    Debug.WriteLine(@"APNS: Certificate revoked!");
                }
                return false;
            }

            if (!_apnsStream.IsMutuallyAuthenticated)
            {
                if (generalSwitch.TraceError)
                {
                    Debug.WriteLine("Apns: ERROR: SSL Stream Failed to Authenticate");
                }
                return false;
            }

            if (!_apnsStream.CanWrite)
            {
                if (generalSwitch.TraceError)
                {
                    Debug.WriteLine("Apns: ERROR: SSL Stream is not Writable");
                }
                return false;
            }
            return true;
        }

        private bool validateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            return true; // Dont care about server's cert
        }

        private X509Certificate SelectLocalCertificate(object sender, string targetHost, X509CertificateCollection localCertificates, X509Certificate remoteCertificate, string[] acceptableIssuers)
        {
            return _certificate;
        }

        private static byte[] GeneratePayload(NotificationPayload payload)
        {
            try
            {
                if (payload.DeviceToken.Length != 64)
                {
                    if (generalSwitch.TraceError)
                    {
                        Debug.WriteLine("Apns: ERROR: Invalid device token length, possible simulator entry: " + payload.DeviceToken);
                    }
                    return null;
                }

                //convert Device token to HEX value.
                byte[] deviceToken = new byte[payload.DeviceToken.Length / 2];
                for (int i = 0; i < deviceToken.Length; i++)
                    deviceToken[i] = byte.Parse(payload.DeviceToken.Substring(i * 2, 2), System.Globalization.NumberStyles.HexNumber);

                var memoryStream = new MemoryStream();

                // Command
                memoryStream.WriteByte(0x01); // Enhanced notification format command

                //Adding ID to Payload
                byte[] payloadIdBytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(payload.PayloadId));
                memoryStream.Write(payloadIdBytes, 0, payloadIdBytes.Length);

                //Adding ExpiryDate to Payload
                int expiryTimeStamp = -1;
                if (payload.Expiry != DateTime.MinValue)
                {
                    TimeSpan epochTimeSpan = payload.Expiry - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    expiryTimeStamp = (int)epochTimeSpan.TotalSeconds;
                }
                byte[] expiry = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(expiryTimeStamp));
                memoryStream.Write(expiry, 0, expiry.Length);

                // device token length
                byte[] deviceTokenSize = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Convert.ToInt16(deviceToken.Length)));
                memoryStream.Write(deviceTokenSize, 0, 2);

                // Token
                memoryStream.Write(deviceToken, 0, deviceToken.Length);

                // Payload Json
                string apnMessage = payload.ToJson();
                if (generalSwitch.TraceInfo)
                {
                    Debug.WriteLine("Apns: Payload generated for " + payload.DeviceToken + " : " + apnMessage);
                }

                byte[] apnMessageBytes = Encoding.UTF8.GetBytes(apnMessage);

                // message length
                byte[] apnMessageLength = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Convert.ToInt16(apnMessageBytes.Length)));
                memoryStream.Write(apnMessageLength, 0, apnMessageLength.Length);

                // Write the message
                memoryStream.Write(apnMessageBytes, 0, apnMessageBytes.Length);
                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                if (generalSwitch.TraceError)
                {
                    Debug.WriteLine("Apns: ERROR: Unable to generate payload - " + ex.Message);
                }
                return null;
            }
        }

        public List<Feedback> GetFeedBack()
        {
            try
            {
                var feedbacks = new List<Feedback>();
                if (generalSwitch.TraceInfo)
                {
                    Debug.WriteLine("Apns: Connecting to feedback service.");
                }

                if (!_connected)
                {
                    Connect(_feedbackHost, FeedbackPort, _certificates);
                }

                if (_connected)
                {
                    //Set up
                    byte[] buffer = new byte[38];
                    int recd = 0;
                    DateTime minTimestamp = DateTime.Now.AddYears(-1);

                    //Get the first feedback
                    recd = _apnsStream.Read(buffer, 0, buffer.Length);
                    if (generalSwitch.TraceInfo)
                    {
                        Debug.WriteLine("Apns: Feedback response received.");
                    }
                    if (recd == 0 && generalSwitch.TraceWarning)
                    {
                        Debug.WriteLine("Apns: Feedback response is empty.");
                    }

                    //Continue while we have results and are not disposing
                    while (recd > 0)
                    {
                        if (generalSwitch.TraceInfo)
                        {
                            Debug.WriteLine("Apns: processing feedback response");
                        }
                        var fb = new Feedback();

                        //Get our seconds since 1970 ?
                        byte[] bSeconds = new byte[4];
                        byte[] bDeviceToken = new byte[32];

                        Array.Copy(buffer, 0, bSeconds, 0, 4);

                        //Check endianness
                        if (BitConverter.IsLittleEndian)
                            Array.Reverse(bSeconds);

                        int tSeconds = BitConverter.ToInt32(bSeconds, 0);

                        //Add seconds since 1970 to that date, in UTC and then get it locally
                        fb.Timestamp = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(tSeconds).ToLocalTime();


                        //Now copy out the device token
                        Array.Copy(buffer, 6, bDeviceToken, 0, 32);

                        fb.DeviceToken = BitConverter.ToString(bDeviceToken).Replace("-", "").ToLower().Trim();

                        //Make sure we have a good feedback tuple
                        if (fb.DeviceToken.Length == 64 && fb.Timestamp > minTimestamp)
                        {
                            //Raise event
                            //this.Feedback(this, fb);
                            feedbacks.Add(fb);
                        }

                        //Clear our array to reuse it
                        Array.Clear(buffer, 0, buffer.Length);

                        //Read the next feedback
                        recd = _apnsStream.Read(buffer, 0, buffer.Length);
                    }
                    // close the connection here !
                    Disconnect();
                    if (feedbacks.Count > 0)
                    {
                        if (generalSwitch.TraceInfo)
                        {
                            Debug.WriteLine("Apns: Total {0} feedbacks received.", feedbacks.Count);
                        }
                    }
                    return feedbacks;
                }
            }
            catch (Exception ex)
            {
                if (generalSwitch.TraceError)
                {
                    Debug.WriteLine("Apns: ERROR: Error occurred on receiving feed back. - " + ex.Message);
                }
                return null;
            }
            return null;
        }
    }

    public class MyAsyncInfo
    {
        public Byte[] ReadBuffer { get; set; }
        public SslStream MyStream { get; set; }

        public MyAsyncInfo(SslStream stream)
        {
            ReadBuffer = new byte[6];
            MyStream = stream;
        }
    }
}
