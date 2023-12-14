using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Objectives;
using Utility;
using Utility.Observable;

namespace MapGeneration.VillageGeneration
{
    public class VillageGenerator : MonoBehaviour
    {
        [SerializeField] private int size;
        private float width;

        [SerializeField] private int numberOfObjectives = 4;
        [SerializeField] private int objectiveRadius = 3;
        [SerializeField] private ObjectPool[] possibleObjectives;
        private List<Vector3Int> objectivePositions;

        private bool[,] buildingConditionalArrays;

        [SerializeField] private VillageBuildingsCollection villageBuildingsCollection;

        public bool[] borderConditionalArray;

        private bool[,] mask;

        public enum GenerationMethod
        {
            Random,
            Corridors
        }

        //randomGeneration
        [SerializeField] private float density;

        //corridorsGeneration
        [SerializeField] private float turnProbability;
        [SerializeField] private float forwardProbability;

        [SerializeField] private GenerationMethod generationMethod;

        public void Generate()
        {
            Generate(0);
        }

        /// <summary>
        /// Generates the village first by making a 2 dimensional bool array 
        /// then using the village collection places the buildings
        /// When this function is used with width != 0 this means that is is being used as a building
        /// meaning it's og size will overiden to make it fit in the bigger generator and the mask will make it
        /// go only in the places given by the bigger generator
        /// </summary>
        /// <param name="UpperWidth"> the width of the upper generator</param>
        public void Generate(float UpperWidth)
        {
            //if the mask is null then it has already been used
            if (UpperWidth != 0 && mask == null) return;
            if (villageBuildingsCollection == null) throw new System.Exception("Need to have a village collection component");
            if (numberOfObjectives != 0 && (possibleObjectives == null || possibleObjectives.Length == 0)) throw new System.Exception("Need to have at least one objective to choose from");
            if (size < 3) throw new System.ArgumentException("Size must be bigger than 2");
            if (density < 0 || density > 1) throw new System.ArgumentException("Density must be between 0 and 1");
            
            width = villageBuildingsCollection.GetWidth();
            
            if (UpperWidth % width != 0) throw new System.Exception("Sub generator with width not divideable by upper generator width given");

            size = UpperWidth == 0 ? size : mask.GetLength(0) * (int)(UpperWidth / width);

            buildingConditionalArrays = new bool[size+2, size+2];
            villageBuildingsCollection.ReadyCollection(size);

            //objectives
            foreach(ObjectPool pool in possibleObjectives)
            {
                pool.ReadyInitialObjects(numberOfObjectives);
                if (!pool) { throw new System.Exception("Null pool in possible objectives"); }
            }

            objectivePositions = new();

            PlaceObjectives();

            GenerateConditionalBoolArray();

            if (mask != null)
            {
                int divide = (int)(UpperWidth / width);

                for (int i = 0; i < mask.GetLength(0); i++)
                {
                    for (int j = 0; j < mask.GetLength(1); j++)
                    {
                        if (!mask[i, j])
                        {
                            FillExtract(buildingConditionalArrays, i * divide, j * divide, divide, false);
                        }
                    }
                }

                mask = null;
            }

            GenerateBorders();

            PlaceBuildings();

            villageBuildingsCollection.FinishCollection(width);

            Debug.Log("Generation Done");
        }

        //--------------------------------------------------utility functions
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

        private void FillExtract(bool[,] baseArray, int start0, int start1, int radius, bool fill)
        {
            for (int i2 = 0; i2 < radius; i2++)
            {
                for (int j2 = 0; j2 < radius; j2++)
                {
                    baseArray[i2 + start0, j2 + start1] = fill;
                }
            }
        }

        //-------------------------------------------------------------unity events
        private void OnDrawGizmosSelected()
        {
            if (buildingConditionalArrays != null)
            {
                float width = villageBuildingsCollection.GetWidth();

                Color color;
                Vector3 origin;

                for (int i = 0; i < size; i++)
                {
                    for (int j = 0; j < size; j++)
                    {
                        color = buildingConditionalArrays[i, j] ? Color.white : Color.black;
                        origin = new Vector3((i) * width - ((size-1) * width / 2), 0, (j) * width - ((size-1) * width / 2));
                        Gizmos.color = color;
                        Gizmos.DrawLine(origin, origin + Vector3.up * 5);
                    }
                }
            }
        }

        //-------------------------------------------------------generating methods

