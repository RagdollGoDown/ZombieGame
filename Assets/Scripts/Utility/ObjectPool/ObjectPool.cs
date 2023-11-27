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
        /// Gets the first disabled object in the pool
        /// </summary>
        /// <param name="enabled">if we return the object enabled or disabled</param>
        /// <returns>the pulled object</returns>
        public GameObject Pull(bool enabled)
        {
            for (int i = 0; i < objects.Count; i++)
            {
                if (!objects[i].activeSelf)
                {
                    GameObject obj = objects[i];
                    objects.Remove(obj);
                    objects.Insert(objects.Count - 1, obj);

                    if (enabled) obj.SetActive(true);
                    return obj;
                }
            }

            int possibleIndex = possibleObjects.Count > 1 ? Random.Range(1, possibleObjects.Count - 1) : 0;

            objects.Add(Instantiate(possibleObjects[possibleIndex], transform));

            if (enabled) objects[objects.Count - 1].SetActive(true);
            return objects[objects.Count - 1];
        }

        //----------------------------------------editor methods

        /// <summary>
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
        /// Put all initial objects
        /// </summary>
        /// <param name="numberOfObjects">the number of objects to ready</param>
        public void ReadyInitialObjects(int numberOfObjects)
        {
            initialNumber = numberOfObjects;

            ReadyInitialObjects();
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
