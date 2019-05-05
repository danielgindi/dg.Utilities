using System;
using System.Collections.Generic;
using System.Text;

namespace dg.Utilities.Apns
{
    public class NotificationPayload
    {
        public NotificationAlert Alert { get; set; }

        public string DeviceToken { get; set; }

        public int? Badge { get; set; }

        public string Sound { get; set; }

        public DateTime Expiry { get; set; }

        public object ContextData { get; set; }

        public bool ContentAvailable { get; set; }

        internal int PayloadId { get; set; }

        public Dictionary<string, object[]> CustomItems
        {
            get;
            private set;
        }

        public NotificationPayload(string deviceToken)
        {
            DeviceToken = deviceToken;
            Alert = new NotificationAlert();
            CustomItems = new Dictionary<string, object[]>();
            Expiry = DateTime.UtcNow.AddMonths(1);
        }

        public NotificationPayload(string deviceToken, string alert)
        {
            DeviceToken = deviceToken;
            Alert = new NotificationAlert() { Body = alert };
            CustomItems = new Dictionary<string, object[]>();
            Expiry = DateTime.UtcNow.AddMonths(1);
        }

        public NotificationPayload(string deviceToken, string alert, int? badge)
        {
            DeviceToken = deviceToken;
            Alert = new NotificationAlert() { Body = alert };
            Badge = badge;
            CustomItems = new Dictionary<string, object[]>();
            Expiry = DateTime.UtcNow.AddMonths(1);
        }

        public NotificationPayload(string deviceToken, string alert, int? badge, string sound)
        {
            DeviceToken = deviceToken;
            Alert = new NotificationAlert() { Body = alert };
            Badge = badge;
            Sound = sound;
            CustomItems = new Dictionary<string, object[]>();
            Expiry = DateTime.UtcNow.AddMonths(1);
        }

        public NotificationPayload(string deviceToken, NotificationAlert alert, int? badge, string sound)
        {
            DeviceToken = deviceToken;
            Alert = alert;
            Badge = badge;
            Sound = sound;
            CustomItems = new Dictionary<string, object[]>();
            Expiry = DateTime.UtcNow.AddMonths(1);
        }

        public void AddCustom(string key, params object[] values)
        {
            if (values != null)
                this.CustomItems.Add(key, values);
        }

        public string ToJson()
        {
            StringBuilder json = new StringBuilder();
            json.Append('{'); // main object
            json.Append(@"""aps"":{"); // aps

            bool apsFirst = true;

            if (!this.Alert.IsEmpty)
            {
                if (!apsFirst) json.Append(','); else apsFirst = false;

                json.Append(@"""alert"":{");

                bool alertFirst = true;

                if (!string.IsNullOrEmpty(this.Alert.TitleLocalizedKey))
                {
                    if (!alertFirst) json.Append(','); else alertFirst = false;
                    json.Append(@"""title-loc-key"":");
                    JsonHelper.WriteValue(this.Alert.TitleLocalizedKey, json);
                }

                if (this.Alert.TitleLocalizedArgs != null && this.Alert.TitleLocalizedArgs.Count > 0)
                {
                    if (!alertFirst) json.Append(','); else alertFirst = false;
                    json.Append(@"""title-loc-args"":");
                    JsonHelper.WriteValue(this.Alert.TitleLocalizedArgs.ToArray(), json);
                }

                if (!string.IsNullOrEmpty(this.Alert.LocalizedKey))
                {
                    if (!alertFirst) json.Append(','); else alertFirst = false;
                    json.Append(@"""loc-key"":");
                    JsonHelper.WriteValue(this.Alert.LocalizedKey, json);
                }

                if (this.Alert.LocalizedArgs != null && this.Alert.LocalizedArgs.Count > 0)
                {
                    if (!alertFirst) json.Append(','); else alertFirst = false;
                    json.Append(@"""loc-args"":");
                    JsonHelper.WriteValue(this.Alert.LocalizedArgs.ToArray(), json);
                }

                if (!string.IsNullOrEmpty(this.Alert.Title))
                {
                    if (!alertFirst) json.Append(','); else alertFirst = false;
                    json.Append(@"""title"":");
                    JsonHelper.WriteValue(this.Alert.Title, json);
                }

                if (!string.IsNullOrEmpty(this.Alert.Body))
                {
                    if (!alertFirst) json.Append(','); else alertFirst = false;
                    json.Append(@"""body"":");
                    JsonHelper.WriteValue(this.Alert.Body, json);
                }

                if (!string.IsNullOrEmpty(this.Alert.LaunchImage))
                {
                    if (!alertFirst) json.Append(','); else alertFirst = false;
                    json.Append(@"""launch-image"":");
                    JsonHelper.WriteValue(this.Alert.LaunchImage, json);
                }

                if (!string.IsNullOrEmpty(this.Alert.ActionLocalizedKey))
                {
                    if (!alertFirst) json.Append(','); else alertFirst = false;
                    json.Append(@"""action-loc-key"":");
                    JsonHelper.WriteValue(this.Alert.ActionLocalizedKey, json);
                }

                json.Append('}');

            }

            if (this.Badge.HasValue)
            {
                if (!apsFirst) json.Append(','); else apsFirst = false;
                json.Append(@"""badge"":");
                JsonHelper.WriteValue(this.Badge.Value, json);
            }

            if (!string.IsNullOrEmpty(this.Sound))
            {
                if (!apsFirst) json.Append(','); else apsFirst = false;
                json.Append(@"""sound"":");
                JsonHelper.WriteValue(this.Sound, json);
            }

            if (this.ContentAvailable)
            {
                if (!apsFirst) json.Append(','); else apsFirst = false;
                json.Append(@"""content-available"":");
                JsonHelper.WriteValue(1, json);
            }

            json.Append('}'); // aps

            foreach (string key in this.CustomItems.Keys)
            {
                json.Append(@",""");
                json.Append(key);
                json.Append(@""":");
                if (this.CustomItems[key].Length == 1)
                {
                    JsonHelper.WriteValue(this.CustomItems[key][0], json);
                }
                else if (this.CustomItems[key].Length > 1)
                {
                    JsonHelper.WriteValue(this.CustomItems[key], json);
                }
            }

            json.Append('}'); // main object

            string rawString = json.ToString();

            /*StringBuilder encodedString = new StringBuilder();
            foreach (char c in rawString)
            {
                if ((int)c < 32 || (int)c > 127)
                    encodedString.Append("\\u" + String.Format("{0:x4}", Convert.ToUInt32(c)));
                else
                    encodedString.Append(c);
            }*/
            return rawString;// encodedString.ToString();
        }

        public override string ToString()
        {
            return ToJson();
        }
    }
}
