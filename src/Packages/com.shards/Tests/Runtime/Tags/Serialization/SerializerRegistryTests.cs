using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Shards.Tags.Serialization;
using Shards.Tags;
using System;

namespace Shards.Tests.Tags.Serialization
{
    public class SerializerRegistryTests
    {
        private static readonly Type TypeParameter = typeof(List<>).GetGenericArguments()[0];

        [Test]
        public void BaseType()
        {
            // derived not in inheritance hierarchy of base type
            Assert.IsNull(SerializerRegistry.GetBaseType(typeof(int), typeof(ArrayList))); // non-generic
            Assert.IsNull(SerializerRegistry.GetBaseType(typeof(int), typeof(List<int>))); // generic
            Assert.IsNull(SerializerRegistry.GetBaseType(typeof(int), typeof(IList)));     // interface

            // interfaces
            Assert.AreEqual(SerializerRegistry.GetBaseType(typeof(List<int>), typeof(ICollection<>)), typeof(ICollection<int>));
            Assert.AreEqual(SerializerRegistry.GetBaseType(typeof(int[]), typeof(ICollection<>)), typeof(ICollection<int>));

            // walking up hierarchy
            Assert.AreEqual(SerializerRegistry.GetBaseType(typeof(ListTag), typeof(List<>)), typeof(List<ITag>));
        }

        [Test]
        public void NormalizeType()
        {
            Assert.AreEqual(SerializerRegistry.GetNormalizedType(typeof(float)), typeof(float));

            Assert.AreEqual(SerializerRegistry.GetNormalizedType(TypeParameter), SerializerRegistry.OpenType);
            Assert.AreEqual(SerializerRegistry.GetNormalizedType(typeof(List<>)), typeof(List<SerializerRegistry.Open>));

            Type partialType = typeof(Dictionary<,>).MakeGenericType(typeof(string), TypeParameter);
            Assert.AreEqual(SerializerRegistry.GetNormalizedType(partialType), typeof(Dictionary<string, SerializerRegistry.Open>));

            Type nestedPartialType = typeof(List<>).MakeGenericType(partialType);
            Assert.AreEqual(SerializerRegistry.GetNormalizedType(nestedPartialType), typeof(List<Dictionary<string, SerializerRegistry.Open>>));
        }

        [Test]
        public void NormalizeArrayType()
        {
            Assert.AreEqual(SerializerRegistry.GetNormalizedType(typeof(float[])), typeof(float[]));
            Assert.AreEqual(SerializerRegistry.GetNormalizedType(typeof(float[,])), typeof(float[,]));
            Assert.AreEqual(SerializerRegistry.GetNormalizedType(typeof(float[][])), typeof(float[][]));

            Type openArray = TypeParameter.MakeArrayType();
            Assert.AreEqual(SerializerRegistry.GetNormalizedType(openArray), typeof(SerializerRegistry.Open[]));
            Assert.AreEqual(SerializerRegistry.GetNormalizedType(TypeParameter.MakeArrayType(2)), typeof(SerializerRegistry.Open[,]));

            Type nestedArray = openArray.MakeArrayType();
            Assert.AreEqual(SerializerRegistry.GetNormalizedType(nestedArray), typeof(SerializerRegistry.Open[][]));
        }

        [Test]
        public void NormalizeMixedType()
        {
            Type nestedDict = typeof(Dictionary<,>).MakeGenericType(
                typeof(List<>).MakeGenericType(TypeParameter),
                typeof(List<>).MakeGenericType(TypeParameter)
            );

            Assert.AreEqual(SerializerRegistry.GetNormalizedType(nestedDict), typeof(Dictionary<List<SerializerRegistry.Open>, List<SerializerRegistry.Open>>));

            Type nestedListArray = typeof(List<>).MakeGenericType(TypeParameter.MakeArrayType());
            Assert.AreEqual(SerializerRegistry.GetNormalizedType(nestedListArray), typeof(List<SerializerRegistry.Open[]>));
        }

        [Test]
        public void ExpandType()
        {
            Assert.That(SerializerRegistry.GetTypeExpansions(typeof(float)), Is.EquivalentTo(new[] {
                (0, typeof(float)), (1, SerializerRegistry.OpenType)
            }));

            Assert.That(SerializerRegistry.GetTypeExpansions(typeof(List<int>)), Is.EquivalentTo(new[] {
                (0, typeof(List<int>)),
                (1, typeof(List<SerializerRegistry.Open>)),
                (2, SerializerRegistry.OpenType)
            }));

            Assert.That(SerializerRegistry.GetTypeExpansions(typeof(Dictionary<string, bool>)), Is.EquivalentTo(new[] {
                (0, typeof(Dictionary<string, bool>)),
                (1, typeof(Dictionary<SerializerRegistry.Open, bool>)),
                (1, typeof(Dictionary<string, SerializerRegistry.Open>)),
                (2, typeof(Dictionary<SerializerRegistry.Open,SerializerRegistry.Open>)),
                (3, SerializerRegistry.OpenType)
            }));
        }

