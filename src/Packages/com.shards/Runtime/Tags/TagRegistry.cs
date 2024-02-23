using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Shards.Tags
{
    public enum TagType : byte
    {
        None,
        Compound,
        List,
        String,
        Byte,
        Int16,
        Int32,
        Int64,
        Float,
        Double,
        Bool
    }

    public static class TagRegistry
    {
        private static readonly Dictionary<Type, TagType> TagTypeMap = new();

        public static ITag CreateTagFromType(TagType type) => type switch
        {
            TagType.None => null,
            TagType.Compound => new CompoundTag(),
            TagType.List => new ListTag(),
            TagType.String => new StringTag(),
            TagType.Byte => new ByteTag(),
            TagType.Int16 => new Int16Tag(),
            TagType.Int32 => new Int32Tag(),
            TagType.Int64 => new Int64Tag(),
            TagType.Float => new FloatTag(),
            TagType.Double => new DoubleTag(),
            TagType.Bool => new BoolTag(),
            _ => throw new ArgumentException($"No such tag type: {type}")
        };

        public static TagType GetTypeOfTagType(Type type)
        {
            if (TagTypeMap.TryGetValue(type, out TagType value)) return value;
            TagAttribute attr = type.GetCustomAttribute<TagAttribute>();
            TagType tagType = attr is null ? TagType.None : attr.Type;
            TagTypeMap.Add(type, tagType);
            return tagType;
        }

        public static TagType GetTypeOfTag(ITag tag) => GetTypeOfTagType(tag.GetType());
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class TagAttribute : Attribute
    {
        public TagType Type { get; private set; }
        public TagAttribute(TagType type) => Type = type;
    }
}
