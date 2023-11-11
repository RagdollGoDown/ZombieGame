using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;
using System.Linq;

namespace MapGeneration.VillageGeneration
{
    public class VillageBuildingsCollection : MonoBehaviour
    {
        [SerializeField] private List<VillageBuilding> buildings;
        [SerializeField] private float buildingWidth;

        public GameObject Get(bool[,] conditionArray)
        {
            if (buildings == null) buildings = new();

            if (conditionArray == null) throw new System.ArgumentException("null condition array given");

            foreach (VillageBuilding vb in buildings)
            {
                bool[,] vbConditionArray = vb.GetConditionArray();
                bool eq = 
                    vbConditionArray.Rank == conditionArray.Rank &&
                Enumerable.Range(0, vbConditionArray.Rank)
                .All(dimension => vbConditionArray.GetLength(dimension) == conditionArray.GetLength(dimension)) &&
                vbConditionArray.Cast<bool>().SequenceEqual(conditionArray.Cast<bool>());

                if (eq)
                {
                    return vb.possibleObjects.Pull(true);
                }
            }

            return null;
        }

        //the width is equal to the height (they are on squares)
        public float GetWidth()
        {
            return buildingWidth;
        }

        public List<VillageBuilding> GetBuildings()
        {
            if (buildings == null) { buildings = new(); }

            return buildings;
        }

        public void ClearBuildingsOnMap()
        {
            foreach(VillageBuilding vb in buildings)
            {
                vb.possibleObjects.ReadyInitialObjects();
            }
        }

        public void ClearBuildingsList()
        {
            buildings = new();
        }
    }

    public class VillageBuilding
    {
        public string name = "Building Name";

        public bool b02;
        public bool b00;
        public bool b01;
        public bool b10;
        public bool b11 = true;
        public bool b12;
        public bool b20;
        public bool b21;
        public bool b22;

        public ObjectPool possibleObjects;

        public VillageBuilding() { }

        public VillageBuilding(VillageBuilding vb) 
        {
            b00 = vb.b00;
            b01 = vb.b01;
            b02 = vb.b02;
            b10 = vb.b10;
            b11 = vb.b11;
            b12 = vb.b12;
            b20 = vb.b20;
            b21 = vb.b21;
            b22 = vb.b22;

            name = vb.name;

            possibleObjects = vb.possibleObjects;
        }

        public bool[,] GetConditionArray()
        {
            bool[,] key = new bool[3, 3];
            key[0, 0] = b00;
            key[0, 1] = b01;
            key[0, 2] = b02;
            key[1, 0] = b10;
            key[1, 1] = b11;
            key[1, 2] = b12;
            key[2, 0] = b20;
            key[2, 1] = b21;
            key[2, 2] = b22;

            return key;
        }

        public ObjectPool GetObjectPool()
        {
            return possibleObjects;
        } 
    }
}