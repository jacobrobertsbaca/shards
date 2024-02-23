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
        private Fragment lastFragment;

        private void OnEnable()
        {
            if (Fragment) Fragment.OnShardAdded(this);
            lastFragment = Fragment;
        }

        private void OnTransformParentChanged()
        {
            if (isActiveAndEnabled && lastFragment) lastFragment.OnShardRemoved(this);
            fragmentCached = false;
            if (isActiveAndEnabled && Fragment) Fragment.OnShardAdded(this);
            lastFragment = Fragment;
        }

        private void OnDisable()
        {
            if (Fragment) Fragment.OnShardRemoved(this);
        }
    }
}