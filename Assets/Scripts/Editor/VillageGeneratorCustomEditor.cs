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
        private VillageGenerator target;

        private const string sizePath = "size";
        private SerializedProperty size;
        private readonly static int BASE_SIZE = 10;

        //generation
        private const string generationMethodPath = "generationMethod";
        private SerializedProperty generationMethod;

        //when generating random
        private const string densityPath = "density";
        private SerializedProperty density;
        private readonly static float BASE_DENSITY = 0.5f;

        //when generating in corridors
        private const string forwardProbaPath = "forwardProbability";
        private SerializedProperty forwardProbability;
        private const string turnProbaPath = "turnProbability";
        private SerializedProperty turnProbability;

        private const string onlyCorridorsNoOpenSpacesPath = "onlyCorridorsNoOpenSpaces";
        private SerializedProperty onlyCorridorsNoOpenSpaces;

        private const string vbcPath = "villageBuildingsCollection";
        private SerializedProperty vbc;

        private void OnEnable()
        {
            target = (VillageGenerator)serializedObject.targetObject;

            size = serializedObject.FindProperty(sizePath);
            density = serializedObject.FindProperty(densityPath);

            generationMethod = serializedObject.FindProperty(generationMethodPath);

            turnProbability = serializedObject.FindProperty(turnProbaPath);
            forwardProbability = serializedObject.FindProperty(forwardProbaPath);
            onlyCorridorsNoOpenSpaces = serializedObject.FindProperty(onlyCorridorsNoOpenSpacesPath);

            vbc = serializedObject.FindProperty(vbcPath);
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

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.PropertyField(generationMethod);

            switch (generationMethod.enumValueIndex)
            {
                case 1:
                    CorridorsGeneration();
                    break;
                default:
                    RandomGeneration();
                    break;
            }

            EditorGUILayout.PropertyField(vbc);

            ShowBorderConditionalArray(target);

            if (GUILayout.Button("Generate Village"))
            {
                target.Generate();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void RandomGeneration()
        {
            EditorGUILayout.PropertyField(density);

            if (density.floatValue < 0 || density.floatValue > 1)
            {
                density.floatValue = BASE_DENSITY;
            }
        }

        private void CorridorsGeneration()
        {
            EditorGUILayout.PropertyField(turnProbability);
            EditorGUILayout.PropertyField(forwardProbability);
            EditorGUILayout.PropertyField(onlyCorridorsNoOpenSpaces);

            if (turnProbability.floatValue < 0)
            {
                turnProbability.floatValue = 0;
            }

            if (forwardProbability.floatValue < 0)
            {
                forwardProbability.floatValue = 0;
            }
        }

        private void ShowBorderConditionalArray(VillageGenerator v)
        {
            if (v.borderConditionalArray == null || v.borderConditionalArray.Length != 9) v.borderConditionalArray = new bool[9];

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical();
            v.borderConditionalArray[0] = EditorGUILayout.Toggle(v.borderConditionalArray[0]);
            v.borderConditionalArray[1] = EditorGUILayout.Toggle(v.borderConditionalArray[1]);
            v.borderConditionalArray[2] = EditorGUILayout.Toggle(v.borderConditionalArray[2]);
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical();
            v.borderConditionalArray[3] = EditorGUILayout.Toggle(v.borderConditionalArray[3]);
            v.borderConditionalArray[4] = EditorGUILayout.Toggle(v.borderConditionalArray[4]);
            v.borderConditionalArray[5] = EditorGUILayout.Toggle(v.borderConditionalArray[5]);
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical();
            v.borderConditionalArray[6] = EditorGUILayout.Toggle(v.borderConditionalArray[6]);
            v.borderConditionalArray[7] = EditorGUILayout.Toggle(v.borderConditionalArray[7]);
            v.borderConditionalArray[8] = EditorGUILayout.Toggle(v.borderConditionalArray[8]);
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

        }
    }
}