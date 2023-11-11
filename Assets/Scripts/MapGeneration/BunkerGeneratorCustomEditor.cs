using UnityEditor;
using UnityEngine;

namespace MapGeneration.BunkerGeneration
{
    [CustomEditor(typeof(BunkerGenerator))]
    public class BunkerGeneratorCustomEditor : Editor
    {
        private const string maxNumCorridorsPath = "maxNumCorridors";
        SerializedProperty maxNumCorridors;
        private const string corridorExtensionProbabilityPath = "corridorExtensionProbability";
        SerializedProperty corridorExtensionProbability;
        private const string corridorLengthPath = "corridorLength";
        SerializedProperty corridorLength;

        private void OnEnable()
        {
            corridorExtensionProbability = serializedObject.FindProperty(corridorExtensionProbabilityPath);
            maxNumCorridors = serializedObject.FindProperty(maxNumCorridorsPath);
            corridorLength = serializedObject.FindProperty(corridorLengthPath);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(corridorLength);
            EditorGUILayout.PropertyField(maxNumCorridors);
            EditorGUILayout.PropertyField(corridorExtensionProbability);

            if (GUILayout.Button("Generate Bunker"))
            {
                ((BunkerGenerator)target).GenerateBunker();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}