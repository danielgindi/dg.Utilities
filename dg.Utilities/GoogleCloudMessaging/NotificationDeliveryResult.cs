using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace dg.Utilities.GoogleCloudMessaging
{
    public class NotificationDeliveryResult
    {
        public NotificationDeliveryResult(HttpNotificationPayload Payload, HttpStatusCode HttpStatusCode, string Response)
        {
            this.Payload = Payload;
            this.HttpStatusCode = HttpStatusCode;
            this.Response = Response;
        }

        public HttpStatusCode HttpStatusCode { get; private set; }
        public HttpNotificationPayload Payload { get; private set; }
        public string Response { get; private set; }
    }
}
