using System;
using System.Collections.Generic;
using System.Text;

namespace dg.Utilities
{
    public static class CharArrayExtensions
    {
        public static string GenerateRandomString(this char[] input, int length)
        {
            return StringHelper.GenerateRandomString(input, length);
        }
    }
}
