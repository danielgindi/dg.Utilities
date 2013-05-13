using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

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
                if (!string.IsNullOrEmpty(this.Alert.Body)
                    && string.IsNullOrEmpty(this.Alert.LocalizedKey)
                    && string.IsNullOrEmpty(this.Alert.ActionLocalizedKey)
                    && string.IsNullOrEmpty(this.Alert.LaunchImage)
                    && (this.Alert.LocalizedArgs == null || this.Alert.LocalizedArgs.Count <= 0))
                {
                    json.Append(@"""alert"":");
                    AppendJValue(json, this.Alert.Body);
                }
                else
                {
                    json.Append(@"""alert"":{");

                    bool alertFirst = true;

                    if (!string.IsNullOrEmpty(this.Alert.LocalizedKey))
                    {
                        if (!alertFirst) json.Append(','); else alertFirst = false;
                        json.Append(@"""loc-key"":");
                        AppendJValue(json, this.Alert.LocalizedKey);
                    }

                    if (this.Alert.LocalizedArgs != null && this.Alert.LocalizedArgs.Count > 0)
                    {
                        if (!alertFirst) json.Append(','); else alertFirst = false;
                        json.Append(@"""loc-args"":");
                        AppendJArray(json, this.Alert.LocalizedArgs.ToArray());
                    }

                    if (!string.IsNullOrEmpty(this.Alert.Body))
                    {
                        if (!alertFirst) json.Append(','); else alertFirst = false;
                        json.Append(@"""body"":");
                        AppendJValue(json, this.Alert.Body);
                    }

                    if (!string.IsNullOrEmpty(this.Alert.LaunchImage))
                    {
                        if (!alertFirst) json.Append(','); else alertFirst = false;
                        json.Append(@"""launch-image"":");
                        AppendJValue(json, this.Alert.LaunchImage);
                    }

                    if (!string.IsNullOrEmpty(this.Alert.ActionLocalizedKey))
                    {
                        if (!alertFirst) json.Append(','); else alertFirst = false;
                        json.Append(@"""action-loc-key"":");
                        AppendJValue(json, this.Alert.ActionLocalizedKey);
                    }

                    json.Append('}');
                }
            }

            if (this.Badge.HasValue)
            {
                if (!apsFirst) json.Append(','); else apsFirst = false;
                json.Append(@"""badge"":");
                AppendJValue(json, this.Badge.Value);
            }

            if (!string.IsNullOrEmpty(this.Sound))
            {
                if (!apsFirst) json.Append(','); else apsFirst = false;
                json.Append(@"""sound"":");
                AppendJValue(json, this.Sound);
            }

            json.Append('}'); // aps

            foreach (string key in this.CustomItems.Keys)
            {
                json.Append(@",""");
                json.Append(key);
                json.Append(@""":");
                if (this.CustomItems[key].Length == 1)
                {
                    AppendJValue(json, this.CustomItems[key][0]);
                }
                else if (this.CustomItems[key].Length > 1)
                {
                    AppendJArray(json, this.CustomItems[key]);
                }
            }

            json.Append('}'); // main object

            string rawString = json.ToString();

            StringBuilder encodedString = new StringBuilder();
            foreach (char c in rawString)
            {
                if ((int)c < 32 || (int)c > 127)
                    encodedString.Append("\\u" + String.Format("{0:x4}", Convert.ToUInt32(c)));
                else
                    encodedString.Append(c);
            }
            return rawString;// encodedString.ToString();
        }
        protected void AppendJArray(StringBuilder sb, object[] array)
        {
            bool first = true;
            sb.Append('[');
            foreach (object arg in array)
            {
                if (!first) sb.Append(','); else first = false;
                AppendJValue(sb, arg);
            }
            sb.Append(']');
        }
        protected void AppendJValue(StringBuilder sb, object value)
        {
            if (value is string)
            {
                sb.Append(((string)value).ToJavaScript('"', true));
            }
            else
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, @"{0}", value);
            }
        }

        public override string ToString()
        {
            return ToJson();
        }
    }
}
