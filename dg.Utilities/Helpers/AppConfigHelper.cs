using System;
using System.Configuration;

namespace dg.Utilities
{
    public static class AppConfigHelper
    {
        public static Double GetDouble(string key, Double defaultValue)
        {
            string cfg = ConfigurationManager.AppSettings[key];
            Double value;
            if (!Double.TryParse(cfg, out value)) value = defaultValue;
            return value;
        }

        public static Decimal GetDecimal(string key, Decimal defaultValue)
        {
            string cfg = ConfigurationManager.AppSettings[key];
            Decimal value;
            if (!Decimal.TryParse(cfg, out value)) value = defaultValue;
            return value;
        }

        public static Int32 GetInt32(string key, Int32 defaultValue)
        {
            string cfg = ConfigurationManager.AppSettings[key];
            Int32 value;
            if (!Int32.TryParse(cfg, out value)) value = defaultValue;
            return value;
        }

        public static Int64 GetInt64(string key, Int64 defaultValue)
        {
            string cfg = ConfigurationManager.AppSettings[key];
            Int64 value;
            if (!Int64.TryParse(cfg, out value)) value = defaultValue;
            return value;
        }

        public static bool GetBoolean(string key, bool defaultValue)
        {
            string cfg = (ConfigurationManager.AppSettings[key] ?? "").ToLowerInvariant();
            if (cfg == @"true" || cfg == "yes" || cfg == "1") return true;
            if (cfg == @"false" || cfg == "no" || cfg == "0") return false;
            return defaultValue;
        }

        public static string GetString(string key, string defaultValue)
        {
            string cfg = ConfigurationManager.AppSettings[key];
            if (cfg == null) return defaultValue;
            else return cfg;
        }
    }
}
