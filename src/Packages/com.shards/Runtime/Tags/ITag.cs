using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Shards.Tags
{
    public interface ITag
    {
        TagType Type => TagRegistry.GetTypeOfTag(this);

        /// <summary>
        /// Reads the tag's data into this object from <paramref name="reader"/>.
        /// </summary>
        /// <param name="reader">A binary reader</param>
        /// <remarks>
        /// Reads only the tag payload. Tags are not prefixed with their type.
        /// </remarks>
        void Read(BinaryReader reader);

        /// <summary>
        /// Writes the tag's data into <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">A binary writer</param>
        /// <remarks>
        /// Writes only the tag payload. Tags are not prefixed with their type.
        /// </remarks>
        void Write(BinaryWriter writer);
    }
}
