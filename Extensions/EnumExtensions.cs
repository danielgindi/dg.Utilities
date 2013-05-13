using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace dg.Utilities
{
    public static class EnumExtensions
    {
        public static bool TryParse<T>(this Enum theEnum, string strType,
            out T result)
        {
            string strTypeFixed = strType.Replace(' ', '_');
            if (Enum.IsDefined(typeof(T), strTypeFixed))
            {
                result = (T)Enum.Parse(typeof(T), strTypeFixed, true);
                return true;
            }
            else
            {
                foreach (string value in Enum.GetNames(typeof(T)))
                {
                    if (value.Equals(strTypeFixed,
                        StringComparison.OrdinalIgnoreCase))
                    {
                        result = (T)Enum.Parse(typeof(T), value);
                        return true;
                    }
                }
                result = default(T);
                return false;
            }
        }
        public static T MaxValue<T>(this Enum value)
            where T : IComparable, IFormattable
        {
            Array values = Enum.GetValues(value.GetType());
            if (values.Length == 0)
            {
                return default(T);
            }
            else
            {
                T val = (T)values.GetValue(0);
                foreach (T val2 in values)
                {
                    if (val2.CompareTo(val) > 0) val = val2;
                }
                return (T)val;
            }
        }
        public static T MinValue<T>(this Enum value)
            where T : IComparable, IFormattable
        {
            Array values = Enum.GetValues(value.GetType());
            if (values.Length == 0)
            {
                return default(T);
            }
            else
            {
                T val = (T)values.GetValue(0);
                foreach (T val2 in values)
                {
                    if (val2.CompareTo(val) < 0) val = val2;
                }
                return (T)val;
            }
        }
    }
}
