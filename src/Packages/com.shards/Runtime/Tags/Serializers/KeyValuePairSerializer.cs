using System.Collections.Generic;
using Shards.Tags.Serialization;

namespace Shards.Tags.Serializers
{
    [TagSerializer(Fallback = true)]
    internal class KeyValuePairSerializer<TKey, TValue> : TagSerializer<KeyValuePair<TKey, TValue>>
    {
        public override void Deserialize(ref KeyValuePair<TKey, TValue> value, ITag tag)
        {
            ListTag lt = TagSerializer.ExpectList(tag, 2);
            value = new(TagSerializer.Deserialize<TKey>(lt[0]), TagSerializer.Deserialize<TValue>(lt[1]));
        }

        public override ITag Serialize(KeyValuePair<TKey, TValue> value)
        {
            return new ListTag(TagSerializer.Serialize<TKey>(value.Key), TagSerializer.Serialize<TValue>(value.Value));
        }
    }
}
