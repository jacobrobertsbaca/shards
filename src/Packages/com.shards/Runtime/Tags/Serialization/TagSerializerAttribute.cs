using System;

namespace Shards.Tags.Serialization
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TagSerializerAttribute : Attribute
    {
        /// <summary>
        /// Whether this <see cref="TagSerializer"/> is ignored.
        /// If <c>true</c>, it will not be used to serialize types by default.
        /// </summary>
        public bool Ignore { get; set; } = false;

        /// <summary>
        /// <para>
        /// The priority of this <see cref="TagSerializer"/> when considered amongst
        /// other serializers for the same type.
        /// </para>
        ///
        /// <para>
        /// Serializers with lower priority values get preference over serializers
        /// with higher priorities.
        /// </para>
        /// </summary>
        public int Priority { get; set; } = 0;
    }
}