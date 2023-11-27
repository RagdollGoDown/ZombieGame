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
        private VillageBuildingsCollection target;

        private const string widthPath = "buildingWidth";
        SerializedProperty width;

        private List<VillageBuilding> buildingsList;
        private VillageBuilding buildingToBeAdded;
        private Vector2 buildingsScrollPos;

        private void OnEnable()
        {
            target = ((VillageBuildingsCollection)serializedObject.targetObject);
            width = serializedObject.FindProperty(widthPath);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.PropertyField(width);

            buildingsList = target.GetBuildings();

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

            serializedObject.ApplyModifiedProperties();
        }

        private void LayoutVillageBuilding(VillageBuilding v)
        {
            EditorGUILayout.BeginVertical();
            v.name = EditorGUILayout.TextField(v.name);
            v.possibleObjects = (ObjectPool)EditorGUILayout.ObjectField(v.possibleObjects, typeof(ObjectPool), true);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.BeginVertical();
                v.conditionalArray[0].value = (HandyBoolValues)EditorGUILayout.EnumPopup(v.conditionalArray[0].value);
                v.conditionalArray[1].value = (HandyBoolValues)EditorGUILayout.EnumPopup(v.conditionalArray[1].value);
                v.conditionalArray[2].value = (HandyBoolValues)EditorGUILayout.EnumPopup(v.conditionalArray[2].value);
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                v.conditionalArray[3].value = (HandyBoolValues)EditorGUILayout.EnumPopup(v.conditionalArray[3].value);
                v.conditionalArray[4].value = (HandyBoolValues)EditorGUILayout.EnumPopup(v.conditionalArray[4].value);
                v.conditionalArray[5].value = (HandyBoolValues)EditorGUILayout.EnumPopup(v.conditionalArray[5].value);
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                v.conditionalArray[6].value = (HandyBoolValues)EditorGUILayout.EnumPopup(v.conditionalArray[6].value);
                v.conditionalArray[7].value = (HandyBoolValues)EditorGUILayout.EnumPopup(v.conditionalArray[7].value);
                v.conditionalArray[8].value = (HandyBoolValues)EditorGUILayout.EnumPopup(v.conditionalArray[8].value);
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();

                if (GUILayout.Button("Remove"))
                {
                    buildingsList.Remove(v);
                }

                if (GUILayout.Button("Duplicate"))
                {
                    buildingsList.Add(new VillageBuilding(v));
                }

                if (GUILayout.Button("Rotate"))
                {
                    v.Rotate();
                }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }
    }
}