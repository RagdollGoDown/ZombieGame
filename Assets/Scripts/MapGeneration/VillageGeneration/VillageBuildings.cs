using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Utility;

namespace MapGeneration.VillageGeneration
{
    [System.Serializable]
    public class VillageBuilding
    {
        private static readonly int DEFAULT_COND_SIZE = 3;

        public string name = "Building Name";

        //done in 1D as unity does not support serialization of multivariate things
        public HandyBool[] conditionalArray;

        public ObjectPool possibleObjects;

        private GameObject tempGameObject;

        public VillageBuilding()
        {
            conditionalArray = new HandyBool[DEFAULT_COND_SIZE * DEFAULT_COND_SIZE];

            for (int i = 0; i <= conditionalArray.Rank; i++)
            {
                for (int j = 0; j <= conditionalArray.Rank; j++)
                {
                    conditionalArray[i * DEFAULT_COND_SIZE + j] = new HandyBool();
                }
            }
        }

        public VillageBuilding(VillageBuilding vb)
        {
            conditionalArray = vb.conditionalArray.Select(a => new HandyBool(a.value)).ToArray();

            possibleObjects = vb.possibleObjects;

            name = vb.name;
        }

        public bool Satisfies(bool[,] other)
        {
            if (other.Rank + 1 != DEFAULT_COND_SIZE || other.Length != conditionalArray.Length) return false;

            for (int i = 0; i <= other.Rank; i++)
            {
                for (int j = 0; j <= other.Rank; j++)
                {
                    if (!conditionalArray[i * DEFAULT_COND_SIZE + j].Compare(other[i, j])) return false;
                }
            }

            return true;
        }

        public void Rotate()
        {
            HandyBool[] old = conditionalArray.Clone() as HandyBool[];

            for (int i = 0; i < DEFAULT_COND_SIZE; i++)
            {
                for (int j = 0; j < DEFAULT_COND_SIZE; j++)
                {
                    conditionalArray[i * DEFAULT_COND_SIZE + j] = old[j * DEFAULT_COND_SIZE + DEFAULT_COND_SIZE - 1 - i];
                }
            }
        }
    }
}
