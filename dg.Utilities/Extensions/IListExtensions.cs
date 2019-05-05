using System.Collections.Generic;

namespace dg.Utilities
{
    public static class IListExtensions
    {
        public static List<T> Sort<T>(this List<T> input, string propertyName)
        {
            return ListHelper.Sort(input, propertyName);
        }

        public static List<T> Sort<T>(this List<T> input, string propertyName, bool ascendingOrder)
        {
            return ListHelper.Sort(input, propertyName, ascendingOrder);
        }

        public static List<T[]> ColumnizeByCols<T>(this IList<T> input, int columnCount)
        {
            return ListHelper.ColumnizeByCols(input, columnCount);
        }
    }
}
