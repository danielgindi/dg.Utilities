using System;

namespace dg.Utilities.Serialization
{
    [Serializable()]
    public class SerializableLongLongKey : SerializableDoubleKey<Int64, Int64>
    {
        public SerializableLongLongKey()
        {
        }
        public SerializableLongLongKey(Int64 A, Int64 B)
            : base(A, B)
        {
        }
    }
}
