using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;

public class ZombieSpawner : MonoBehaviour
{
    [SerializeField] private float timeBetweenSpawns = 1;

    [SerializeField] private GameObject zombiePrefab;

    private int _zombiesToSpawn;
    private DamageableObject _zombieTargetOnSpawn;
    private bool _isSpawning;

    private ObjectPool zombiePool;

    private bool _canSpawn;

    //these positions are offsets from the gameobject's position
    [SerializeField] private Vector3[] spawnPositionsOffset;

    private void Awake()
    {
        _canSpawn = !(zombiePrefab == null);

        if (_canSpawn)
        {
            zombiePrefab.SetActive(false);
        }

        if (spawnPositionsOffset.Length == 0)
        {
            spawnPositionsOffset = new Vector3[] { Vector3.zero };
        }
    }

    public int AddZombiesToSpawn(int amountOfZombies, DamageableObject zombieTarget)
    {
        if (zombiePool == null) throw new System.NullReferenceException("No object pool to take zombies from");

        if (!_canSpawn) { return 0; }

        _zombiesToSpawn += amountOfZombies;
        _zombieTargetOnSpawn = zombieTarget;

        ZombieSpawnerManager.AddZombie(amountOfZombies);

        if (!_isSpawning && _zombiesToSpawn > 0)
        {
            SpawnZombie();
        }

        return amountOfZombies;
    }

    private void SpawnZombie()
    {
        _isSpawning = true;

        if (_zombiesToSpawn <= 0)
        {
            _isSpawning = false;
            return;
        }

        ZombieBehaviour zb = zombiePool.Pull(enabled:false).GetComponent<ZombieBehaviour>();
        zb.transform.position =
            spawnPositionsOffset[_zombiesToSpawn % spawnPositionsOffset.Length] + transform.position;
        zb.gameObject.SetActive(true);
        zb.StartChase(_zombieTargetOnSpawn);
        _zombiesToSpawn--;
        Invoke(nameof(SpawnZombie), timeBetweenSpawns);
    }

    //-----------------------------------------------get/setters

    public void SetZombiePool(ObjectPool zombiePool)
    {
        this.zombiePool = zombiePool;
    }

    public bool CanSpawn() { return _canSpawn; }

    public bool IsSpawning() { return _zombiesToSpawn > 0; }
}
