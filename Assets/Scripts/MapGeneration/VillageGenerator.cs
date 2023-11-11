using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace MapGeneration.VillageGeneration
{
    public class VillageGenerator : MonoBehaviour
    {
        [SerializeField] private int size;

        [SerializeField] private float density;

        private bool[,] buildingConditionalArrays;

        [SerializeField] private VillageBuildingsCollection villageBuildingsCollection;

        public void Generate()
        {
            if (villageBuildingsCollection == null) throw new System.Exception("Need to have a village collection component");
            if (size < 3) throw new System.ArgumentException("Size must be bigger than 2");
            if (density < 0 || density > 1) throw new System.ArgumentException("Density must be between 0 and 1");

            buildingConditionalArrays = new bool[size, size];
            villageBuildingsCollection.ClearBuildingsOnMap();

            Debug.Log("Bool map setup");
            //generate random bool map
            float rand;

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    rand = Random.Range(0f, 1f);

                    if (rand <= density)
                    {
                        buildingConditionalArrays[i, j] = true;
                    }
                }
            }

            Debug.Log("Building placement");
            //convert bool map to buildings using buildings collection
            float width = villageBuildingsCollection.GetWidth();

            for (int i = 0; i < size - 2; i++)
            {
                for (int j = 0; j < size - 2; j++)
                {
                    GameObject building = villageBuildingsCollection.Get(
                        ExtractArray(buildingConditionalArrays, i, j, 3));
                    
                    if (building != null)
                    {
                        building.transform.position = new Vector3((i+1) * width, 0, (j+1) * width);
                    }
                }
            }

            Debug.Log("Generation Done");
        }

        private bool[,] ExtractArray(bool[,] baseArray, int start0, int start1, int radius)
        {
            bool[,] extract = new bool[radius, radius];

            for (int i2 = 0; i2 < radius; i2++)
            {
                for (int j2 = 0; j2 < radius; j2++)
                {
                    extract[i2, j2] = baseArray[i2 + start0, j2 + start1];
                }
            }

            return extract;
        }
    }
}