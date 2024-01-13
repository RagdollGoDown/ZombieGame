using System.Collections;
using System.Threading.Tasks;
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

            int possibleIndex = Random.Range(0, possibleObjects.Count);

            objects.Add(Instantiate(possibleObjects[possibleIndex], transform));

            if (enabled) objects[^1].SetActive(true);
            return objects[^1];
        }

        //----------------------------------------editor methods

        /// <summary>
        /// empty then Instantiate all initial objects
        /// </summary>
        public void ReadyInitialObjects()
        {
            if (possibleObjects.Count == 0 || initialNumber == 0) {return;}

            EmptyObjects();

            int possibleIndex;

            objects = new();

            for (int i = 0; i < initialNumber; i++)
            {
                possibleIndex = Random.Range(0,possibleObjects.Count);
                objects.Add(Instantiate(possibleObjects[possibleIndex], transform));
                objects[^1].SetActive(false);
            }
        }

        /// <summary>
        /// empty then Instantiate all initial objects, but asynchronously
        /// </summary>
        public async void ReadyInitialObjectsAsync()
        {
            if (possibleObjects.Count == 0 || initialNumber == 0) { return; }

            EmptyObjects();

            int possibleIndex;

            objects = new();

            for (int i = 0; i < initialNumber; i++)
            {
                possibleIndex = Random.Range(0, possibleObjects.Count);
                objects.Add(Instantiate(possibleObjects[possibleIndex], transform));
                objects[^1].SetActive(false);

                await Task.Yield(); 
            }
        }

        /// <summary>
        /// empty then Instantiate all initial objects
        /// </summary>
        /// <param name="numberOfObjects">the number of objects to ready</param>
        public void ReadyInitialObjects(int numberOfObjects)
        {
            initialNumber = numberOfObjects;

            ReadyInitialObjects();
        }

        /// <summary>
        /// empty then Instantiate all initial objects, but asynchronously
        /// </summary>
        /// <param name="numberOfObjects">the number of objects to ready</param>
        public void ReadyInitialObjectsAsync(int numberOfObjects)
        {
            initialNumber = numberOfObjects;

            ReadyInitialObjectsAsync();
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
