using UnityEngine;

namespace Shards.Tags.Serialization.Serializers
{
    [TagSerializer(Fallback = true)]
    internal class Vector2Serializer : TagSerializer<Vector2>
    {
        public override void Deserialize(ref Vector2 value, ITag tag)
        {
            ListTag lt = TagSerializer.ExpectList(tag, 2);
            TagSerializer.Deserialize(ref value.x, lt[0]);
            TagSerializer.Deserialize(ref value.y, lt[1]);
        }

        public override ITag Serialize(Vector2 value) => new ListTag(
            TagSerializer.Serialize(value.x),
            TagSerializer.Serialize(value.y)
        );
    }

    [TagSerializer(Fallback = true)]
    internal class Vector3Serializer : TagSerializer<Vector3>
    {
        public override void Deserialize(ref Vector3 value, ITag tag)
        {
            ListTag lt = TagSerializer.ExpectList(tag, 3);
            TagSerializer.Deserialize(ref value.x, lt[0]);
            TagSerializer.Deserialize(ref value.y, lt[1]);
            TagSerializer.Deserialize(ref value.z, lt[2]);
        }

        public override ITag Serialize(Vector3 value) => new ListTag(
            TagSerializer.Serialize(value.x),
            TagSerializer.Serialize(value.y),
            TagSerializer.Serialize(value.z)
        );
    }

    [TagSerializer(Fallback = true)]
    internal class Vector4Serializer : TagSerializer<Vector4>
    {
        public override void Deserialize(ref Vector4 value, ITag tag)
        {
            ListTag lt = TagSerializer.ExpectList(tag, 4);
            TagSerializer.Deserialize(ref value.x, lt[0]);
            TagSerializer.Deserialize(ref value.y, lt[1]);
            TagSerializer.Deserialize(ref value.z, lt[2]);
            TagSerializer.Deserialize(ref value.w, lt[3]);
        }

        public override ITag Serialize(Vector4 value) => new ListTag(
            TagSerializer.Serialize(value.x),
            TagSerializer.Serialize(value.y),
            TagSerializer.Serialize(value.z),
            TagSerializer.Serialize(value.w)
        );
    }

    [TagSerializer(Fallback = true)]
    internal class ColorSerializer : TagSerializer<Color>
    {
        public override void Deserialize(ref Color value, ITag tag)
        {
            ListTag lt = TagSerializer.ExpectList(tag, 4);
            TagSerializer.Deserialize(ref value.r, lt[0]);
            TagSerializer.Deserialize(ref value.g, lt[1]);
            TagSerializer.Deserialize(ref value.b, lt[2]);
            TagSerializer.Deserialize(ref value.a, lt[3]);
        }

        public override ITag Serialize(Color value) => new ListTag(
            TagSerializer.Serialize(value.r),
            TagSerializer.Serialize(value.g),
            TagSerializer.Serialize(value.b),
            TagSerializer.Serialize(value.a)
        );
    }
}