using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace dg.Utilities
{
    public static class ArrayExtensions
    {
        public static bool Contains<T>(this T[] input, T item)
        {
            foreach (T t in input)
            {
                if (t.Equals(item)) return true;
            }
            return false;
        }
        public static object Find<T>(this T[] input, Predicate<T> match)
        {
            foreach (T t in input)
            {
                if (match(t)) return t;
            }
            return null;
        }
        public static T[] GetUniqueArray<T>(this T[] input)
        {
            ArrayList newArray = new ArrayList();
            foreach (T t in input)
            {
                if (!newArray.Contains(t)) newArray.Add(t);
            }
            return (T[])(newArray.ToArray(typeof(T)));
        }
        public static T[] Remove<T>(this T[] input, T item)
        {
            ArrayList newArray = new ArrayList();
            foreach (T t in input)
            {
                if (!t.Equals(item)) newArray.Add(t);
            }
            return (T[])(newArray.ToArray(typeof(T)));
        }
    }
}
