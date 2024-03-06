using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Shards.Tags
{
    public enum TagType : byte
    {
        None        = 0,
        Null        = 1,
        Compound    = 2,
        List        = 3,
        String      = 4,
        SByte       = 5,
        Byte        = 6,
        Int16       = 7,
        UInt16      = 8,
        Int32       = 9,
        UInt32      = 10,
        Int64       = 11,
        UInt64      = 12,
        Float       = 13,
        Double      = 14,
        Decimal     = 15,
        Bool        = 16,
        Blob        = 17
    }

    internal static class TagRegistry
    {
        private static readonly Dictionary<Type, TagType> TagTypeMap = new();

        public static ITag CreateTagFromType(TagType type) => type switch
        {
            TagType.Null => new NullTag(),
            TagType.Compound => new CompoundTag(),
            TagType.List => new ListTag(),
            TagType.String => new StringTag(),
            TagType.SByte => new SByteTag(),
            TagType.Byte => new ByteTag(),
            TagType.Int16 => new Int16Tag(),
            TagType.UInt16 => new UInt16Tag(),
            TagType.Int32 => new Int32Tag(),
            TagType.UInt32 => new UInt32Tag(),
            TagType.Int64 => new Int64Tag(),
            TagType.UInt64 => new UInt64Tag(),
            TagType.Float => new FloatTag(),
            TagType.Double => new DoubleTag(),
            TagType.Decimal => new DecimalTag(),
            TagType.Bool => new BoolTag(),
            TagType.Blob => new BlobTag(),
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

        public static TagType GetTypeOfTag<T>() where T : ITag => GetTypeOfTagType(typeof(T));

        public static TagType GetTypeOfTag(ITag tag) => GetTypeOfTagType(tag.GetType());
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    internal class TagAttribute : Attribute
    {
        public TagType Type { get; private set; }
        public TagAttribute(TagType type) => Type = type;
    }
}
