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
        private ReferenceRegistry.Reference[] serializedRefs = new ReferenceRegistry.Reference[0];
        private ReferenceRegistry references = new();

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

        private void MarkSceneDirty()
        {
#if UNITY_EDITOR
            if (Application.IsPlaying(this)) return;
            PrefabUtility.RecordPrefabInstancePropertyModifications(this);
            EditorSceneManager.MarkSceneDirty(gameObject.scene);
#endif
        }
    }
}