using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace dg.Utilities
{
    public static class EnumStrings
    {
        public sealed class StringValue : Attribute
        {
            private string _Value;

            public StringValue(string Value) { this._Value = Value; }
            public string Value
            {
                get { return _Value; }
                set { _Value = value; }
            }
        }
        public static string GetEnumStringValue(Object en)
        {
            System.Type type = en.GetType();
            System.Reflection.MemberInfo[] memInfo = type.GetMember(en.ToString());
            if (memInfo != null && memInfo.Length > 0)
            {
                object[] attrs = memInfo[0].GetCustomAttributes(typeof(StringValue), false);
                if (attrs != null && attrs.Length > 0) return ((StringValue)attrs[0]).Value;
            }
            return en.ToString();
        }
    }
}
