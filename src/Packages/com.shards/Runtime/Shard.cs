using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Shards
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class Shard : MonoBehaviour
    {
        [SerializeField] private string guid;
        public string Identifier => guid;

        private void Awake()
        {
            TryAssignIdentifier();
        }

        private void Update()
        {
            if (!Application.IsPlaying(gameObject)) TryAssignIdentifier();
        }

#if UNITY_EDITOR
        private void OnValidate() => TryAssignIdentifier();
#endif

        private void TryAssignIdentifier()
        {
            if (Application.IsPlaying(gameObject))
            {
                // Play mode
                if (string.IsNullOrWhiteSpace(guid))
                    guid = System.Guid.NewGuid().ToString();
            }
            else
            {
                // Editor
#if UNITY_EDITOR
                var globalId = GlobalObjectId.GetGlobalObjectIdSlow(gameObject).ToString();

                if (globalId != guid)
                {
                    guid = globalId;
                    if (PrefabUtility.IsPartOfPrefabInstance(this))
                    {
                        // If we're a part of a prefab, we need to set globalId and ensure
                        // that the property is marked as an override.
                        PrefabUtility.RecordPrefabInstancePropertyModifications(this);
                    }

                    // Mark scene dirty
                    EditorSceneManager.MarkSceneDirty(gameObject.scene);
                }
#endif
            }
        }
    }
}
