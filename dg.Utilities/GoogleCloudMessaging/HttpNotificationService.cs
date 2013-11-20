using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Threading;
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

        private SingleThreadedTimer timer = new SingleThreadedTimer();
        private List<HttpNotificationPayload> currentQueue = new List<HttpNotificationPayload>();
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
        }
        public delegate void GcmResultDelegate(NotificationDeliveryResult result);

        public void SendMessage(HttpNotificationPayload Payload, GcmResultDelegate Result)
        {
            lock (currentQueue)
            {
                Payload.ContextData = Result;
                currentQueue.Add(Payload);
            }
            lock (timer)
            {
                if (!timer.IsStarted)
                {
                    timer.Milliseconds = GCM_TIMER_MS;
                    timer.Elapsed += DoSendApnsQueue;
                    timer.Start();
                }
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

        private void DoSendApnsQueue(Object sender, EventArgs e)
        {
            List<HttpNotificationPayload> queue = null;
            lock (currentQueue)
            {
                queue = currentQueue;
                currentQueue = new List<HttpNotificationPayload>();
            }
            if (queue.Count > 0)
            {
                try
                {
                    List<NotificationDeliveryResult> results = new List<NotificationDeliveryResult>();
                    NotificationDeliveryResult result;
                    foreach (HttpNotificationPayload payload in queue)
                    {
                        result = notificationSender.SendHttpPayload(payload);
                        results.Add(result);

                        if (result.HttpStatusCode != HttpStatusCode.OK && result.HttpStatusCode != HttpStatusCode.Unauthorized)
                        {
                            lock (currentQueue)
                            { // Return to end of the queue to try next time
                                currentQueue.Add(payload);
                            }
                        }
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
