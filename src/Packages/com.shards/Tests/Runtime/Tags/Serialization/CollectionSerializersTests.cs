using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Shards.Tags.Serialization;
using Shards.Tags;

namespace Shards.Tests.Tags.Serialization
{
    public class CollectionSerializersTests
    {
        public void TestEnumerable<TSource>(TSource source) where TSource : IEnumerable
        {
            ITag serialized = TagSerializer.Serialize<TSource>(source);
            Assert.That(serialized is ListTag);
            TSource deserialized = TagSerializer.Deserialize<TSource>(serialized);
            Assert.That(deserialized, Is.EqualTo(source));
        }

        [Test] public void SerializeArray() => TestEnumerable(new int[] { 1, 2, 3 });
        [Test] public void SerializeList() => TestEnumerable(new List<int> { 1, 2, 3 });
        [Test] public void SerializeSet() => TestEnumerable(new HashSet<int> { 1, 2, 3 });

        [Test]
        public void SerializeDict() => TestEnumerable(new Dictionary<int, string>
        {
            { 1, "hello" },
            { 2, "world" },
            { 3, "!!!" }
        });
    }
}
