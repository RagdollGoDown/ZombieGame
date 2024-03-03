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

        private bool[,] buildingConditionalArrays;

        [SerializeField] private VillageBuildingsCollection villageBuildingsCollection;

        public bool[] borderConditionalArray;

        //This sais where the buildings can be placed
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
        [SerializeField] private bool onlyCorridorsNoOpenSpaces = false;

        [SerializeField] private GenerationMethod generationMethod;

        /// <summary>
        /// Generates the village first by making a 2 dimensional bool array 
        /// then using the village collection places the buildings
        /// When this function is used with width != 0 this means that is is being used as a building
        /// meaning it's og size will overiden to make it fit in the bigger generator and the mask will make it
        /// go only in the places given by the bigger generator
        /// </summary>
        /// <param name="upperWidth"> the width of the upper generator</param>
        public void Generate(List<GameObject> necessaryBuildingsToPlace = null)
        {
            
            if (villageBuildingsCollection == null) throw new System.Exception("Need to have a village collection component");
            if (size < 3) throw new System.ArgumentException("Size must be bigger than 2");
            if (density < 0 || density > 1) throw new System.ArgumentException("Density must be between 0 and 1");

            buildingConditionalArrays = new bool[size+2, size+2];
            villageBuildingsCollection.ReadyCollection(size);
            width = villageBuildingsCollection.GetWidth();

            List<Vector3Int> necessaryBuildingPositions = PlaceNecessaryBuildings(necessaryBuildingsToPlace);

            GenerateConditionalBoolArray(necessaryBuildingPositions);

            GenerateBorders();

            PlaceBuildings();

            villageBuildingsCollection.FinishCollection(buildingConditionalArrays,size);
        }

        public void GenerateAsSubGenerator(bool[,] upperConditionalBuildingArray,float upperWidth,int upperSize)
        {
            width = villageBuildingsCollection.GetWidth();

            if (upperWidth % width != 0) throw new System.Exception("Sub generator with width not divideable by upper generator width given");
            size = (int)(upperWidth / width) * upperSize;

            buildingConditionalArrays = new bool[size+2, size+2];
            villageBuildingsCollection.ReadyCollection(size);

            ExpandUpperConditionalBool(upperWidth, upperConditionalBuildingArray);

            PlaceBuildingsWithMask((int)(upperWidth / width));

            villageBuildingsCollection.FinishCollection(buildingConditionalArrays,size);
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

        /// <summary>
        /// This will expand the conditional bool array of the upper generator
        /// so that we can fit the buildings of the sub generator
        /// </summary>
        /// <param name="upperWidth">the width of the upper generator</param>
        private void ExpandUpperConditionalBool(float upperWidth, bool[,] upperConditionalBoolArray){
            
            int divide = (int)(upperWidth / width);

            for (int i = 0; i < upperConditionalBoolArray.GetLength(0)-2; i++)
            {
                for (int j = 0; j < upperConditionalBoolArray.GetLength(1)-2; j++)
                {
                    FillExtract(buildingConditionalArrays, i * divide, j * divide, divide, upperConditionalBoolArray[i, j]);
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
                        origin = new Vector3(i * width - ((size-1) * width / 2), 0, j * width - ((size-1) * width / 2));
                        Gizmos.color = color;
                        Gizmos.DrawLine(origin, origin + Vector3.up * 5);

                        if (mask != null && mask[i/2, j/2])
                        {
                            Gizmos.color = Color.red;
                            Gizmos.DrawWireCube(origin, new Vector3(5, 0, 5));
                        }
                    }
                }
            }
        }

        //-------------------------------------------------------generating methods

        private void GenerateBuildingConditionalArrayRandom(List<Vector3Int> necessaryBuildingPositions = null)
        {
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    buildingConditionalArrays[i, j] = Random.Range(0,1.0f) <= density;
                }
            }

            necessaryBuildingPositions?.ForEach(position =>
            {
                FillExtract(buildingConditionalArrays,
                    position.x - 1, position.y - 1, 3, false);
            });
        }

        private void GenerateBuildingConditionalArrayCorridors(List<Vector3Int> necessaryBuildingPositions = null)
        {
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    buildingConditionalArrays[i, j] = true;
                }
            }

            float rand;

            List<Vector3Int> deadEnds = new();


            void continueCorridor(int x, int y, int fromx, int fromy){

                // if it's on the border or it already has a corridor then pass
                if (x <= 1 || y <= 1 || x >= size - 2 || y >= size - 2 || !buildingConditionalArrays[x, y])
                {
                    return;
                }

                if (x <= 2 || y <= 2 || x >= size - 3 || y >= size - 3)
                {
                    continueCorridor(x + y - fromy, y + x - fromx, x, y);
                }

                buildingConditionalArrays[x, y] = false;

                rand = Random.Range(0,1.0f);

                bool forward = Random.Range(0, 1.0f) <= forwardProbability;
                bool right = Random.Range(0, 1.0f) <= turnProbability;
                bool left = Random.Range(0, 1.0f) <= turnProbability;

                    
                //go forward
                if (forward && 
                    (!onlyCorridorsNoOpenSpaces || 
                    (buildingConditionalArrays[x + (y - fromy), y + (x - fromx)] && buildingConditionalArrays[x - (y - fromy), y - (x - fromx)])))
                {
                    continueCorridor(2 * x - fromx, 2 * y - fromy, x, y);
                }

                //turn right
                if (right&& 
                    (!onlyCorridorsNoOpenSpaces ||
                    buildingConditionalArrays[x + (y - fromy) + (x - fromx), y + (y - fromy) - (x - fromx)]))
                {
                    continueCorridor(x + y - fromy, y + x - fromx, x, y);
                }

                //turn left
                if (left&& 
                    (!onlyCorridorsNoOpenSpaces ||
                    buildingConditionalArrays[x - (y - fromy) + (x - fromx), y + (y - fromy) + (x - fromx)]))
                {
                    continueCorridor(x - y + fromy, y - x + fromx, x, y);
                }
            }

            int halfSize = size / 2;

            //this is meant to ensure that the necessary buildings are linked to the rest of the corridors
            necessaryBuildingPositions?.ForEach(position =>
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
                    position.x - 1, position.y - 1, 3, false);
            });


            buildingConditionalArrays[halfSize, halfSize] = false;

            continueCorridor(halfSize, halfSize - 1, halfSize, halfSize);
            continueCorridor(halfSize + 1, halfSize, halfSize, halfSize);
            continueCorridor(halfSize, halfSize + 1, halfSize, halfSize);
            continueCorridor(halfSize - 1, halfSize, halfSize, halfSize);
        }

        //-------------------------------------------generation steps

        /// <summary>
        /// Places the buildings that are necessarily on the map
        /// Does not need the conditional bool array
        /// </summary>
        /// <returns>The positions of the placed objects in the grid</returns>
        private List<Vector3Int> PlaceNecessaryBuildings(List<GameObject> necessaryBuildingsToPlace)
        {
            if (necessaryBuildingsToPlace == null) return null;

            float tempAngle = 0;
            float tempDistance;
            List<Vector3Int> tempList = new();

            int tempX;
            int tempY;

            Vector2 angleRange = new(2 * Mathf.PI / (necessaryBuildingsToPlace.Count + 1), 2 * Mathf.PI / (necessaryBuildingsToPlace.Count - 1));
            Vector2 distanceRange = new(size / 4, size / 2.5f);

            for (int i = 0; i < necessaryBuildingsToPlace.Count; i++)
            {
                GameObject nbp = necessaryBuildingsToPlace[i];

                tempAngle += Random.Range(angleRange.x, angleRange.y);
                tempDistance = Random.Range(distanceRange.x, distanceRange.y);
                tempX = (int)(tempDistance * Mathf.Cos(tempAngle)) + size / 2;
                if (tempX >= size - 3) tempX -= 1;
                if (tempX <= 2) tempX += 1;

                tempY = (int)(tempDistance * Mathf.Sin(tempAngle)) + size / 2;
                if (tempY >= size - 3) tempY -= 1;
                if (tempY <= 2) tempY += 1;

                nbp.transform.position = transform.position + new Vector3(tempX * width - ((size - 1) * width / 2), 0, tempY * width - ((size - 1) * width / 2));
                nbp.SetActive(true);

                tempList.Add(new(tempX, tempY));
            }


            return tempList;
        }

        /// <summary>
        /// Chooses which generation method to use and uses it
        /// </summary>
        private void GenerateConditionalBoolArray(List<Vector3Int> necessaryBuildingPositions = null)
        {
            switch (generationMethod)
            {
                case GenerationMethod.Corridors:
                    GenerateBuildingConditionalArrayCorridors(necessaryBuildingPositions);
                    break;

                default:
                    GenerateBuildingConditionalArrayRandom(necessaryBuildingPositions);
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

        /// <summary>
        /// places all the building
        /// </summary>
        private void PlaceBuildingsWithMask(int divide)
        {

            //convert bool map to buildings using buildings collection
            if (mask == null) throw new System.Exception("Mask not created");
            for (int i = 0; i < size - 2; i++)
            {
                for (int j = 0; j < size - 2; j++)
                {
                    if (mask[(i+1)/divide,(j+1)/divide])
                    {
                        villageBuildingsCollection.Place(
                            ExtractArray(buildingConditionalArrays, i, j, 3), width, size, i, j);   
                    }
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

        public bool[,] GetBuildingConditionalArrays(){
            return buildingConditionalArrays;
        }

        public void ResetMask(int size){
            mask = new bool[size, size];
        }

        /// <summary>
        /// The mask tells which area should be empty and which shouldn't
        /// empty being filled with false
        /// This function only sets an area to true
        /// </summary>
        /// <param name="size">size of the upper gen, only used if the mask wasn't created</param>
        /// <param name="i">row of the mask</param>
        /// <param name="j">column of the mask</param>
        public void SetMask(int i, int j)
        {
            mask[i, j] = true;
        }
    }
}