        private void GenerateBuildingConditionalArrayRandom()
        {
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    buildingConditionalArrays[i, j] = Random.Range(0,1.0f) <= density;
                }
            }
        }

        private void GenerateBuildingConditionalArrayCorridors()
        {
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    buildingConditionalArrays[i, j] = true;
                }
            }

            float rand;

            void continueCorridor(int x, int y, int fromx, int fromy){

                if (x <= 1 || y <= 1 || x >= size -2 || y >= size -2 || !buildingConditionalArrays[x,y])
                {
                    return;
                }

                buildingConditionalArrays[x, y] = false;

                rand = Random.Range(0,1.0f);

                //go forward
                if (rand <= forwardProbability)
                {
                    continueCorridor(2 * x - fromx, 2 * y - fromy, x, y);
                }

                rand = Random.Range(0,1.0f);

                //turn right
                if (rand <= turnProbability)
                {
                    continueCorridor(x + y - fromy, y + x - fromx, x, y);
                }

                rand = Random.Range(0,1.0f);

                //turn left
                if (rand <= turnProbability)
                {
                    continueCorridor(x - y + fromy, y - x + fromx, x, y);
                }
            }

            int halfSize = size / 2;
            buildingConditionalArrays[halfSize, halfSize] = false;

            continueCorridor(halfSize, halfSize - 1, halfSize, halfSize);
            continueCorridor(halfSize + 1, halfSize, halfSize, halfSize);
            continueCorridor(halfSize, halfSize + 1, halfSize, halfSize);
            continueCorridor(halfSize - 1, halfSize, halfSize, halfSize);

            //this is meant to ensure that the objectives are linked to the rest of the corridors
            foreach(Vector3Int position in objectivePositions)
            {
                //we try to send a corridor somewhat in the center to connect the objectives to it
                if (halfSize - position.x < 0)
                {
                    continueCorridor(position.x - 1, position.y, position.x, position.y);
                }
                else
                {
                    continueCorridor(position.x + 1, position.y, position.x, position.y);
                }

                FillExtract(buildingConditionalArrays, 
                    position.x-(int)Mathf.Floor(objectiveRadius/2), position.y-(int)Mathf.Floor(objectiveRadius / 2), objectiveRadius, false);
            }
        }
        
        //-------------------------------------------generation steps

        /// <summary>
        /// Places the objectives
        /// Does not need the conditional bool array
        /// </summary>
        private void PlaceObjectives()
        {
            Debug.Log("Objective Placement");
            float tempAngle = 0;
            float tempDistance;

            int tempX;
            int tempY;

            Vector2 angleRange = new(2 * Mathf.PI / (numberOfObjectives + 1), 2 * Mathf.PI / (numberOfObjectives - 1));
            Vector2 distanceRange = new(size / 4, size / 2.5f);

            GameObject objective;

            for (int i = 0; i < numberOfObjectives; i++)
            {
                tempAngle += Random.Range(angleRange.x, angleRange.y);
                tempDistance = Random.Range(distanceRange.x, distanceRange.y);
                tempX = (int)(tempDistance * Mathf.Cos(tempAngle)) + size / 2;
                tempY = (int)(tempDistance * Mathf.Sin(tempAngle)) + size / 2;

                objective = possibleObjectives[Random.Range(0, possibleObjectives.Length)].Pull(false);
                objective.transform.position = transform.position + new Vector3(tempX * width - ((size - 1) * width / 2), 0, tempY * width - ((size - 1) * width / 2));
                objective.SetActive(true);

                objectivePositions.Add(new(tempX, tempY));
            }
        }

        /// <summary>
        /// Chooses which generation method to use and uses it
        /// </summary>
        private void GenerateConditionalBoolArray()
        {
            Debug.Log("Bool map setup");
            //generate random bool map

            switch (generationMethod)
            {
                case GenerationMethod.Corridors:
                    GenerateBuildingConditionalArrayCorridors();
                    break;

                default:

                    GenerateBuildingConditionalArrayRandom();
                    break;
            }
        }

        /// <summary>
        /// generates the borders if there are any to generate
        /// should be called after the generation methods
        /// </summary>
        private void GenerateBorders()
        {
            buildingConditionalArrays[0, 0] = borderConditionalArray[0];
            buildingConditionalArrays[size - 1, 0] = borderConditionalArray[2];
            buildingConditionalArrays[0, size - 1] = borderConditionalArray[6];
            buildingConditionalArrays[size - 1, size - 1] = borderConditionalArray[8];

            for (int i = 1; i < size - 1; i++)
            {
                buildingConditionalArrays[i, 0] = borderConditionalArray[1];
                buildingConditionalArrays[0, i] = borderConditionalArray[3];
                buildingConditionalArrays[i, size - 1] = borderConditionalArray[7];
                buildingConditionalArrays[size - 1, i] = borderConditionalArray[5];
            }
        }

        /// <summary>
        /// places all the building
        /// </summary>
        private void PlaceBuildings()
        {
            Debug.Log("Building placement");
            //convert bool map to buildings using buildings collection

            for (int i = 0; i < size - 2; i++)
            {
                for (int j = 0; j < size - 2; j++)
                {
                    villageBuildingsCollection.Place(
                        ExtractArray(buildingConditionalArrays, i, j, 3), width, size, i, j);
                }
            }
        }

        //-----------------------------------------get/setters

        public VillageBuildingsCollection GetCollection()
        {
            return villageBuildingsCollection;
        }

        public void SetCollection(VillageBuildingsCollection newCollection)
        {
            villageBuildingsCollection = newCollection;
        }

        public void SetBuildingConditionalArrays(bool[,] newArray)
        {
            buildingConditionalArrays = newArray.Clone() as bool[,];
        }

        /// <summary>
        /// The mask tells which area should be empty and which shouldn't
        /// empty being filled with false
        /// This function only sets an area to true
        /// </summary>
        /// <param name="size">size of the upper gen, only used if the mask wasn't created</param>
        /// <param name="i">row of the mask</param>
        /// <param name="j">column of the mask</param>
        public void SetMask(int size, int i, int j)
        {
            if (mask == null) mask = new bool[size, size];
            mask[i, j] = true;
        }
    }
}