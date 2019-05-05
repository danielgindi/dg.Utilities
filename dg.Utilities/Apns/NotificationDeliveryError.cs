using System;

namespace dg.Utilities.Apns
{
    public class NotificationDeliveryError
    {
        public NotificationDeliveryError(DeliveryErrorType type, NotificationPayload payload)
        {
            this.ErrorType = type;
            this.Payload = payload;
        }

        public NotificationDeliveryError(Exception exception, NotificationPayload payload)
        {
            this.ErrorType = DeliveryErrorType.Unknown;
            this.Exception = exception;
            this.Payload = payload;
        }

        public DeliveryErrorType ErrorType { get; private set; }
        public NotificationPayload Payload { get; private set; }
        public Exception Exception { get; private set; }

        public bool IsException
        {
            get { return Exception != null; }
        }
    }

    public enum DeliveryErrorType : byte
    {
        NoErrors = 0,
        ProcessingError = 1,
        MissingDeviceToken = 2,
        MissingTopic = 3,
        MissingPayload = 4,
        InvalidTokenSize = 5,
        InvalidTopicSize = 6,
        InvalidPayloadSize = 7,
        InvalidToken = 8,
        Unknown = 255,
    }
}
