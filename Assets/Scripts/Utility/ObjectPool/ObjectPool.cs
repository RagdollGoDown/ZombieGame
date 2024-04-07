using System.Collections;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Utility
{
    public class ObjectPool : MonoBehaviour
    {
        private static Dictionary<string, ObjectPool> availableObjectPools;

        [SerializeField] private List<GameObject> objects;

        [SerializeField] private int initialNumber = 5;
        private CancellationTokenSource readyInitialAsyncCancelTokenSource;
        private CancellationToken readyInitialAsyncCancelToken;
        [SerializeField] private List<GameObject> possibleObjects;

        public readonly UnityEvent<GameObject> onObjectAdded = new();


        //----------------------------------------pool function

        /// <summary>
        /// Gets the first disabled object in the pool
        /// It's often better to pull disabled and then enable it, 
        /// as some manipulations can't be done when the object is already enabled
        /// </summary>
        /// <param name="enabled">if we return the object enabled or disabled</param>
        /// <returns>the pulled object</returns>
        public GameObject Pull(bool enabled)
        {
            if (objects == null || objects.Count == 0 || objects[0] == null)
            {
                ReadyInitialObjects();
            }

            for (int i = 0; i < objects.Count; i++)
            {
                if (!objects[i].activeSelf)
                {
                    GameObject obj = objects[i];

                    obj.SetActive(enabled);
                    return obj;
                }
            }

            AddObjectToPool();
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

            objects = new();

            for (int i = 0; i < initialNumber; i++)
            {
                AddObjectToPool();
            }
        }

        /// <summary>
        /// empty then Instantiate all initial objects, but asynchronously
        /// </summary>
        public async void ReadyInitialObjectsAsync()
        {
            readyInitialAsyncCancelTokenSource = new();
            readyInitialAsyncCancelToken = readyInitialAsyncCancelTokenSource.Token;

            if (possibleObjects.Count == 0 || initialNumber == 0) { return; }

            EmptyObjects();

            objects = new();

            for (int i = 0; i < initialNumber && !readyInitialAsyncCancelToken.IsCancellationRequested; i++)
            {
                AddObjectToPool();

                await Task.Yield(); 
            }
        }

        private void AddObjectToPool()
        {
            GameObject added = Instantiate(possibleObjects[Random.Range(0, possibleObjects.Count)], transform);
            objects.Add(added);
            onObjectAdded.Invoke(added);
            added.SetActive(false);
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

        //-------------------------------unity events

        private void OnEnable()
        {
            availableObjectPools ??= new();

            availableObjectPools.Add(name, this);
        }

        private void OnDisable()
        {
            availableObjectPools.Remove(name);
        }

        private void OnDestroy()
        {
            readyInitialAsyncCancelTokenSource?.Cancel();
        }

        //-----------------------------getters and setters

        public static ObjectPool GetPool(string name)
        {
            availableObjectPools.TryGetValue(name, out ObjectPool pool);

            try
            {

                if (pool == null)
                {
                    pool = GameObject.Find(name).GetComponent<ObjectPool>();
                }
            }
            catch
            {
                Debug.Log("Pool " + name + " " + " is unfound");
            }

            return pool;
        }
    }
}
