using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Threading;
using dg.Utilities.Threading;

namespace dg.Utilities.Apns
{
    public class NotificationService : IDisposable
    {
        private bool APNS_Sandbox = false;
        private string APNS_P12 = null;
        private string APNS_P12PWD = null;
        private int APNS_TIMER_MS = 500;

        private SingleThreadedTimer timer = null;
        private List<NotificationPayload> currentQueue = null;
        private Mutex notificationChannelMutex = new Mutex();
        private NotificationChannel notificationChannel = null;

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
                if (timer != null)
                {
                    timer.Dispose();
                    timer = null;
                }
                if (notificationChannel != null)
                {
                    notificationChannel.Dispose();
                    notificationChannel = null;
                }
                if (notificationChannelMutex != null)
                {
                    notificationChannelMutex.Dispose();
                    notificationChannelMutex = null;
                }
            }
            // Now clean up Native Resources (Pointers)
        }
        #endregion

        public NotificationService(bool Sandbox, string P12path, string P12Password, int TimerStepMilliseconds)
        {
            APNS_Sandbox = Sandbox;
            APNS_P12 = P12path;
            APNS_P12PWD = P12Password;
            APNS_TIMER_MS = TimerStepMilliseconds;

            timer = new SingleThreadedTimer();
            currentQueue = new List<NotificationPayload>();
        }
        public delegate void APNSErrorDelegate();

        public void SendMessage(string DeviceToken, string Message, int? Badge, string Sound, APNSErrorDelegate Rejected)
        {
            lock (currentQueue)
            {
                NotificationPayload payload = new NotificationPayload(DeviceToken, Message, Badge, Sound);
                payload.ContextData = Rejected;
                currentQueue.Add(payload);
            }
            lock (timer)
            {
                if (!timer.IsStarted)
                {
                    timer.Milliseconds = APNS_TIMER_MS;
                    timer.Elapsed += DoSendApnsQueue;
                    timer.Start();
                }
            }
        }
        public void SendMessage(string DeviceToken, NotificationAlert Alert, int? Badge, string Sound, APNSErrorDelegate Rejected)
        {
            SendMessage(DeviceToken, Alert, Badge, Sound, null, Rejected);
        }
        public void SendMessage(string DeviceToken, NotificationAlert Alert, int? Badge, string Sound, Dictionary<string, object[]> CustomItems, APNSErrorDelegate Rejected)
        {
            lock (currentQueue)
            {
                NotificationPayload payload = new NotificationPayload(DeviceToken, Alert, Badge, Sound);
                payload.ContextData = Rejected;
                if (CustomItems != null)
                {
                    foreach (string key in CustomItems.Keys)
                    {
                        payload.AddCustom(key, CustomItems[key]);
                    }
                }
                currentQueue.Add(payload);
            }
            lock (timer)
            {
                if (!timer.IsStarted)
                {
                    timer.Milliseconds = APNS_TIMER_MS;
                    timer.Elapsed += DoSendApnsQueue;
                    timer.Start();
                }
            }
        }
        private void DoSendApnsQueue(Object sender, EventArgs e)
        {
            List<NotificationPayload> queue = null;
            lock (currentQueue)
            {
                queue = currentQueue;
                currentQueue = new List<NotificationPayload>();
            }
            if (queue.Count > 0)
            {
                notificationChannelMutex.WaitOne();
                try
                {
                    if (notificationChannel == null) notificationChannel = new NotificationChannel(APNS_Sandbox, APNS_P12, APNS_P12PWD);
                }
                catch
                {
                    lock (currentQueue)
                    {
                        currentQueue.InsertRange(0, queue);
                    }
                }
                notificationChannelMutex.ReleaseMutex();

                if (notificationChannel != null)
                {
                    try
                    {
                        List<NotificationDeliveryError> errors = notificationChannel.SendToApple(queue);
                        NotificationPayload payload;
                        foreach (NotificationDeliveryError error in errors)
                        {
                            payload = error.Payload;
                            if (payload != null)
                            {
                                if (payload.ContextData != null && payload.ContextData is APNSErrorDelegate)
                                {
                                    ((APNSErrorDelegate)payload.ContextData)();
                                }
                            }
                        }
                    }
                    catch
                    {
                    }
                }
            }
            lock (timer)
            {
                lock (currentQueue)
                {
                    if (currentQueue.Count == 0 && timer.IsStarted)
                    {
                        timer.Stop();
                    }
                }
            }
        }
    }
}
