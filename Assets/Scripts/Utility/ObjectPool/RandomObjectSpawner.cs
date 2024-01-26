using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utility
{
    public class RandomObjectSpawner : MonoBehaviour
    {
        [SerializeField] private string objectPoolName;

        private ObjectPool pool;

        [SerializeField] private bool spawnOnEnable;
        [SerializeField] private float probabilityOfEmpty = 0;

        [SerializeField] private Vector3 AreaCovered;

        public void SpawnObject()
        {
            if (probabilityOfEmpty != 0 && Random.Range(0f, 1f) < probabilityOfEmpty) return;

            if (pool == null)
            {
                pool = ObjectPool.GetPool(objectPoolName);
            }

            GameObject obj = pool.Pull(false);

            Vector3 offset = Random.Range(-AreaCovered.x, AreaCovered.x) * transform.right +
                Random.Range(-AreaCovered.y, AreaCovered.y) * transform.up +
                Random.Range(-AreaCovered.z, AreaCovered.z) * transform.forward;

            obj.transform.SetPositionAndRotation(transform.position + offset, transform.rotation);
            obj.SetActive(true);
        }

        // Update is called once per frame
        private void OnEnable()
        {
            if (spawnOnEnable)
            {
                SpawnObject();
            }
        }
    }
}