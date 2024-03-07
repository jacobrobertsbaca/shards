using System;
using Shards.Tags.Serialization;

namespace Shards.Tags.Serializers
{
    [TagSerializer(Fallback = true)]
    internal abstract class ValueSerializer<TTag, TValue> : TagSerializer<TValue>
        where TTag : IValueTag<TValue>, new()
    {
        protected void ThrowRange<TNumber>(TNumber value)
        {
            throw new TagException($"Got value {value} which is outside of the range of type {typeof(TValue)}");
        }

        public override void Deserialize(ref TValue value, ITag tag)
        {
            value = TagSerializer.ExpectTag<TTag>(tag).Value;
        }

        public override ITag Serialize(TValue value)
        {
            TTag tag = new();
            tag.Value = value;
            return tag;
        }
    }

    internal abstract class IntegerSerializer<TTag, TValue> : ValueSerializer<TTag, TValue>
        where TValue : struct
        where TTag : IValueTag<TValue>, new()
    {
        protected abstract TValue ConvertSigned(long value);
        protected abstract TValue ConvertUnsigned(ulong value);

        public override void Deserialize(ref TValue value, ITag tag)
        {
            value = tag switch
            {
                SByteTag t => ConvertSigned(t.Value),
                ByteTag t => ConvertUnsigned(t.Value),
                Int16Tag t => ConvertSigned(t.Value),
                UInt16Tag t => ConvertUnsigned(t.Value),
                Int32Tag t => ConvertSigned(t.Value),
                UInt32Tag t => ConvertUnsigned(t.Value),
                Int64Tag t => ConvertSigned(t.Value),
                UInt64Tag t => ConvertUnsigned(t.Value),
                FloatTag t => ConvertSigned((long) t.Value),
                DoubleTag t => ConvertSigned((long) t.Value),
                DecimalTag t => ConvertSigned((long) t.Value),
                _ => throw new TagException($"Expected {typeof(TTag)}, got {tag.Type}"),
            };
        }
    }

    internal abstract class FloatingPointSerializer<TTag, TValue> : ValueSerializer<TTag, TValue>
        where TValue : struct
        where TTag : IValueTag<TValue>, new()
    {
        protected abstract TValue ConvertFloat(float f);
        protected abstract TValue ConvertDouble(double d);
        protected abstract TValue ConvertDecimal(decimal d);

        public override void Deserialize(ref TValue value, ITag tag)
        {
            value = tag switch
            {
                SByteTag t => ConvertDouble(t.Value),
                ByteTag t => ConvertDouble(t.Value),
                Int16Tag t => ConvertDouble(t.Value),
                UInt16Tag t => ConvertDouble(t.Value),
                Int32Tag t => ConvertDouble(t.Value),
                UInt32Tag t => ConvertDouble(t.Value),
                Int64Tag t => ConvertDouble(t.Value),
                UInt64Tag t => ConvertDouble(t.Value),
                FloatTag t => ConvertFloat(t.Value),
                DoubleTag t => ConvertDouble(t.Value),
                DecimalTag t => ConvertDecimal(t.Value),
                _ => throw new TagException($"Expected {typeof(TTag)}, got {tag.Type}"),
            };
        }
    }

    internal class SByteSerializer : IntegerSerializer<SByteTag, sbyte>
    {
        protected override sbyte ConvertSigned(long value)
        {
            if (value < sbyte.MinValue || value > sbyte.MaxValue) ThrowRange(value);
            return (sbyte)value;
        }

        protected override sbyte ConvertUnsigned(ulong value)
        {
            if (value > (ulong)sbyte.MaxValue) ThrowRange(value);
            return ConvertSigned((long)value);
        }
    }

    internal class ByteSerializer : IntegerSerializer<ByteTag, byte>
    {
        protected override byte ConvertSigned(long value)
        {
            if (value < 0) ThrowRange(value);
            return ConvertUnsigned((ulong)value);
        }

        protected override byte ConvertUnsigned(ulong value)
        {
            if (value > byte.MaxValue) ThrowRange(value);
            return (byte)value;
        }
    }

    internal class Int16Serializer : IntegerSerializer<Int16Tag, short>
    {
        protected override short ConvertSigned(long value)
        {
            if (value < short.MinValue || value > short.MaxValue) ThrowRange(value);
            return (short)value;
        }

        protected override short ConvertUnsigned(ulong value)
        {
            if (value > (ulong) short.MaxValue) ThrowRange(value);
            return ConvertSigned((long)value);
        }
    }

    internal class UInt16Serializer : IntegerSerializer<UInt16Tag, ushort>
    {
        protected override ushort ConvertSigned(long value)
        {
            if (value < 0) ThrowRange(value);
            return ConvertUnsigned((ulong)value);
        }

        protected override ushort ConvertUnsigned(ulong value)
        {
            if (value > ushort.MaxValue) ThrowRange(value);
            return (ushort)value;
        }
    }

    internal class Int32Serializer : IntegerSerializer<Int32Tag, int>
    {
        protected override int ConvertSigned(long value)
        {
            if (value < int.MinValue || value > int.MaxValue) ThrowRange(value);
            return (int)value;
        }

        protected override int ConvertUnsigned(ulong value)
        {
            if (value > int.MaxValue) ThrowRange(value);
            return ConvertSigned((long)value);
        }
    }

    internal class UInt32Serializer : IntegerSerializer<UInt32Tag, uint>
    {
        protected override uint ConvertSigned(long value)
        {
            if (value < 0) ThrowRange(value);
            return ConvertUnsigned((ulong)value);
        }

        protected override uint ConvertUnsigned(ulong value)
        {
            if (value > uint.MaxValue) ThrowRange(value);
            return (uint)value;
        }
    }

    internal class Int64Serializer : IntegerSerializer<Int64Tag, long>
    {
        protected override long ConvertSigned(long value) => value;

        protected override long ConvertUnsigned(ulong value)
        {
            if (value > long.MaxValue) ThrowRange(value);
            return (long)value;
        }
    }

    internal class UInt64Serializer : IntegerSerializer<UInt64Tag, ulong>
    {
        protected override ulong ConvertSigned(long value)
        {
            if (value < 0) ThrowRange(value);
            return (ulong)value;
        }

        protected override ulong ConvertUnsigned(ulong value) => value;
    }

    internal class FloatSerializer : FloatingPointSerializer<FloatTag, float>
    {
        protected override float ConvertDecimal(decimal d) => (float)d;
        protected override float ConvertDouble(double d) => (float) d;
        protected override float ConvertFloat(float f) => f;
    }
    internal class DoubleSerializer : FloatingPointSerializer<DoubleTag, double>
    {
        protected override double ConvertDecimal(decimal d) => (double)d;
        protected override double ConvertDouble(double d) => d;
        protected override double ConvertFloat(float f) => (double)f;
    }

    internal class DecimalSerializer : FloatingPointSerializer<DecimalTag, decimal>
    {
        protected override decimal ConvertDecimal(decimal d) => d;
        protected override decimal ConvertDouble(double d) => (decimal)d;
        protected override decimal ConvertFloat(float f) => (decimal)f;
    }

    internal class StringSerializer : ValueSerializer<StringTag, string> {}
    internal class BoolSerializer : ValueSerializer<BoolTag, bool> {}
    internal class BlobSerializer : ValueSerializer<BlobTag, byte[]> {}
}