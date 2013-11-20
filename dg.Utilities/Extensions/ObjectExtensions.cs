using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace dg.Utilities
{
    public static class ObjectExtensions
    {
        public static bool IsNull(this object value)
        {
            if (value == null) return true;
            if (value is System.Data.SqlTypes.INullable && ((System.Data.SqlTypes.INullable)value).IsNull) return true;
            if (value is DBNull) return true;
            else return false;
        }
    }
}
