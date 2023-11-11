using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Utility;

namespace MapGeneration.VillageGeneration
{
    [CustomEditor(typeof(VillageBuildingsCollection))]
    public class VillageBuildingsCollectionCustomEditor : Editor
    {
        private const string widthPath = "buildingWidth";
        SerializedProperty width;

        private List<VillageBuilding> buildingsList;
        private VillageBuilding buildingToBeAdded;
        private Vector2 buildingsScrollPos;

        private void OnEnable()
        {
            width = serializedObject.FindProperty(widthPath);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.PropertyField(width);

            buildingsList = ((VillageBuildingsCollection)serializedObject.targetObject).GetBuildings();

            //------------------------------------------show buildings

            EditorGUILayout.LabelField("Colletion Buildings:");

            VillageBuilding v;

            buildingsScrollPos = 
                EditorGUILayout.BeginScrollView(buildingsScrollPos, GUILayout.MinHeight(0), GUILayout.MaxHeight(300));

            for (int i = 0; i < buildingsList.Count; i++)
            {
                v = buildingsList[i];

                LayoutVillageBuilding(v);
            }

            EditorGUILayout.EndScrollView();

            //---------------------------------add new building

            if (buildingToBeAdded == null) { buildingToBeAdded = new(); }

            if (GUILayout.Button("Add Building"))
            {
                buildingsList.Add(buildingToBeAdded);
                buildingToBeAdded = null;
            }

            if (GUILayout.Button("Clear Buildings"))
            {
                ((VillageBuildingsCollection)serializedObject.targetObject).ClearBuildingsList();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void LayoutVillageBuilding(VillageBuilding v)
        {
            EditorGUILayout.BeginVertical();
            v.name = EditorGUILayout.TextField(v.name);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();
                v.b00 = EditorGUILayout.Toggle(v.b00);
                v.b01 = EditorGUILayout.Toggle(v.b01);
                v.b02 = EditorGUILayout.Toggle(v.b02);
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                v.b10 = EditorGUILayout.Toggle(v.b10);
                v.b11 = EditorGUILayout.Toggle(v.b11);
                v.b12 = EditorGUILayout.Toggle(v.b12);
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                v.b20 = EditorGUILayout.Toggle(v.b20);
                v.b21 = EditorGUILayout.Toggle(v.b21);
                v.b22 = EditorGUILayout.Toggle(v.b22);
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();
                v.possibleObjects = (ObjectPool)EditorGUILayout.ObjectField(v.possibleObjects, typeof(ObjectPool), true);

                if (GUILayout.Button("Remove"))
                {
                    buildingsList.Remove(v);
                }

                if (GUILayout.Button("Duplicate"))
                {
                    buildingsList.Add(new VillageBuilding(v));
                }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }
    }
}