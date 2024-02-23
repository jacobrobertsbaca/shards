using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Shards.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace Shards
{
    [ExecuteAlways]
    public class Fragment : MonoBehaviour, ISerializationCallbackReceiver
    {
        [SerializeField]
        private ReferenceTracker.Reference[] serializedRefs = new ReferenceTracker.Reference[0];
        private ReferenceTracker references = new();

        public void OnShardAdded(Shard shard)
        {
            references.TrackShard(shard);
            MarkSceneDirty();
        }

        public void OnShardRemoved(Shard shard)
        {
            references.UntrackShard(shard);
            MarkSceneDirty();
        }

        public void OnAfterDeserialize() => references.References = serializedRefs;
        public void OnBeforeSerialize() => serializedRefs = references.References;

        [System.Diagnostics.Conditional("UNITY_EDITOR")]
        private void MarkSceneDirty()
        {
            if (Application.IsPlaying(this)) return;
            PrefabUtility.RecordPrefabInstancePropertyModifications(this);
            EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }
    }
}