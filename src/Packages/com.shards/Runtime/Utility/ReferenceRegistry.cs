using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Shards.Utility
{
    public class ReferenceRegistry
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

            public bool IsValid()
            {
                if (shard == null) return false;
                if (string.IsNullOrWhiteSpace(id)) return false;
                return true;
            }
        }

        public Reference[] References
        {
            get
            {
                var refArray = refs.Values.Where(r => r.IsValid()).ToArray();
                Array.Sort(refArray, (a, b) => -a.Present.CompareTo(b.Present));
                return refArray;
            }

            set
            {
                refs.Clear();
                foreach (var sr in value)
                {
                    if (sr.IsValid()) refs.Add(sr.Id, sr);
                }
            }
        }

        private Dictionary<string, Reference> refs = new();

        /// <summary>
        /// Adds this shard as an active reference.
        /// </summary>
        /// <param name="shard">The shard to track.</param>
        /// <returns><c>true</c> if any state has changed.</returns>
        public bool TrackShard(Shard shard)
        {
            Debug.Assert(shard);
            Reference reference;

            if (!TryGetReference(shard, out reference))
            {
                string guid = Guid.NewGuid().ToString();
                reference = new Reference(guid, shard);
                refs.Add(guid, reference);
                reference.Present = true;
                return true;
            }

            bool wasPresent = reference.Present;
            reference.Present = true;
            return !wasPresent;
        }

        /// <summary>
        /// Removes this shard as an active reference.
        /// </summary>
        /// <param name="shard">The shard to stop tracking.</param>
        /// <returns><c>true</c> if any state has changed.</returns>
        public bool UntrackShard(Shard shard)
        {
            Debug.Assert(shard);

            if (TryGetReference(shard, out var reference))
            {
                bool wasPresent = reference.Present;
                reference.Present = false;
                return wasPresent;
            }

            return false;
        }

        private bool TryGetReference(Shard shard, out Reference reference)
        {
            reference = refs.Values.Where(r => r.Shard == shard).SingleOrDefault();
            return reference is not null;
        }
    }
}