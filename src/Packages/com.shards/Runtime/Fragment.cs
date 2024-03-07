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
using Unity.EditorCoroutines.Editor;
#endif

namespace Shards
{
    [ExecuteAlways]
    public class Fragment : MonoBehaviour, ISerializationCallbackReceiver
    {
        [SerializeField]
        private ReferenceRegistry.Reference[] serializedRefs;
        private ReferenceRegistry references = new();

        private void Awake()
        {
            foreach (var shard in GetComponentsInChildren<Shard>())
                shard.NotifyFragmentMayChange();
        }

        public void OnShardAdded(Shard shard)
        {
            if (references.TrackShard(shard)) MarkSceneDirty();
        }

        public void OnShardRemoved(Shard shard)
        {
            references.UntrackShard(shard);
        }

        public void OnAfterDeserialize() => references.SetReferences(serializedRefs);
        public void OnBeforeSerialize() => serializedRefs = references.GetReferences();

        private void MarkSceneDirty()
        {
#if UNITY_EDITOR

            // For some reason Unity does not allow marking the scene as dirty
            // on the first frame.
            //
            // This allows us to get around that by delaying marking the scene dirty
            // by an editor frame in case new references were added on startup.

            IEnumerator Coroutine()
            {
                yield return null;
                PrefabUtility.RecordPrefabInstancePropertyModifications(this);
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }

            if (Application.IsPlaying(this)) return;
            EditorCoroutineUtility.StartCoroutine(Coroutine(), this);
#endif
        }
    }
}