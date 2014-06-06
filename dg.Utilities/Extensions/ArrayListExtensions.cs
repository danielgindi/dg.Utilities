using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace dg.Utilities
{
    public static class ArrayListExtensions
    {
        public static object Find(this ArrayList input, Predicate<object> match)
        {
            foreach (object t in input)
            {
                if (match(t)) return t;
            }
            return null;
        }
    }
}
