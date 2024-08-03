using UnityEditor;
using UnityEngine;

namespace RunTimeAnimationEvent.Editor
{
    [CustomEditor(typeof(AnimationEventRunTime))]
    public class RunTimeAnimationEditor : UnityEditor.Editor
    {
        private SerializedProperty _animationEventList;

        private void OnEnable()
        {
            _animationEventList = serializedObject.FindProperty("animationStructs");
        }

        public override void OnInspectorGUI()
        {
            if (Application.isPlaying)
            {
                for (int i = 0; i < _animationEventList.arraySize; i++)
                {
                    var elementProperty = _animationEventList.GetArrayElementAtIndex(i);
                    var stringValueProperty = elementProperty.FindPropertyRelative("actionName");
                    var stringValue = stringValueProperty.stringValue;
                
                    EditorGUILayout.LabelField($"AnimationEvent: {stringValue} || Time: {elementProperty.FindPropertyRelative("eventTime").floatValue}");
                }

                EditorGUILayout.Space();
            }

            serializedObject.Update();
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();

            DrawDefaultInspector();
        }
    }
}