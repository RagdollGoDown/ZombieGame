using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Utility
{
    [CustomEditor(typeof(ObjectPool))]
    public class ObjectPoolEditor : Editor
    {
        private const string initialNumberPath = "initialNumber";
        SerializedProperty initialNumber;
        private const string possibleObjectsPath = "possibleObjects";
        SerializedProperty possibleObjects;

        void OnEnable()
        {
            initialNumber = serializedObject.FindProperty(initialNumberPath);
            possibleObjects = serializedObject.FindProperty(possibleObjectsPath);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(initialNumber);
            EditorGUILayout.PropertyField(possibleObjects);

            if (GUILayout.Button("Ready Pool"))
            {
                ((ObjectPool)target).ReadyInitialObjects();
            }

            if (GUILayout.Button("Empty Pool"))
            {
                ((ObjectPool)target).EmptyObjects();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
