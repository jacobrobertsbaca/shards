//using System.Collections;
//using System.Collections.Generic;
//using UnityEditor;
//using UnityEngine;

//namespace Shards.Editor
//{
//    [CustomEditor(typeof(Shard))]
//    [CanEditMultipleObjects]
//    public class ShardEditor : UnityEditor.Editor
//    {
//        SerializedProperty guid;

//        void OnEnable()
//        {
//            guid = serializedObject.FindProperty("guid");
//        }

//        public override void OnInspectorGUI()
//        {
//            using (new EditorGUI.DisabledScope(true))
//            {
//                EditorGUILayout.PropertyField(guid, new GUIContent("Identifier"));
//            }
//            serializedObject.ApplyModifiedProperties();
//        }
//    }
//}
