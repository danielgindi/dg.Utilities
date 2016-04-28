using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace dg.Utilities.GoogleCloudMessaging
{
    public class HttpNotificationPayload
    {
        /// <summary>
        /// A string array with the list of devices (registration IDs) receiving the message. It must contain at least 1 and at most 1000 registration IDs. To send a multicast message, you must use JSON. For sending a single message to a single device, you could use a JSON object with just 1 registration id, or plain text (see below). A request must include a recipient—this can be either a registration ID, an array of registration IDs, or a notification_key. Required.
        /// </summary>
        public string[] RegistrationIds { get; set; }

        /// <summary>
        /// A string that maps a single user to multiple registration IDs associated with that user. This allows a 3rd-party server to send a single message to multiple app instances (typically on multiple devices) owned by a single user. A 3rd-party server can use notification_key as the target for a message instead of an individual registration ID (or array of registration IDs). The maximum number of members allowed for a notification_key is 10. For more discussion of this topic, see <see cref="http://developer.android.com/google/gcm/notifications.html">User Notifications</see>. Optional.
        /// </summary>
        public string NotificationKey { get; set; }

        /// <summary>
        /// An arbitrary string (such as "Updates Available") that is used to collapse a group of like messages when the device is offline, so that only the last message gets sent to the client. This is intended to avoid sending too many messages to the phone when it comes back online. Note that since there is no guarantee of the order in which messages get sent, the "last" message may not actually be the last message sent by the application server. Collapse keys are also called <see cref="http://developer.android.com/google/gcm/server.html#s2s">send-to-sync messages</see>. 
        /// Note: GCM allows a maximum of 4 different collapse keys to be used by the GCM server at any given time. In other words, the GCM server can simultaneously store 4 different send-to-sync messages per device, each with a different collapse key. If you exceed this number GCM will only keep 4 collapse keys, with no guarantees about which ones they will be. See <see cref="http://developer.android.com/google/gcm/adv.html#collapsible">Advanced Topics</see> for more discussion of this topic. Optional.
        /// </summary>
        public string CollapseKey { get; set; }

        /// <summary>
        /// A JSON object whose fields represents the key-value pairs of the message's payload data. If present, the payload data it will be included in the Intent as application data, with the key being the extra's name. For instance, "data":{"score":"3x1"} would result in an intent extra named score whose value is the string 3x1. There is no limit on the number of key/value pairs, though there is a limit on the total size of the message (4kb). The values could be any JSON object, but we recommend using strings, since the values will be converted to strings in the GCM server anyway. If you want to include objects or other non-string data types (such as integers or booleans), you have to do the conversion to string yourself. Also note that the key cannot be a reserved word (from or any word starting with google.). To complicate things slightly, there are some reserved words (such as collapse_key) that are technically allowed in payload data. However, if the request also contains the word, the value in the request will overwrite the value in the payload data. Hence using words that are defined as field names in this table is not recommended, even in cases where they are technically allowed. Optional.
        /// </summary>
        public Dictionary<string, object[]> Data
        {
            get;
            private set;
        }

        /// <summary>
        /// If included, indicates that the message should not be sent immediately if the device is idle. The server will wait for the device to become active, and then only the last message for each collapse_key value will be sent. The default value is false. Optional.
        /// </summary>
        public bool? DelayWhileIdle { get; set; }

        /// <summary>
        /// How long (in seconds) the message should be kept on GCM storage if the device is offline. Optional (default time-to-live is 4 weeks).
        /// </summary>
        public int? TimeToLive { get; set; }

        /// <summary>
        /// A string containing the package name of your application. When set, messages will only be sent to registration IDs that match the package name. Optional.
        /// </summary>
        public string RestrictedPackageName { get; set; }

        /// <summary>
        /// If included, allows developers to test their request without actually sending a message. Optional. The default value is false.
        /// </summary>
        public bool? DryRun { get; set; }

        /// <summary>
        /// Not for GCM. This is data to be associated with this instance.
        /// </summary>
        public object ContextData { get; set; }

        public HttpNotificationPayload(string registrationId)
        {
            RegistrationIds = new string[] { registrationId };
        }

        public HttpNotificationPayload(string[] registrationIds)
        {
            RegistrationIds = registrationIds;
        }
        
        public void AddCustom(string key, params object[] values)
        {
            if (Data == null) Data = new Dictionary<string, object[]>();
            if (values != null) Data.Add(key, values);
        }

        public string ToJson()
        {
            StringBuilder json = new StringBuilder();
            json.Append('{'); // main object

            bool first = true;

            if (RegistrationIds != null)
            {
                if (!first) json.Append(','); else first = false;
                json.Append(@"""registration_ids"":");
                JsonHelper.WriteValue(RegistrationIds, json);
            }

            if (NotificationKey != null)
            {
                if (!first) json.Append(','); else first = false;
                json.Append(@"""notification_key"":");
                JsonHelper.WriteValue(NotificationKey, json);
            }

            if (CollapseKey != null)
            {
                if (!first) json.Append(','); else first = false;
                json.Append(@"""collapse_key"":");
                JsonHelper.WriteValue(CollapseKey, json);
            }

            if (Data != null)
            {
                if (!first) json.Append(',');
                else first = false;

                json.Append(@"""data"":{");

                bool subFirst = true;
                foreach (string key in Data.Keys)
                {
                    if (!subFirst) json.Append(',');
                    else subFirst = false;
                    
                    JsonHelper.WriteValue(key, json);
                    json.Append(':');
                    if (Data[key].Length == 1)
                    {
                        JsonHelper.WriteValue(Data[key][0], json);
                    }
                    else if (Data[key].Length > 1)
                    {
                        JsonHelper.WriteValue(Data[key], json);
                    }
                }

                json.Append(@"}");
            }

            if (DelayWhileIdle != null)
            {
                if (!first) json.Append(',');
                else first = false;

                json.Append(@"""delay_while_idle"":");
                JsonHelper.WriteValue(DelayWhileIdle.Value, json);
            }

            if (TimeToLive != null)
            {
                if (!first) json.Append(',');
                else first = false;

                json.Append(@"""time_to_live"":");
                JsonHelper.WriteValue(TimeToLive.Value, json);
            }

            if (RestrictedPackageName != null)
            {
                if (!first) json.Append(',');
                else first = false;

                json.Append(@"""restricted_package_name"":");
                JsonHelper.WriteValue(RestrictedPackageName, json);
            }

            if (DryRun != null)
            {
                if (!first) json.Append(',');
                else first = false;

                json.Append(@"""dry_run"":");
                JsonHelper.WriteValue(DryRun.Value, json);
            }
            
            json.Append('}'); // main object
            
            return json.ToString();
        }
        
        public override string ToString()
        {
            return ToJson();
        }
    }
}
