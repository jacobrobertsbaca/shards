using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Shards.Tags.Serialization;
using Shards.Tags;

namespace Shards.Tests.Tags.Serialization.Serializers
{
    public class ValueSerializersTests
    {
        public void TestValid<TValue, TTag>(TValue source) where TTag : IValueTag<TValue>
        {
            ITag tag = TagSerializer.Serialize<TValue>(source);
            Assert.That(tag is TTag);
            TTag tt = (TTag)tag;
            Assert.AreEqual(tt.Value, source);
            TValue des = TagSerializer.Deserialize<TValue>(tag);
            Assert.AreEqual(des, source);
        }

        public void TestConversion<TSource, TSourceTag, TDest>(TSource sourceValue)
            where TSourceTag : IValueTag<TSource>, new()
        {
            TSourceTag source = new();
            source.Value = sourceValue;
            TDest destValue = TagSerializer.Deserialize<TDest>(source);
            TestContext.WriteLine($"Converted {typeof(TSource)} {sourceValue} to {typeof(TDest)} {destValue}");
        }

        public void InvalidConversion<TSource, TSourceTag, TDest>(TSource sourceValue)
            where TSourceTag : IValueTag<TSource>, new()
        {
            Assert.Throws<TagException>(() => TestConversion<TSource, TSourceTag, TDest>(sourceValue));
        }

        [TestCase((byte)0)]
        [TestCase((sbyte)0)]
        [TestCase((ushort)0)]
        [TestCase((short)0)]
        [TestCase((uint)0)]
        [TestCase((int)0)]
        [TestCase((ulong)0)]
        [TestCase((long)0)]
        [TestCase((float)0)]
        [TestCase((double)0)]
        public void ByteConversionsValid<T>(T value) => TestConversion<byte, ByteTag, T>(15);

        [TestCase((sbyte)0)]
        public void ByteConversionsInvalid<T>(T value) => InvalidConversion<byte, ByteTag, T>(255);

        [TestCase((byte)0)]
        [TestCase((sbyte)0)]
        [TestCase((ushort)0)]
        [TestCase((short)0)]
        [TestCase((uint)0)]
        [TestCase((int)0)]
        [TestCase((ulong)0)]
        [TestCase((long)0)]
        [TestCase((float)0)]
        [TestCase((double)0)]
        public void SByteConversionsValid<T>(T value) => TestConversion<sbyte, SByteTag, T>(15);

        [TestCase((byte)0)]
        [TestCase((ushort)0)]
        [TestCase((uint)0)]
        [TestCase((ulong)0)]
        public void SByteConversionsInvalid<T>(T value) => InvalidConversion<sbyte, SByteTag, T>(sbyte.MinValue);

        [TestCase((byte)0)]
        [TestCase((sbyte)0)]
        [TestCase((ushort)0)]
        [TestCase((short)0)]
        [TestCase((uint)0)]
        [TestCase((int)0)]
        [TestCase((ulong)0)]
        [TestCase((long)0)]
        [TestCase((float)0)]
        [TestCase((double)0)]
        public void UInt16ConversionsValid<T>(T value) => TestConversion<ushort, UInt16Tag, T>(15);

        [TestCase((byte)0)]
        [TestCase((sbyte)0)]
        [TestCase((short)0)]
        public void UInt16ConversionsInvalid<T>(T value) => InvalidConversion<ushort, UInt16Tag, T>(ushort.MaxValue);

        [TestCase((byte)0)]
        [TestCase((sbyte)0)]
        [TestCase((ushort)0)]
        [TestCase((short)0)]
        [TestCase((uint)0)]
        [TestCase((int)0)]
        [TestCase((ulong)0)]
        [TestCase((long)0)]
        [TestCase((float)0)]
        [TestCase((double)0)]
        public void Int16ConversionsValid<T>(T value) => TestConversion<short, Int16Tag, T>(15);

        [TestCase((byte)0)]
        [TestCase((sbyte)0)]
        [TestCase((ushort)0)]
        [TestCase((uint)0)]
        [TestCase((ulong)0)]
        public void Int16ConversionsInvalid<T>(T value) => InvalidConversion<short, Int16Tag, T>(short.MinValue);

