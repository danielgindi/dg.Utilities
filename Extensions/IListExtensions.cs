using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

namespace dg.Utilities
{
    public static class IListExtensions
    {
        public static List<T> Sort<T>(this List<T> input, string propertyName)
        {
            return input.Sort(propertyName, true);
        }
        public static List<T> Sort<T>(this List<T> input, string propertyName, bool ascendingOrder)
        {
            if (propertyName == null || input.Count <= 2) return input;
            Type type = input[0].GetType();
            PropertyInfo info = type.GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if (info != null)
            {
                if (ascendingOrder)
                {
                    input.Sort(delegate(T a, T b)
                    {
                        object val1 = info.GetValue(a, null);
                        object val2 = info.GetValue(b, null);
                        if (val1 == null && val2 != null) return -1;
                        else if (val2 == null && val1 != null) return 1;
                        else if (val2 == null && val1 == null) return 0;
                        return ((IComparable)val1).CompareTo(val2);
                    });
                }
                else
                {
                    input.Sort(delegate(T b, T a)
                    {
                        object val1 = info.GetValue(a, null);
                        object val2 = info.GetValue(b, null);
                        if (val1 == null && val2 != null) return -1;
                        else if (val2 == null && val1 != null) return 1;
                        else if (val2 == null && val1 == null) return 0;
                        return ((IComparable)val1).CompareTo(val2);
                    });
                }
            }
            else
            {
                FieldInfo finfo = type.GetField(propertyName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                if (finfo == null) return input;

                if (ascendingOrder)
                {
                    input.Sort(delegate(T a, T b)
                    {
                        object val1 = finfo.GetValue(a);
                        object val2 = finfo.GetValue(b);
                        if (val1 == null && val2 != null) return -1;
                        else if (val2 == null && val1 != null) return 1;
                        else if (val2 == null && val1 == null) return 0;
                        return ((IComparable)val1).CompareTo(val2);
                    });
                }
                else
                {
                    input.Sort(delegate(T b, T a)
                    {
                        object val1 = finfo.GetValue(a);
                        object val2 = finfo.GetValue(b);
                        if (val1 == null && val2 != null) return -1;
                        else if (val2 == null && val1 != null) return 1;
                        else if (val2 == null && val1 == null) return 0;
                        return ((IComparable)val1).CompareTo(val2);
                    });
                }
            }
            return input;
        }
    }
}
