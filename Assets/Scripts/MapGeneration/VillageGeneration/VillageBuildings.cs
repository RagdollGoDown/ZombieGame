using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Utility;
using System.Threading.Tasks;

namespace MapGeneration.VillageGeneration
{
    [System.Serializable]
    public class VillageBuilding
    {
        private static readonly int DEFAULT_COND_SIZE = 3;

        public string name = "Building Name";

        //done in 1D as unity does not support serialization of multivariate things
        public HandyBool[] conditionalArray;

        public VillageBuilding()
        {
            conditionalArray = new HandyBool[DEFAULT_COND_SIZE * DEFAULT_COND_SIZE];

            for (int i = 0; i < DEFAULT_COND_SIZE; i++)
            {
                for (int j = 0; j < DEFAULT_COND_SIZE; j++)
                {
                    conditionalArray[i * DEFAULT_COND_SIZE + j] = new HandyBool();
                }
            }
        }

        public VillageBuilding(VillageBuilding vb)
        {
            conditionalArray = vb.conditionalArray.Select(a => new HandyBool(a.value)).ToArray();

            name = vb.name;
        }

        public virtual void Finish(float width,int size, bool[,] conditionalBuildingArray) { }

        public virtual void Place(float width, int size, int posX, int posY, float rotation) { }
        
        public virtual void Ready(int size) { }

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

    [System.Serializable]
    public class ObjectPoolBuilding : VillageBuilding
    {
        public ObjectPool possibleObjects;
        // TODO benchmark if temps are worth it or not
        private GameObject tempGameObject;

        public ObjectPoolBuilding() : base() { }

        public ObjectPoolBuilding(ObjectPoolBuilding opb) : base(opb)
        {
            possibleObjects = opb.possibleObjects;
        }

        public override void Finish(float width, int size, bool[,] conditionalBuildingArray) { }

        public override void Place(float width, int size, int posX, int posY, float rotation)
        {
            if (possibleObjects == null) { return; }

            tempGameObject = possibleObjects.Pull(false);

            tempGameObject.transform.position = new Vector3((posX + 1) * width - ((size - 1) * width / 2), 0, (posY + 1) * width - ((size - 1) * width / 2));
            tempGameObject.transform.Rotate(Vector3.up, rotation);
            tempGameObject.SetActive(true);

            if (tempGameObject.TryGetComponent(out PlacableBuilding placable))
            {
                placable.onPlaced.Invoke();
            }
        }

        public override void Ready(int size)
        {
            possibleObjects.ReadyInitialObjects(size/5);
        }
    }

    [System.Serializable]
    public class GeneratorProxyBuilding : VillageBuilding
    {
        public VillageGenerator generator;
        
        public GeneratorProxyBuilding() : base() { }

        public GeneratorProxyBuilding(GeneratorProxyBuilding gpb) : base(gpb)
        {
            generator = gpb.generator;
        }

        public override void Finish(float width,int size, bool[,] conditionalBuildingArray)
        {
            generator.GenerateAsSubGenerator(conditionalBuildingArray,width, size);
        }

        public override void Place(float width, int size, int posX, int posY, float rotation)
        {
            generator.SetMask(posX + 1, posY + 1);
        }

        public override void Ready(int size) 
        {
            generator.ResetMask(size);
            generator.GetCollection().ReadyCollection(size);
        }
    }
}
