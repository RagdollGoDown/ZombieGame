using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Utility;

namespace MapGeneration.VillageGeneration
{
    [CustomEditor(typeof(VillageGenerator))]
    public class VillageGeneratorCustomEditor : Editor
    {
        private const string sizePath = "size";
        SerializedProperty size;
        private readonly static int BASE_SIZE = 10;

        private const string densityPath = "density";
        SerializedProperty density;
        private readonly static float BASE_DENSITY = 0.5f;

        private void OnEnable()
        {
            size = serializedObject.FindProperty(sizePath);
            density = serializedObject.FindProperty(densityPath);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(size);

            if (size.intValue < 3)
            {
                size.intValue = BASE_SIZE;
            }

            EditorGUILayout.PropertyField(density);

            if (density.floatValue < 0 || density.floatValue > 1)
            {
                density.floatValue = BASE_DENSITY;
            }

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("Generate Village"))
            {
                ((VillageGenerator)serializedObject.targetObject).Generate();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}