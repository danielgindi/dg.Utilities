using System;
using System.Collections.Generic;
using dg.Utilities.Threading;
using System.Collections.Concurrent;

namespace dg.Utilities.Apns
{
    public class NotificationService : IDisposable
    {
        private bool APNS_Sandbox = false;
        private string APNS_P12 = null;
        private string APNS_P12PWD = null;
        private int APNS_TIMER_MS = 500;

        private SingleThreadedTimer timer = null;
        private ConcurrentQueue<NotificationPayload> currentQueue = new ConcurrentQueue<NotificationPayload>();
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
            timer.Milliseconds = APNS_TIMER_MS;
            timer.Elapsed += DoSendApnsQueue;
        }

        public delegate void APNSErrorDelegate();

        public void SendMessage(string deviceToken, string message, int? badge, string sound, APNSErrorDelegate rejected)
        {
            NotificationPayload payload = new NotificationPayload(deviceToken, message, badge, sound);
            payload.ContextData = rejected;
            currentQueue.Enqueue(payload);

            if (!timer.IsStarted)
            {
                timer.Start();
            }
        }

        public void SendMessage(string deviceToken, NotificationAlert alert, int? badge, string sound, APNSErrorDelegate rejected)
        {
            SendMessage(deviceToken, alert, badge, sound, null, rejected);
        }

        public void SendMessage(string deviceToken, NotificationAlert alert, int? badge, string sound, Dictionary<string, object[]> custom_items, APNSErrorDelegate rejected)
        {
            NotificationPayload payload = new NotificationPayload(deviceToken, alert, badge, sound);
            payload.ContextData = rejected;
            if (custom_items != null)
            {
                foreach (string key in custom_items.Keys)
                {
                    payload.AddCustom(key, custom_items[key]);
                }
            }
            currentQueue.Enqueue(payload);

            if (!timer.IsStarted)
            {
                timer.Start();
            }
        }

        private void DoSendApnsQueue(Object sender, EventArgs e)
        {
            ConcurrentQueue<NotificationPayload> failedQueue = new ConcurrentQueue<NotificationPayload>();

            SendApnsQueue(currentQueue, ref failedQueue);

            if (failedQueue.Count > 0)
            {
                NotificationPayload payload = null;
                while (failedQueue.TryDequeue(out payload))
                {
                    // Return these to the end of the queue to try next time
                    currentQueue.Enqueue(payload);
                }
            }
        }

        private void SendApnsQueue(ConcurrentQueue<NotificationPayload> queue, ref ConcurrentQueue<NotificationPayload> failedQueue)
        {
            if (queue.Count == 0) return;

            try
            {
                if (notificationChannel == null)
                {
                    notificationChannel = new NotificationChannel(APNS_Sandbox, APNS_P12, APNS_P12PWD);
                }
            }
            catch
            {
            }

            List<NotificationPayload> payloads = new List<NotificationPayload>();

            NotificationPayload payload;
            while (queue.TryDequeue(out payload))
            {
                payloads.Add(payload);
            }

            if (notificationChannel != null)
            {
                try
                {
                    List<NotificationDeliveryError> errors = notificationChannel.SendToApple(payloads);
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
                    // Return to end of the queue to try next time
                    failedQueue.Enqueue(payload);
                }
            }
        }
    }
}
