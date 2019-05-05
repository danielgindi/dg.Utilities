using System;
using System.Xml.Serialization;

namespace dg.Utilities.Serialization
{
    [XmlRoot("AttributesContainer")]
    public class AttributesContainer : SerializableDictionary<string, string>
    {
        public static AttributesContainer NewFromXml(string xml)
        {
            return XmlSerializerHelper.DeserializeObject(xml, typeof(AttributesContainer).AssemblyQualifiedName, false) as AttributesContainer;
        }
        public static AttributesContainer NewOrEmptyFromXml(string xml)
        {
            AttributesContainer ls = NewFromXml(xml);
            if (ls == null) ls = new AttributesContainer();
            return ls;
        }
        public string ToXml()
        {
            return XmlSerializerHelper.SerializeObject(this, typeof(AttributesContainer));
        }
        public override string ToString()
        {
            return ToXml();
        }

        public string GetOrEmpty(string key)
        {
            string value;
            if (!this.TryGetValue(key, out value) || value == null) value = string.Empty;
            return value;
        }
        public string GetOrNull(string key)
        {
            string value;
            if (!this.TryGetValue(key, out value)) value = null;
            return value;
        }
        public int GetInt(string key, int defaultValue)
        {
            string value;
            if (!this.TryGetValue(key, out value) || value == null) value = string.Empty;
            int intValue;
            if (!int.TryParse(value, out intValue)) intValue = defaultValue;
            return intValue;
        }
        public Int32 GetInt32(string key, Int32 defaultValue)
        {
            string value;
            if (!this.TryGetValue(key, out value) || value == null) value = string.Empty;
            Int32 intValue;
            if (!Int32.TryParse(value, out intValue)) intValue = defaultValue;
            return intValue;
        }
        [CLSCompliantAttribute(false)]
        public UInt32 GetUInt32(string key, UInt32 defaultValue)
        {
            string value;
            if (!this.TryGetValue(key, out value) || value == null) value = string.Empty;
            UInt32 intValue;
            if (!UInt32.TryParse(value, out intValue)) intValue = defaultValue;
            return intValue;
        }
        public Int64 GetInt64(string key, Int64 defaultValue)
        {
            string value;
            if (!this.TryGetValue(key, out value) || value == null) value = string.Empty;
            Int64 intValue;
            if (!Int64.TryParse(value, out intValue)) intValue = defaultValue;
            return intValue;
        }
        [CLSCompliantAttribute(false)]
        public UInt64 GetUInt64(string key, UInt64 defaultValue)
        {
            string value;
            if (!this.TryGetValue(key, out value) || value == null) value = string.Empty;
            UInt64 intValue;
            if (!UInt64.TryParse(value, out intValue)) intValue = defaultValue;
            return intValue;
        }
        public float GetFloat(string key, float defaultValue)
        {
            string value;
            if (!this.TryGetValue(key, out value) || value == null) value = string.Empty;
            float fValue;
            if (!float.TryParse(value, out fValue)) fValue = defaultValue;
            return fValue;
        }
        public double GetDouble(string key, double defaultValue)
        {
            string value;
            if (!this.TryGetValue(key, out value) || value == null) value = string.Empty;
            double dValue;
            if (!double.TryParse(value, out dValue)) dValue = defaultValue;
            return dValue;
        }
        public decimal GetDecimal(string key, decimal defaultValue)
        {
            string value;
            if (!this.TryGetValue(key, out value) || value == null) value = string.Empty;
            decimal dValue;
            if (!decimal.TryParse(value, out dValue)) dValue = defaultValue;
            return dValue;
        }
        public DateTime GetDateTime(string key, DateTime defaultValue)
        {
            string value;
            if (!this.TryGetValue(key, out value) || value == null) value = string.Empty;
            DateTime dtValue;
            if (!DateTime.TryParseExact(value, @"yyyy-MM-yyTHH:mm:ss.fffffffzzz", null, System.Globalization.DateTimeStyles.AssumeUniversal, out dtValue))
            {
                if (!DateTime.TryParse(value, out dtValue)) dtValue = defaultValue;
            }
            return dtValue;
        }
        public bool GetBool(string key, bool defaultValue)
        {
            string value;
            if (!this.TryGetValue(key, out value) || value == null) value = string.Empty;
            bool bValue;
            if (!bool.TryParse(value, out bValue)) bValue = defaultValue;
            return bValue;
        }

        public void SetValue(string key, object value)
        {
            this[key] = value.ToString();
        }
        public void SetValue(string key, DateTime value)
        {
            this[key] = value.ToString(@"yyyy-MM-yyTHH:mm:ss.fffffffzzz");
        }
    }
}
