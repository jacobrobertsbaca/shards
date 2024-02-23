using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Shards
{
    public interface ISnapshotReader
    {
        bool ContainsKey(string key);
        T Read<T>(string key);
    }
}