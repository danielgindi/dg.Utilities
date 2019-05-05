using System;
using System.IO;
using System.Text;
using System.Net;

namespace dg.Utilities.GoogleCloudMessaging
{
    public class NotificationSender
    {
        public string GcmApiKey { get; set; }

        public string GcmAccessPointUrl = @"https://android.googleapis.com/gcm/send";

        public NotificationDeliveryResult SendHttpPayload(HttpNotificationPayload Payload)
        {
            WebRequest webRequest = WebRequest.Create(GcmAccessPointUrl);
            webRequest.Method = "POST";
            webRequest.ContentType = "application/json";
            webRequest.Headers.Add("Authorization", "key=" + GcmApiKey);
            Byte[] byteArray = Encoding.UTF8.GetBytes(Payload.ToJson());
            webRequest.ContentLength = byteArray.Length;

            using (Stream dataStream = webRequest.GetRequestStream())
            {
                dataStream.Write(byteArray, 0, byteArray.Length);
            }

            HttpWebResponse webResponse = null;
            NotificationDeliveryResult result = null;

            try
            {
                webResponse = webRequest.GetResponse() as HttpWebResponse;
            }
            catch (WebException ex)
            {
                webResponse = ex.Response as HttpWebResponse;
            }
            finally
            {
                if (webResponse != null)
                {
                    try
                    {
                        using (Stream responseStream = webResponse.GetResponseStream())
                        {
                            using (StreamReader streamReader = new StreamReader(responseStream))
                            {
                                string responseText = streamReader.ReadToEnd();

                                switch (webResponse.StatusCode)
                                {
                                    default:
                                    case HttpStatusCode.OK: // Should parse JSON result
                                    case HttpStatusCode.BadRequest: // JSON malformed
                                    case HttpStatusCode.Unauthorized: // API Key unauthorized
                                        result = new NotificationDeliveryResult(Payload, webResponse.StatusCode, responseText);
                                        break;
                                }
                            }
                        }
                    }
                    finally
                    {
                        webResponse.Close();
                    }
                }
            }

            return result;
        }
    }
}