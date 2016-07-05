using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;
using System.Configuration;

namespace dg.Utilities
{
    public static class AppConfig
    {
        [Obsolete]
        public static Double GetDouble(string key, Double defaultValue)
        {
            return AppConfigHelper.GetDouble(key, defaultValue);
        }

        [Obsolete]
        public static Decimal GetDecimal(string key, Decimal defaultValue)
        {
            return AppConfigHelper.GetDecimal(key, defaultValue);
        }

        [Obsolete]
        public static Int32 GetInt32(string key, Int32 defaultValue)
        {
            return AppConfigHelper.GetInt32(key, defaultValue);
        }

        [Obsolete]
        public static Int64 GetInt64(string key, Int64 defaultValue)
        {
            return AppConfigHelper.GetInt64(key, defaultValue);
        }

        [Obsolete]
        public static bool GetBoolean(string key, bool defaultValue)
        {
            return AppConfigHelper.GetBoolean(key, defaultValue);
        }

        [Obsolete]
        public static string GetString(string key, string defaultValue)
        {
            return AppConfigHelper.GetString(key, defaultValue);
        }
    }
}
