using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Shards.Tags
{
    [Tag(TagType.Compound)]
    public sealed class CompoundTag : Dictionary<string, ITag>, ITag
    {
        public void Read(BinaryReader reader)
        {
            Clear();

            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                string key = reader.ReadString();
                TagType type = (TagType) reader.ReadByte();
                ITag value = TagRegistry.CreateTagFromType(type);
                value.Read(reader);
                Add(key, value);
            }
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Count);
            foreach (var kv in this)
            {
                writer.Write(kv.Key);
                writer.Write((byte) kv.Value.Type);
                kv.Value.Write(writer);
            }
        }
    }
}