        [TestCase((byte)0)]
        [TestCase((sbyte)0)]
        [TestCase((ushort)0)]
        [TestCase((short)0)]
        [TestCase((uint)0)]
        [TestCase((int)0)]
        [TestCase((ulong)0)]
        [TestCase((long)0)]
        [TestCase((float)0)]
        [TestCase((double)0)]
        public void UInt32ConversionsValid<T>(T value) => TestConversion<uint, UInt32Tag, T>(15);

        [TestCase((byte)0)]
        [TestCase((sbyte)0)]
        [TestCase((ushort)0)]
        [TestCase((short)0)]
        [TestCase((int)0)]
        public void UInt32ConversionsInvalid<T>(T value) => InvalidConversion<uint, UInt32Tag, T>(uint.MaxValue);

        [TestCase((byte)0)]
        [TestCase((sbyte)0)]
        [TestCase((ushort)0)]
        [TestCase((short)0)]
        [TestCase((uint)0)]
        [TestCase((int)0)]
        [TestCase((ulong)0)]
        [TestCase((long)0)]
        [TestCase((float)0)]
        [TestCase((double)0)]
        public void Int32ConversionsValid<T>(T value) => TestConversion<int, Int32Tag, T>(15);

        [TestCase((byte)0)]
        [TestCase((sbyte)0)]
        [TestCase((short)0)]
        [TestCase((ushort)0)]
        [TestCase((uint)0)]
        [TestCase((ulong)0)]
        public void Int32ConversionsInvalid<T>(T value) => InvalidConversion<int, Int32Tag, T>(int.MinValue);

        [TestCase((byte)0)]
        [TestCase((sbyte)0)]
        [TestCase((ushort)0)]
        [TestCase((short)0)]
        [TestCase((uint)0)]
        [TestCase((int)0)]
        [TestCase((ulong)0)]
        [TestCase((long)0)]
        [TestCase((float)0)]
        [TestCase((double)0)]
        public void Int64ConversionsValid<T>(T value) => TestConversion<long, Int64Tag, T>(15);

        [TestCase((byte)0)]
        [TestCase((sbyte)0)]
        [TestCase((short)0)]
        [TestCase((ushort)0)]
        [TestCase((int)0)]
        [TestCase((uint)0)]
        [TestCase((ulong)0)]
        public void Int64ConversionsInvalid<T>(T value) => InvalidConversion<long, Int64Tag, T>(long.MinValue);

        [TestCase((byte)0)]
        [TestCase((sbyte)0)]
        [TestCase((ushort)0)]
        [TestCase((short)0)]
        [TestCase((uint)0)]
        [TestCase((int)0)]
        [TestCase((ulong)0)]
        [TestCase((long)0)]
        [TestCase((float)0)]
        [TestCase((double)0)]
        public void UInt64ConversionsValid<T>(T value) => TestConversion<ulong, UInt64Tag, T>(15);

        [TestCase((byte)0)]
        [TestCase((sbyte)0)]
        [TestCase((ushort)0)]
        [TestCase((short)0)]
        [TestCase((uint)0)]
        [TestCase((int)0)]
        [TestCase((long)0)]
        public void UInt64ConversionsInvalid<T>(T value) => InvalidConversion<ulong, UInt64Tag, T>(ulong.MaxValue);


        [TestCase((byte)0)]
        [TestCase((sbyte)0)]
        [TestCase((ushort)0)]
        [TestCase((short)0)]
        [TestCase((uint)0)]
        [TestCase((int)0)]
        [TestCase((ulong)0)]
        [TestCase((long)0)]
        [TestCase((float)0)]
        [TestCase((double)0)]
        public void FloatConversionValid<T>(T value) => TestConversion<float, FloatTag, T>(15.5f);

        [TestCase((byte)0)]
        [TestCase((sbyte)0)]
        [TestCase((ushort)0)]
        [TestCase((short)0)]
        [TestCase((uint)0)]
        [TestCase((int)0)]
        [TestCase((ulong)0)]
        [TestCase((long)0)]
        [TestCase((float)0)]
        [TestCase((double)0)]
        public void DoubleConversionValid<T>(T value) => TestConversion<double, DoubleTag, T>(15.5);
    }
}