        [Test]
        public void ExpandArrayType()
        {
            Assert.That(SerializerRegistry.GetTypeExpansions(typeof(float[])), Is.EquivalentTo(new[] {
                (0, typeof(float[])),
                (1, typeof(SerializerRegistry.Open[])),
                (2, SerializerRegistry.OpenType)
            }));

            // Test multidimensional arrays
            Assert.That(SerializerRegistry.GetTypeExpansions(typeof(float[,])), Is.EquivalentTo(new[] {
                (0, typeof(float[,])),
                (1, typeof(SerializerRegistry.Open[,])),
                (2, SerializerRegistry.OpenType)
            }));

            // Test nested arrays
            Assert.That(SerializerRegistry.GetTypeExpansions(typeof(float[][])), Is.EquivalentTo(new[] {
                (0, typeof(float[][])),
                (1, typeof(SerializerRegistry.Open[][])),
                (2, typeof(SerializerRegistry.Open[])),
                (3, SerializerRegistry.OpenType)
            }));
        }

        [Test]
        public void ExpandMixedType()
        {
            Assert.That(SerializerRegistry.GetTypeExpansions(typeof(Dictionary<float[], List<int>>[])), Is.EquivalentTo(new[]
            {
                (0, typeof(Dictionary<float[], List<int>>[])),
                (1, typeof(Dictionary<float[], List<SerializerRegistry.Open>>[])),
                (1, typeof(Dictionary<SerializerRegistry.Open[], List<int>>[])),
                (2, typeof(Dictionary<float[], SerializerRegistry.Open>[])),
                (2, typeof(Dictionary<SerializerRegistry.Open[], List<SerializerRegistry.Open>>[])),
                (2, typeof(Dictionary<SerializerRegistry.Open, List<int>>[])),
                (3, typeof(Dictionary<SerializerRegistry.Open, List<SerializerRegistry.Open>>[])),
                (3, typeof(Dictionary<SerializerRegistry.Open[], SerializerRegistry.Open>[])),
                (4, typeof(Dictionary<SerializerRegistry.Open, SerializerRegistry.Open>[])),
                (5, typeof(SerializerRegistry.Open[])),
                (6, typeof(SerializerRegistry.Open)),
            }));
        }

        [TagSerializer(Ignore = true)]
        private class StringKeyDictSerializer<T> : TagSerializer<Dictionary<string, T>>
        {
            public override void Deserialize(ref Dictionary<string, T> value, ITag tag) => throw new System.NotImplementedException();
            public override ITag Serialize(Dictionary<string, T> value) => throw new System.NotImplementedException();
        }

        [TagSerializer(Ignore = true)]
        private class IntValueDictSerializer<T> : TagSerializer<Dictionary<T, int>>
        {
            public override void Deserialize(ref Dictionary<T, int> value, ITag tag) => throw new System.NotImplementedException();
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
            public override void Deserialize(ref Dictionary<string, T> value, ITag tag) => throw new System.NotImplementedException();
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
            public override void Deserialize(ref float value, ITag tag) => throw new System.NotImplementedException();
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
            public override void Deserialize(ref List<T> value, ITag tag) => throw new System.NotImplementedException();
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
            public override void Deserialize(ref Dictionary<K, V> value, ITag tag) => throw new System.NotImplementedException();
            public override ITag Serialize(Dictionary<K, V> value) => throw new System.NotImplementedException();
        }

        [TagSerializer(Ignore = true)]
        private class ReversedDictionarySerializer<K, V> : TagSerializer<Dictionary<V, K>>
        {
            public override void Deserialize(ref Dictionary<V, K> value, ITag tag) => throw new System.NotImplementedException();
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
            public override void Deserialize(ref float value, ITag tag) => throw new System.NotImplementedException();
            public override ITag Serialize(float value) => throw new System.NotImplementedException();
        }

        [TagSerializer(Ignore = true, Priority = 0)]
        private class HighPriority : TagSerializer<float>
        {
            public override void Deserialize(ref float value, ITag tag) => throw new System.NotImplementedException();
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
            public override void Deserialize(ref TCollection value, ITag tag) => throw new System.NotImplementedException();
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
