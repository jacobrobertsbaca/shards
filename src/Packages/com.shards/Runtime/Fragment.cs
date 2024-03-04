using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Shards.Utility;
using UnityEngine;
using UnityEngine.SceneManagement;
using Shards.Tags;
using Shards.Tags.Serialization;

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

        private void Awake()
        {
            if (Application.IsPlaying(this))
            {
                // TagSerializer.WalkHierarchy(typeof(SetListSerializer<>));

                //foreach (var type in SerializerRegistry.GetTypeExpansions(typeof (Dictionary<string, Dictionary<string, HashSet<int>>>)))
                //{
                //    Debug.Log(type);
                //}

                //Type objectType = typeof(Dictionary<string, List<bool>>);
                //Type templateType = typeof(Dictionary<,>);
                //foreach (var kv in SerializerRegistry.GetGenericTypeMap(objectType, templateType))
                //    Debug.Log(kv);

                //var serializerType = SerializerRegistry.BindSerializerType(
                //    typeof(Dictionary<string, bool>),
                //    typeof(Dictionary<,>),
                //    typeof(DictionarySerializer<,>)
                //);
                //Debug.Log(serializerType);

                var reg = new SerializerRegistry();
                var serializer = reg.Get<Dictionary<string, Dictionary<List<int>, HashSet<bool>>>>();
                int x = 1;
            }
        }

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