using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Shards.Tags.Serialization;
using Shards.Tags;

namespace Shards.Tests.Tags.Serializers
{
    public class UnitySerializersTests
    {
        public void Test<T>(T actual)
        {
            ITag serialized = TagSerializer.Serialize<T>(actual);
            T deserialized = TagSerializer.Deserialize<T>(serialized);
            Assert.AreEqual(deserialized, actual);
        }

        [Test] public void TestVec2()   => Test(new Vector2(2, 4));
        [Test] public void TestVec3()   => Test(new Vector3(2, 4, 6));
        [Test] public void TestVec4()   => Test(new Vector4(2, 4, 6, 8));
        [Test] public void TestColor()  => Test(new Color(0.3f, 0.2f, 0.8f, 0.3f));
    }
}
