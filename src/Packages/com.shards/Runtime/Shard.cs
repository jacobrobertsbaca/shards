using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Shards
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class Shard : MonoBehaviour
    {
        private enum StaticMode
        {
            Unassigned,
            Static,
            Dynamic
        }

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

        public bool IsStatic => staticMode == StaticMode.Static;

        [SerializeField]
        private StaticMode staticMode = StaticMode.Unassigned;

        private bool fragmentCached = false;
        private Fragment fragment;

        private void Awake()
        {
            AssignStaticMode();
        }

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

        private void OnValidate()
        {
            AssignStaticMode();
        }

        private void AssignStaticMode()
        {
            if (Application.IsPlaying(this))
            {
                // Play Mode
                if (staticMode == StaticMode.Unassigned)
                    staticMode = StaticMode.Dynamic;
            }
            else
            {
                // Editor
#if UNITY_EDITOR
                StaticMode GetStaticMode()
                {
                    // Prefab assets always are always marked Unassigned so that they
                    // become Static when instantiated
                    if (PrefabUtility.IsPartOfPrefabAsset(this)) return StaticMode.Unassigned;
                    if (PrefabStageUtility.GetPrefabStage(gameObject)) return StaticMode.Unassigned;
                    return StaticMode.Static;
                }

                StaticMode preferredMode = GetStaticMode();

                if (preferredMode != staticMode)
                {
                    staticMode = preferredMode;
                    PrefabUtility.RecordPrefabInstancePropertyModifications(this);
                    EditorSceneManager.MarkSceneDirty(gameObject.scene);
                }
#endif
            }
        }

        internal void NotifyFragmentMayChange()
        {
            OnBeforeTransformParentChanged();
            OnTransformParentChanged();
        }
    }
}