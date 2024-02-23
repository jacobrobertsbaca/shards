using System.Collections.Generic;
using System.IO;

namespace Shards.Tags
{
    [Tag(TagType.List)]
    public sealed class ListTag : List<ITag>, ITag
    {
        public void Read(BinaryReader reader)
        {
            Clear();
            
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                TagType itemType = (TagType) reader.ReadByte();
                ITag value = TagRegistry.CreateTagFromType(itemType);
                value.Read(reader);
                Add(value);
            }
         }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Count);
            foreach (ITag value in this)
            {
                writer.Write((byte) value.Type);
                value.Write(writer);
            }
        }
    }
}
