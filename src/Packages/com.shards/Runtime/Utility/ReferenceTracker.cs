using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Shards.Utility
{
    public class ReferenceTracker
    {
        [Serializable]
        public class Reference
        {
            [SerializeField] private string id;
            [SerializeField] private bool present;
            [SerializeField] private Shard shard;

            public string Id => id;
            public Shard Shard => shard;

            public bool Present
            {
                get => present;
                set => present = value;
            }

            public Reference(string id, Shard shard)
            {
                this.id = id;
                this.shard = shard;
            }
        }

        public Reference[] References
        {
            get
            {
                var refArray = refs.Values.Where(r => r.Shard != null).ToArray();
                Array.Sort(refArray, (a, b) => -a.Present.CompareTo(b.Present));
                return refArray;
            }

            set
            {
                refs.Clear();
                foreach (var sr in value)
                {
                    if (sr.Shard != null) refs.Add(sr.Id, sr);
                }
            }
        }

        private Dictionary<string, Reference> refs = new();

        public void TrackShard(Shard shard)
        {
            Debug.Assert(shard);
            Reference reference;

            if (!TryGetReference(shard, out reference))
            {
                string guid = Guid.NewGuid().ToString();
                reference = new Reference(guid, shard);
                refs.Add(guid, reference);
            }

            reference.Present = true;
        }

        public void UntrackShard(Shard shard)
        {
            Debug.Assert(shard);

            if (TryGetReference(shard, out var reference))
            {
                reference.Present = false;
            }
        }

        private bool TryGetReference(Shard shard, out Reference reference)
        {
            reference = refs.Values.Where(r => r.Shard == shard).SingleOrDefault();
            return reference is not null;
        }
    }
}