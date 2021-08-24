using System;

namespace dg.Utilities
{
    public static class ObjectExtensions
    {
        public static bool IsNull(this object value)
        {
            switch (value)
            {
                case null: return true;
                case System.Data.SqlTypes.INullable nullable when nullable.IsNull: return true;
                case DBNull _: return true;
                default: return false;
            }
        }
    }
}
