using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utility
{
    public class ObjectPool : MonoBehaviour
    {
        [SerializeField] private List<GameObject> objects;

        [SerializeField] private int initialNumber = 5;
        [SerializeField] private List<GameObject> possibleObjects;


        //----------------------------------------pool function

        /// <summary>
        /// Returns the first disabled object in the pool
        /// </summary>
        /// <returns>the object but enabled</returns>
        public GameObject Pull()
        {
            for (int i = 0; i < objects.Count; i++)
            {
                if (!objects[i].activeSelf)
                {
                    GameObject obj = objects[i];
                    objects.Remove(obj);
                    objects.Insert(objects.Count - 1, obj);

                    obj.SetActive(true);
                    return obj;
                }
            }

            int possibleIndex = possibleObjects.Count > 1 ? Random.Range(1, possibleObjects.Count - 1) : 0;

            objects.Add(Instantiate(possibleObjects[possibleIndex], transform));

            return objects[objects.Count - 1];
        }

        //----------------------------------------editor methods

        /// <summary>
        /// SHOULD ONLY BE USED IN EDITOR CODE
        /// Put all initial objects
        /// </summary>
        public void ReadyInitialObjects()
        {
            if (possibleObjects.Count == 0 || initialNumber == 0) {return;}

            EmptyObjects();

            int possibleStep = possibleObjects.Count > 1 ? Random.Range(1, possibleObjects.Count - 1) : 0;
            int possibleIndex = 0;

            objects = new();

            for (int i = 0; i < initialNumber; i++)
            {
                objects.Add(Instantiate(possibleObjects[possibleIndex], transform));
                objects[objects.Count-1].gameObject.SetActive(false);
                possibleIndex = (possibleIndex + possibleStep) % possibleObjects.Count;
            }
        }

        /// <summary>
        /// SHOULD ONLY BE USED IN EDITOR CODE
        /// Empty all objects from the pool
        /// </summary>
        public void EmptyObjects()
        {
            List<GameObject> children = new();

            foreach (Transform child in transform)
            {
                children.Add(child.gameObject);
            }

            GameObject current;
            int count = children.Count;

            for (int i = 0; i < count; i++)
            {
                current = children[0];
                children.RemoveAt(0);
                DestroyImmediate(current);
            }
        }
    }
}
