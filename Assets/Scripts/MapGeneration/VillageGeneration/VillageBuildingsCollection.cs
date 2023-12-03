using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;
using System.Linq;

namespace MapGeneration.VillageGeneration
{
    [System.Serializable]
    public class VillageBuildingsCollection : MonoBehaviour
    {
        [SerializeReference] private List<VillageBuilding> buildings;
        [SerializeField] private float buildingWidth;

        private bool[][,] tempArrayAndAllRotations = new bool[4][,];

        public void Place(bool[,] conditionArray, float width, int size, int posX, int posY)
        {
            if (buildings == null) buildings = new();
            tempArrayAndAllRotations[0] = conditionArray ?? throw new System.ArgumentException("null condition array given");
            tempArrayAndAllRotations[1] = Rotate(tempArrayAndAllRotations[0]);
            tempArrayAndAllRotations[2] = Rotate(tempArrayAndAllRotations[1]);
            tempArrayAndAllRotations[3] = Rotate(tempArrayAndAllRotations[2]);

            foreach (VillageBuilding vb in buildings)
            {
                for (int i = 0; i < tempArrayAndAllRotations.Length; i++)
                {
                    if (vb.Satisfies(tempArrayAndAllRotations[i]))
                    {
                        vb.Place(width, size, posX, posY, i * 90);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Will rotate the conditional array in clock wise direction
        /// </summary>
        private bool[,] Rotate(bool[,] array)
        {
            bool[,] newArray = array.Clone() as bool[,];

            for (int i = 0; i <= array.Rank; i++)
            {
                for (int j = 0; j <= array.Rank; j++)
                {
                    newArray[i, j] = array[j, array.Rank - i];
                }
            }

            return newArray;
        }

        //the width is equal to the height (they are on squares)
        public float GetWidth()
        {
            return buildingWidth;
        }

        public List<VillageBuilding> GetBuildings()
        {
            if (buildings == null) {
                buildings = new(); }

            return buildings;
        }

        public void ReadyCollection(int size)
        {
            foreach (VillageBuilding vb in buildings)
            {
                vb.Ready(size);
            }
        }

        public void FinishCollection(float width)
        {
            foreach (VillageBuilding vb in buildings)
            {
                vb.Finish(width);
            }
        }
    }


    [System.Serializable]
    public enum HandyBoolValues
    {
        False,
        True,
        Either
    }

    [System.Serializable]
    public class HandyBool
    {
        public HandyBoolValues value;

        public HandyBool(){}

        public HandyBool(HandyBoolValues value)
        {
            this.value = value;
        }


        public bool Compare(HandyBool other)
        {
            return (other != null) && (value == HandyBoolValues.Either || other.value == HandyBoolValues.Either || other.value == value);
        }

        public bool Compare(bool other)
        {
            //Debug.Log(other + " compared to " + value);
            //Debug.Log(value == HandyBoolValues.Either || (other && value == HandyBoolValues.True) || (!other && value == HandyBoolValues.False));
            return value == HandyBoolValues.Either || (other && value == HandyBoolValues.True) || (!other && value == HandyBoolValues.False);
        }
    }
}