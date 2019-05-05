using System;

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
