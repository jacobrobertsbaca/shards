namespace Shards.Tags
{
    public abstract class TagSerializer<T>
    {
        public abstract bool TryDeserialize(ITag tag, out T value);
        public abstract ITag Serialize(T value);
    }

    public static class TagSerializer
    {
        public static bool TryDeserialize<T>(ITag tag, out T value)
        {
            value = default;
            return false;
        }

        public static ITag Serialize<T>(T value)
        {
            return null;
        }
    }
}