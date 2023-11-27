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

        //objectives
        private const string numberOfObjectivesPath = "numberOfObjectives";
        SerializedProperty numberOfObjectives;

        private const string objectiveRadiusPath = "objectiveRadius";
        SerializedProperty objectiveRadius;

        private const string possibleObjectivesPath = "possibleObjectives";
        SerializedProperty possibleObjectives;

        //generation
        private const string generationMethodPath = "generationMethod";
        SerializedProperty generationMethod;

        //when generating random
        private const string densityPath = "density";
        SerializedProperty density;
        private readonly static float BASE_DENSITY = 0.5f;

        //when generating in corridors
        private const string forwardProbaPath = "forwardProbability";
        SerializedProperty forwardProbability;
        private const string turnProbaPath = "turnProbability";
        SerializedProperty turnProbability;

        private const string vbcPath = "villageBuildingsCollection";
        SerializedProperty vbc;

        private void OnEnable()
        {
            size = serializedObject.FindProperty(sizePath);
            density = serializedObject.FindProperty(densityPath);

            numberOfObjectives = serializedObject.FindProperty(numberOfObjectivesPath);
            objectiveRadius = serializedObject.FindProperty(objectiveRadiusPath);
            possibleObjectives = serializedObject.FindProperty(possibleObjectivesPath);

            generationMethod = serializedObject.FindProperty(generationMethodPath);

            turnProbability = serializedObject.FindProperty(turnProbaPath);
            forwardProbability = serializedObject.FindProperty(forwardProbaPath);

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

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Objectives");

            EditorGUILayout.PropertyField(numberOfObjectives);
            EditorGUILayout.PropertyField(objectiveRadius);
            EditorGUILayout.PropertyField(possibleObjectives);
            EditorGUILayout.EndVertical();


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

            if (GUILayout.Button("Generate Village"))
            {
                ((VillageGenerator)serializedObject.targetObject).Generate();
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

            if (turnProbability.floatValue < 0)
            {
                turnProbability.floatValue = 0;
            }

            if (forwardProbability.floatValue < 0)
            {
                forwardProbability.floatValue = 0;
            }
        }
    }
}