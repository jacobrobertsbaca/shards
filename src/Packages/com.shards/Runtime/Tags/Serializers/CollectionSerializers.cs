using System.Collections.Generic;
using System.Linq;
using Shards.Tags.Serialization;

namespace Shards.Tags.Serializers
{
    [TagSerializer(Fallback = true)]
    internal class ArraySerializer<TElement> : TagSerializer<TElement[]>
    {
        public override void Deserialize(ref TElement[] value, ITag tag)
        {
            ListTag lt = TagSerializer.ExpectTag<ListTag>(tag);
            if (value is null || value.Length != lt.Count) value = new TElement[lt.Count];

            for (int i = 0; i < value.Length; i++)
                TagSerializer.Deserialize<TElement>(ref value[i], lt[i]);
        }

        public override ITag Serialize(TElement[] value) => new ListTag(value.Select(TagSerializer.Serialize<TElement>));
    }

    internal abstract class BaseCollectionSerializer<TElement, TCollection>
        : TagSerializer<TCollection> where TCollection : IEnumerable<TElement>, new()
    {
        protected virtual bool IsBackwards { get; } = false;

        protected abstract void Clear(TCollection value);
        protected abstract void Add(TCollection value, TElement item);

        public override void Deserialize(ref TCollection value, ITag tag)
        {
            if (value is null) value = new();
            else Clear(value);

            ListTag lt = TagSerializer.ExpectTag<ListTag>(tag);

            if (IsBackwards)
                for (int i = lt.Count - 1; i >= 0; i--) Add(value, TagSerializer.Deserialize<TElement>(lt[i]));
            else
                foreach (ITag itemTag in lt) Add(value, TagSerializer.Deserialize<TElement>(itemTag));
        }

        public override ITag Serialize(TCollection value) => new ListTag(value.Select(TagSerializer.Serialize<TElement>));
    }

    [TagSerializer(Fallback = true)]
    internal class CollectionSerializer<TElement, TCollection>
        : BaseCollectionSerializer<TElement, TCollection> where TCollection : ICollection<TElement>, new()
    {
        protected override void Add(TCollection value, TElement item) => value.Add(item);
        protected override void Clear(TCollection value) => value.Clear();
    }

    [TagSerializer(Fallback = true)]
    internal class StackSerializer<TElement> : BaseCollectionSerializer<TElement, Stack<TElement>>
    {
        protected override bool IsBackwards { get; } = true;
        protected override void Add(Stack<TElement> value, TElement item) => value.Push(item);
        protected override void Clear(Stack<TElement> value) => value.Clear();
    }

    [TagSerializer(Fallback = true)]
    internal class QueueSerializer<TElement> : BaseCollectionSerializer<TElement, Queue<TElement>>
    {
        protected override void Add(Queue<TElement> value, TElement item) => value.Enqueue(item);
        protected override void Clear(Queue<TElement> value) => value.Clear();
    }
}