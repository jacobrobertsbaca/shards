using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shards
{
    public interface ISnapshotWriter
    {
        bool ContainsKey(string key);
        void Write<T>(string key, T value);
    }
}