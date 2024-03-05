namespace Shards.Tags.Serialization
{
    public class SerializerNotFoundException : ShardException
    {
        public SerializerNotFoundException(string message) : base(message) { }
    }
}