using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Shards.Tags.Serialization;
using Shards.Tags;

namespace Shards.Tests.Tags.Serialization
{
    public class SerializerRegistryTests
    {
        [TagSerializer(Ignore = true)]
        private class StringKeyDictSerializer<T> : TagSerializer<Dictionary<string, T>>
        {
            public override Dictionary<string, T> Deserialize(ITag tag) => throw new System.NotImplementedException();
            public override ITag Serialize(Dictionary<string, T> value) => throw new System.NotImplementedException();
        }

        [TagSerializer(Ignore = true)]
        private class IntValueDictSerializer<T> : TagSerializer<Dictionary<T, int>>
        {
            public override Dictionary<T, int> Deserialize(ITag tag) => throw new System.NotImplementedException();
            public override ITag Serialize(Dictionary<T, int> value) => throw new System.NotImplementedException();
        }

        [Test]
        public void ResolveAmbiguous()
        {
            SerializerRegistry reg = new(typeof(StringKeyDictSerializer<>), typeof(IntValueDictSerializer<>));
            Assert.Catch<ShardException>(() => reg.Get<Dictionary<string, int>>());
            Assert.DoesNotThrow(() => reg.Get<Dictionary<string, bool>>());
            Assert.DoesNotThrow(() => reg.Get<Dictionary<bool, int>>());
        }

        [TagSerializer(Priority = -1, Ignore = true)]
        private class OverrideStringKeyDictSerializer<T> : TagSerializer<Dictionary<string, T>>
        {
            public override Dictionary<string, T> Deserialize(ITag tag) => throw new System.NotImplementedException();
            public override ITag Serialize(Dictionary<string, T> value) => throw new System.NotImplementedException();
        }

        [Test]
        public void ResolveAmbiguousOverridePriority()
        {
            SerializerRegistry reg = new(typeof(OverrideStringKeyDictSerializer<>), typeof(IntValueDictSerializer<>));
            Assert.AreEqual(reg.Get<Dictionary<string, int>>().GetType(), typeof(OverrideStringKeyDictSerializer<int>));
        }

        [TagSerializer(Ignore = true)]
        private class FloatSerializer : TagSerializer<float>
        {
            public override float Deserialize(ITag tag) => throw new System.NotImplementedException();
            public override ITag Serialize(float value) => throw new System.NotImplementedException();
        }

        [Test]
        public void ResolveNonGeneric()
        {
            SerializerRegistry reg = new(typeof(FloatSerializer));
            Assert.AreEqual(reg.Get<float>().GetType(), typeof(FloatSerializer));
        }

        [TagSerializer(Ignore = true)]
        private class ListSerializer<T> : TagSerializer<List<T>>
        {
            public override List<T> Deserialize(ITag tag) => throw new System.NotImplementedException();
            public override ITag Serialize(List<T> value) => throw new System.NotImplementedException();
        }

        [Test]
        public void ResolveGenericSingleType()
        {
            SerializerRegistry reg = new(typeof(ListSerializer<>));
            Assert.AreEqual(reg.Get<List<float>>().GetType(), typeof(ListSerializer<float>));
            Assert.AreEqual(reg.Get<List<List<float>>>().GetType(), typeof(ListSerializer<List<float>>));
            Assert.AreEqual(reg.Get<List<Dictionary<string, float>>>().GetType(), typeof(ListSerializer<Dictionary<string, float>>));
        }

        [TagSerializer(Ignore = true)]
        private class DictionarySerializer<K, V> : TagSerializer<Dictionary<K, V>>
        {
            public override Dictionary<K, V> Deserialize(ITag tag) => throw new System.NotImplementedException();
            public override ITag Serialize(Dictionary<K, V> value) => throw new System.NotImplementedException();
        }

        [TagSerializer(Ignore = true)]
        private class ReversedDictionarySerializer<K, V> : TagSerializer<Dictionary<V, K>>
        {
            public override Dictionary<V, K> Deserialize(ITag tag) => throw new System.NotImplementedException();
            public override ITag Serialize(Dictionary<V, K> value) => throw new System.NotImplementedException();
        }

        [Test]
        public void ResolveGenericTwoTypes()
        {
            SerializerRegistry reg = new(typeof(DictionarySerializer<,>));
            Assert.AreEqual(reg.Get<Dictionary<string, float>>().GetType(), typeof(DictionarySerializer<string, float>));
        }

        [Test]
        public void ResolveGenericTwoTypesReversed()
        {
            SerializerRegistry reg = new(typeof(ReversedDictionarySerializer<,>));
            Assert.AreEqual(reg.Get<Dictionary<string, float>>().GetType(), typeof(ReversedDictionarySerializer<float, string>));
        }

        [Test]
        public void ResolveSpecificSerializer()
        {
            // Tests that a more specific serializer is used over a general case generic one
            SerializerRegistry reg = new(typeof(DictionarySerializer<,>), typeof(StringKeyDictSerializer<>));
            Assert.AreEqual(reg.Get<Dictionary<string, int>>().GetType(), typeof(StringKeyDictSerializer<int>));
            Assert.AreEqual(reg.Get<Dictionary<bool, int>>().GetType(), typeof(DictionarySerializer<bool, int>));
        }

        [TagSerializer(Ignore = true, Priority = int.MaxValue)]
        private class LowPriority : TagSerializer<float>
        {
            public override float Deserialize(ITag tag) => throw new System.NotImplementedException();
            public override ITag Serialize(float value) => throw new System.NotImplementedException();
        }

        [TagSerializer(Ignore = true, Priority = 0)]
        private class HighPriority : TagSerializer<float>
        {
            public override float Deserialize(ITag tag) => throw new System.NotImplementedException();
            public override ITag Serialize(float value) => throw new System.NotImplementedException();
        }

        [Test]
        public void ResolveRespectsPriority()
        {
            SerializerRegistry reg = new(typeof(LowPriority), typeof(HighPriority));
            Assert.AreEqual(reg.Get<float>().GetType(), typeof(HighPriority));
        }

        [TagSerializer(Ignore = true)]
        private class CollectionSerializer<TElement, TCollection>
        : TagSerializer<TCollection> where TCollection : ICollection<TElement>, new()
        {
            public override TCollection Deserialize(ITag tag) => throw new System.NotImplementedException();
            public override ITag Serialize(TCollection value) => throw new System.NotImplementedException();
        }

        [Test]
        public void ResolveConstraintParameter()
        {
            SerializerRegistry reg = new(typeof(CollectionSerializer<,>));
            Assert.AreEqual(reg.Get<List<int>>().GetType(), typeof(CollectionSerializer<int, List<int>>));
        }

        [Test]
        public void ResolveConstraintParameterComplex()
        {
            SerializerRegistry reg = new(typeof(CollectionSerializer<,>));
            Assert.AreEqual(reg.Get<Dictionary<string, int>>().GetType(), typeof(CollectionSerializer<KeyValuePair<string, int>, Dictionary<string, int>>));
        }

        [Test]
        public void ResolveConstraintDontMatchGeneral()
        {
            SerializerRegistry reg = new(typeof(CollectionSerializer<,>));
            Assert.IsNull(reg.Get<float>());
        }

        [Test]
        public void ResolveConstraintMissingConstraint()
        {
            SerializerRegistry reg = new(typeof(CollectionSerializer<,>));
            Assert.IsNull(reg.Get<IList<int>>()); // Ensure that `new()` constraint is respected
        } 
    }
}
