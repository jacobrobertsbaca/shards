using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shards.Tags.Serialization
{
    internal interface ITagSerializer
    {
        object Deserialize(ITag tag);
        ITag Serialize(object value);
    }

    public abstract class TagSerializer<T> : ITagSerializer
    {
        public abstract T Deserialize(ITag tag);
        public abstract ITag Serialize(T value);

        object ITagSerializer.Deserialize(ITag tag) => Deserialize(tag);
        ITag ITagSerializer.Serialize(object value) => Serialize((T) value);
    }

    public static class TagSerializer
    {
        private static readonly SerializerRegistry registry = new();

        public static T Deserialize<T>(ITag tag)
        {
            if (tag is NullTag) return default;
            ITagSerializer serializer = registry.Get<T>();
            if (serializer is null) ThrowNoSerializer<T>();
            return (T) serializer.Deserialize(tag);
        }

        public static ITag Serialize<T>(T value)
        {
            if (value == null) return new NullTag();
            ITagSerializer serializer = registry.Get<T>();
            if (serializer is null) ThrowNoSerializer<T>();
            return serializer.Serialize(value);
        }

        public static TTag ExpectTag<TTag>(ITag tag) where TTag : ITag
        {
            if (tag is TTag t) return t;
            throw new TagException($"Expected {TagRegistry.GetTypeOfTag<TTag>()}, got {tag.Type}");
        }

        private static void ThrowNoSerializer<T>()
        {
            throw new SerializerNotFoundException($"A serializer could not be found for type {typeof(T)}");
        }
    }
}