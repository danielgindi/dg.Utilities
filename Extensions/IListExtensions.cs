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

        public static List<T[]> ColumnizeByCols<T>(this IList<T> input, int columnCount)
        {
            int rows;
            List<T> column;
            List<T[]> columns = new List<T[]>();

            rows = input.Count / columnCount;

            if (columnCount * rows != input.Count)
            {
                rows++;
            }

            int matrixX;
            int matrixY;
            int index = 0;
            for (matrixX = 0; matrixX < columnCount; matrixX++)
            {
                column = new List<T>();
                for (matrixY = 0; matrixY < rows; matrixY++)
                {
                    if (input.Count + matrixX + 1 - (index + columnCount) == 0 && matrixY != 0)
                    {
                        continue;
                    }

                    column.Add(input[index]);
                    index++;
                }
                columns.Add(column.ToArray());
            }

            return columns;
        }
    }
}
