using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shards.Tags.Serialization
{
    internal interface ITagSerializer {}

    public abstract class TagSerializer<T> : ITagSerializer
    {
        public abstract void Deserialize(ref T value, ITag tag);
        public abstract ITag Serialize(T value);
    }

    public static class TagSerializer
    {
        private static readonly SerializerRegistry registry = new();

        public static T Deserialize<T>(ITag tag)
        {
            T value = default;
            Deserialize<T>(ref value, tag);
            return value;
        }

        public static void Deserialize<T>(ref T value, ITag tag)
        {
            if (tag is NullTag)
            {
                value = default;
                return;
            }

            TagSerializer<T> serializer = registry.Get<T>();
            if (serializer is null) ThrowNoSerializer<T>();
            serializer.Deserialize(ref value, tag);
        }

        public static ITag Serialize<T>(T value)
        {
            if (value == null) return new NullTag();
            TagSerializer<T> serializer = registry.Get<T>();
            if (serializer is null) ThrowNoSerializer<T>();
            return serializer.Serialize(value);
        }

        public static TTag ExpectTag<TTag>(ITag tag) where TTag : ITag
        {
            if (tag is TTag t) return t;
            throw new TagException($"Expected {TagRegistry.GetTypeOfTag<TTag>()}, got {tag.Type}");
        }

        public static ListTag ExpectList(ITag tag, int count = -1)
        {
            var lt = ExpectTag<ListTag>(tag);
            if (count > 0 && lt.Count != count)
                throw new TagException($"Expected {nameof(ListTag)} with {count} elements, got {lt.Count}");
            return lt;
        } 

        private static void ThrowNoSerializer<T>()
        {
            throw new SerializerNotFoundException($"A serializer could not be found for type {typeof(T)}");
        }
    }
}