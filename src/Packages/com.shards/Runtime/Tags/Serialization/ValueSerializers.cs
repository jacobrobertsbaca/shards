using System;

namespace Shards.Tags.Serialization
{
    [TagSerializer(Priority = int.MaxValue)]
    internal class ValueSerializer<TTag, TValue> : TagSerializer<TValue> where TTag : IValueTag<TValue>, new()
    {
        public override TValue Deserialize(ITag tag) => TagSerializer.ExpectTag<TTag>(tag).Value;

        public override ITag Serialize(TValue value)
        {
            TTag tag = new();
            tag.Value = value;
            return tag;
        }
    }

    internal class ByteSerializer : ValueSerializer<ByteTag, byte> {}
    internal class Int16Serializer : ValueSerializer<Int16Tag, short> {}
    internal class Int32Serializer : ValueSerializer<Int32Tag, int> {}
    internal class Int64Serializer : ValueSerializer<Int64Tag, long> {}
    internal class FloatSerializer : ValueSerializer<FloatTag, float> {}
    internal class DoubleSerializer : ValueSerializer<DoubleTag, double> {}
    internal class StringSerializer : ValueSerializer<StringTag, string> {}
    internal class BoolSerializer : ValueSerializer<BoolTag, bool> {}
}