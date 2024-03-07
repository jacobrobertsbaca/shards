using System;
using UnityEngine;

namespace Shards
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class Shard : MonoBehaviour
    {
        public Fragment Fragment
        {
            get
            {
                if (fragmentCached) return fragment;
                fragment = GetComponentInParent<Fragment>();
                fragmentCached = true;
                return fragment;
            }
        }

        private bool fragmentCached = false;
        private Fragment fragment;

        private void OnEnable()
        {
            if (Fragment) Fragment.OnShardAdded(this);
        }

        private void OnBeforeTransformParentChanged()
        {
            if (isActiveAndEnabled && Fragment) Fragment.OnShardRemoved(this);
        }

        private void OnTransformParentChanged()
        {
            fragmentCached = false;
            if (isActiveAndEnabled && Fragment) Fragment.OnShardAdded(this);
        }

        private void OnDisable()
        {
            if (Fragment) Fragment.OnShardRemoved(this);
        }

        internal void NotifyFragmentMayChange()
        {
            OnBeforeTransformParentChanged();
            OnTransformParentChanged();
        }
    }
}