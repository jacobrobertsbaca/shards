using System;

namespace Shards
{
    public class ShardException : Exception
    {
        public ShardException(string message) : base(message) {}
    }
}