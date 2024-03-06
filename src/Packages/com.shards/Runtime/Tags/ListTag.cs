using System.Collections.Generic;
using System.IO;

namespace Shards.Tags
{
    [Tag(TagType.List)]
    public sealed class ListTag : List<ITag>, ITag
    {
        public TagType ItemType => Count > 0 ? this[0].Type : TagType.None;

        public ListTag() : base() {}
        public ListTag(params ITag[] contents) : base(contents) {}
        public ListTag(IEnumerable<ITag> contents) : base(contents) {}

        public void Read(BinaryReader reader)
        {
            Clear();

            int count = reader.ReadInt32();
            if (count == 0) return;

            TagType itemType = (TagType)reader.ReadByte();
            
            for (int i = 0; i < count; i++)
            {
                ITag value = TagRegistry.CreateTagFromType(itemType);
                value.Read(reader);
                Add(value);
            }
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Count);
            if (Count == 0) return;

            writer.Write((byte) ItemType);

            foreach (ITag value in this)
            {
                value.Write(writer);
            }
        }
    }
}
