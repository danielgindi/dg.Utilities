using System;
using System.Collections.Generic;
using System.Text;

namespace dg.Utilities
{
    public static class IEnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> input, Predicate<T> condition, Action<T> action)
        {
            if (action == null) return;
            foreach (T item in input)
            {
                if (condition == null || condition(item)) action(item);
            }
        }
    }
}
