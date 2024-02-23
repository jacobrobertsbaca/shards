using System.Collections;
using System.Collections.Generic;

namespace Shards.Utility
{
    internal class Bimap<U, V> : ICollection<KeyValuePair<U, V>>, IEnumerable<KeyValuePair<U, V>>
    {
        private Dictionary<U, V> forward = new();
        private Dictionary<V, U> reverse = new();

        public Bimap() {}

        public bool Contains(U u) => forward.ContainsKey(u);
        public bool Contains(V v) => reverse.ContainsKey(v);

        public V this[U u]
        {
            get => forward[u];
            set
            {
                forward[u] = value;
                reverse[value] = u;
            }
        }

        public U this[V v]
        {
            get => reverse[v];
            set
            {
                forward[value] = v;
                reverse[v] = value;
            }
        }


        public void Add(U u, V v)
        {
            forward.Add(u, v);
            reverse.Add(v, u);
        }

        public bool Remove(U u)
        {
            if (!Contains(u)) return false;
            V v = forward[u];
            forward.Remove(u);
            reverse.Remove(v);
            return true;
        }

        public bool Remove(V v)
        {
            if (!Contains(v)) return false;
            U u = reverse[v];
            reverse.Remove(v);
            forward.Remove(u);
            return true;
        }

        public void Clear()
        {
            forward.Clear();
            reverse.Clear();
        }

        public int Count => forward.Count;
        public bool IsReadOnly => false;
        public void Add(KeyValuePair<U, V> item) => Add(item.Key, item.Value);
        public bool Contains(KeyValuePair<U, V> item) => Contains(item.Key);
        public void CopyTo(KeyValuePair<U, V>[] a, int i) => ((ICollection<KeyValuePair<U, V>>) forward).CopyTo(a, i);
        public bool Remove(KeyValuePair<U, V> item) => Remove(item.Key);
        public IEnumerator<KeyValuePair<U, V>> GetEnumerator() => forward.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => forward.GetEnumerator();
    }
}