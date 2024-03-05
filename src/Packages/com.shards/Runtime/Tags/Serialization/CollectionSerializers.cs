using System.Collections.Generic;
using System.Linq;

namespace Shards.Tags.Serialization
{
    internal class CollectionSerializer<TElement, TCollection>
        : TagSerializer<TCollection> where TCollection : ICollection<TElement>, new()
    {
        public override TCollection Deserialize(ITag tag)
        {
            TCollection collection = new();
            foreach (var item in TagSerializer.ExpectTag<ListTag>(tag))
            {
                collection.Add(TagSerializer.Deserialize<TElement>(item));
            }
            return collection;
        }

        public override ITag Serialize(TCollection value)
        {
            ListTag tag = new ListTag();
            foreach (var item in value)
            {
                tag.Add(TagSerializer.Serialize<TElement>(item));
            }
            return tag;
        }
    }

    //internal class ArraySerializer<TElement> : TagSerializer<TElement[]>
    //{
    //    public override TElement[] Deserialize(ITag tag)
    //        => TagSerializer.Deserialize<List<TElement>>(tag).ToArray();

    //    public override ITag Serialize(TElement[] value)
    //        => TagSerializer.Serialize<List<TElement>>(value.ToList());
    //}
}