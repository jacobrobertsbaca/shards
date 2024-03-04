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

    public abstract class ListSerializer<T> : TagSerializer<List<T>> {}

    public abstract class SetListSerializer<T> : ListSerializer<HashSet<T>> {}

    public class DictionarySerializer<V, K> : TagSerializer<Dictionary<K, V>>
    {
        public override Dictionary<K, V> Deserialize(ITag tag)
        {
            throw new NotImplementedException();
        }

        public override ITag Serialize(Dictionary<K, V> value)
        {
            throw new NotImplementedException();
        }
    }

    public class RandomSerializer<T, U, V> : TagSerializer<Dictionary<T, Dictionary<U, HashSet<V>>>>
    {
        public override Dictionary<T, Dictionary<U, HashSet<V>>> Deserialize(ITag tag)
        {
            throw new NotImplementedException();
        }

        public override ITag Serialize(Dictionary<T, Dictionary<U, HashSet<V>>> value)
        {
            throw new NotImplementedException();
        }
    }

    public static class TagSerializer
    {
        public static void WalkHierarchy(Type type)
        {
            if (type is null) return;
            Debug.Log($"\n{type}\n" +
                $"\tIs this a generic type definition? {type.IsGenericTypeDefinition}\n" +
                $"\tIs this a generic type? {type.IsGenericType}\n" +
                $"\tGeneric type definition: {(type.IsGenericType ? type.GetGenericTypeDefinition() : null)}\n" +
                $"\tType arguments: {string.Join<Type>(", ", type.GetGenericArguments())}");
            WalkHierarchy(type.BaseType);

           
        }

        public static T Deserialize<T>(ITag tag)
        {
            return default;
        }

        public static ITag Serialize<T>(T value)
        {
            return null;
        }
    }
}