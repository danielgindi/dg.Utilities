using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using dg.Utilities.Threading;
using System.Net;

namespace dg.Utilities.GoogleCloudMessaging
{
    public class HttpNotificationService : IDisposable
    {
        private int GCM_TIMER_MS = 500;

        public string GcmApiKey
        {
            get { return notificationSender.GcmApiKey; }
            set { notificationSender.GcmApiKey = value; }
        }
        public string GcmAccessPointUrl
        {
            get { return notificationSender.GcmAccessPointUrl; }
            set { notificationSender.GcmAccessPointUrl = value; }
        }

        private SingleThreadedTimer timer = null;
        private ConcurrentQueue<HttpNotificationPayload> currentQueue = new ConcurrentQueue<HttpNotificationPayload>();
        private NotificationSender notificationSender = new NotificationSender();

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
            }
            // Now clean up Native Resources (Pointers)
        }
        #endregion

        public HttpNotificationService(string GcmApiKey, int TimerStepMilliseconds)
        {
            notificationSender.GcmApiKey = GcmApiKey;
            GCM_TIMER_MS = TimerStepMilliseconds;

            timer = new SingleThreadedTimer();
            timer.Milliseconds = GCM_TIMER_MS;
            timer.Elapsed += DoSendGcmQueue;
        }
        public delegate void GcmResultDelegate(NotificationDeliveryResult result);

        public void SendMessage(HttpNotificationPayload Payload, GcmResultDelegate Result)
        {
            Payload.ContextData = Result;
            currentQueue.Enqueue(Payload);

            if (!timer.IsStarted)
            {
                timer.Start();
            }
        }
        public void SendMessage(string RegistrationId, GcmResultDelegate Result)
        {
            HttpNotificationPayload payload = new HttpNotificationPayload(RegistrationId);
            SendMessage(payload, Result);
        }
        public void SendMessage(string[] RegistrationIds, GcmResultDelegate Result)
        {
            HttpNotificationPayload payload = new HttpNotificationPayload(RegistrationIds);
            SendMessage(payload, Result);
        }
        public void SendMessage(string RegistrationId, Dictionary<string, object[]> Data, GcmResultDelegate Result)
        {
            HttpNotificationPayload payload = new HttpNotificationPayload(RegistrationId);
            if (Data != null)
            {
                foreach (string key in Data.Keys)
                {
                    payload.AddCustom(key, Data[key]);
                }
            }
            SendMessage(payload, Result);
        }
        public void SendMessage(string[] RegistrationIds, Dictionary<string, object[]> Data, GcmResultDelegate Result)
        {
            HttpNotificationPayload payload = new HttpNotificationPayload(RegistrationIds);
            if (Data != null)
            {
                foreach (string key in Data.Keys)
                {
                    payload.AddCustom(key, Data[key]);
                }
            }
            SendMessage(payload, Result);
        }

        private void DoSendGcmQueue(Object sender, EventArgs e)
        {
            ConcurrentQueue<HttpNotificationPayload> failedQueue = new ConcurrentQueue<HttpNotificationPayload>();

            SendGcmQueue(currentQueue, ref failedQueue);

            if (failedQueue.Count > 0)
            {
                HttpNotificationPayload payload = null;
                while (failedQueue.TryDequeue(out payload))
                {
                    // Return these to the end of the queue to try next time
                    currentQueue.Enqueue(payload);
                }
            }
        }

        private void SendGcmQueue(ConcurrentQueue<HttpNotificationPayload> queue, ref ConcurrentQueue<HttpNotificationPayload> failedQueue)
        {
            if (queue.Count == 0) return;

            List<NotificationDeliveryResult> results = new List<NotificationDeliveryResult>();

            HttpNotificationPayload payload = null;
            while (queue.TryDequeue(out payload))
            {
                try
                {
                    NotificationDeliveryResult result;

                    result = notificationSender.SendHttpPayload(payload);
                    results.Add(result);

                    if (result.HttpStatusCode != HttpStatusCode.OK && result.HttpStatusCode != HttpStatusCode.Unauthorized)
                    {
                        // Return to end of the queue to try next time
                        failedQueue.Enqueue(payload);
                    }

                    foreach (NotificationDeliveryResult aResult in results)
                    {
                        if (aResult.Payload.ContextData is GcmResultDelegate)
                        {
                            ((GcmResultDelegate)aResult.Payload.ContextData)(aResult);
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
