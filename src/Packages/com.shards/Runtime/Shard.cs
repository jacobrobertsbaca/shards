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
        private static readonly int kGlobalIdPrefixLen = "GlobalObjectId_V1-2-".Length;

        [SerializeField] private string guid;
        public string Identifier => guid;

        private void Awake()
        {
            TryAssignIdentifier();
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
                    guid = $"Dynamic {{{System.Guid.NewGuid()}}}";
            }
            else
            {
                // Editor
#if UNITY_EDITOR
                string globalId = GetStaticIdentifier();

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

#if UNITY_EDITOR
        private string GetStaticIdentifier()
        {
            // Prefab assets always have null identifiers so that they get assigned
            // a dynamic identifier when initialized
            if (PrefabUtility.IsPartOfPrefabAsset(this)) return null;
            if (PrefabStageUtility.GetPrefabStage(gameObject)) return null;

            // Get a persistent identifier for the object
            // If the identifierType is not equal to 2 (scene object), then
            // this is not a scene object (could be part of a prefab, ScriptableObject, etc.)
            GlobalObjectId globalId = GlobalObjectId.GetGlobalObjectIdSlow(gameObject);
            if (globalId.identifierType != 2) return null;

            return $"Static {{{globalId.ToString().Substring(kGlobalIdPrefixLen)}}}";
        }
#endif
    }
}
