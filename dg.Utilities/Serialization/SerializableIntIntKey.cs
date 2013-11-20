using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace dg.Utilities.Serialization
{
    [Serializable()]
    public class SerializableIntIntKey : SerializableDoubleKey<int, int>
    {
        public SerializableIntIntKey()
        {
        }
        public SerializableIntIntKey(int A, int B)
            : base(A, B)
        {
        }
    }
}
