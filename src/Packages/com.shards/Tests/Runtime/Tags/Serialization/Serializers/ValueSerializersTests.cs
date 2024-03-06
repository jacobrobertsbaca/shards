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
        public void TestSerializer<TValue, TTag>(TValue source) where TTag : IValueTag<TValue>
        {
            ITag tag = TagSerializer.Serialize<TValue>(source);
            Assert.That(tag is TTag);
            TTag tt = (TTag)tag;
            Assert.AreEqual(tt.Value, source);
            TValue des = TagSerializer.Deserialize<TValue>(tag);
            Assert.AreEqual(des, source);
        }

        [Test] public void SerializeString()    => TestSerializer<string, StringTag>("hello world");
        [Test] public void SerializeByte()      => TestSerializer<byte, ByteTag>(123);
        [Test] public void SerializeInt16()     => TestSerializer<short, Int16Tag>(123);
        [Test] public void SerializeInt32()     => TestSerializer<int, Int32Tag>(123);
        [Test] public void SerializeInt64()     => TestSerializer<long, Int64Tag>(123);
        [Test] public void SerializeFloat()     => TestSerializer<float, FloatTag>(1.23f);
        [Test] public void SerializeDouble()    => TestSerializer<double, DoubleTag>(1.23);
        [Test] public void SerializeBool()      => TestSerializer<bool, BoolTag>(true);
    }
}
