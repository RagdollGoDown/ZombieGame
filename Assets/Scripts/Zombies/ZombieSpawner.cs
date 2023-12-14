using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;

public class ZombieSpawner : MonoBehaviour
{
    [SerializeField] private float timeBetweenSpawns = 1;

    private int _zombiesToSpawn;
    private DamageableObject _zombieTargetOnSpawn;
    private bool _isSpawning;
    private ObjectPool zombiePool;

    //these positions are offsets from the gameobject's position
    [SerializeField] private Vector3[] spawnPositionsOffset;

    //----------------------------------unity events
    private void Awake()
    {
        if (spawnPositionsOffset.Length == 0)
        {
            spawnPositionsOffset = new Vector3[] { Vector3.zero };
        }
    }

    private void OnEnable()
    {
        ZombieSpawnerManager.AddSpawner(this);
    }

    private void OnDisable()
    {
        ZombieSpawnerManager.RemoveSpawner(this);
    }

    //-------------------------------------------spawning functions

    /// <summary>
    /// This adds zombie to be spawned. It will then spawn them with an interval of time between.
    /// </summary>
    /// <param name="amountOfZombies"> the number of total zombies to spawn</param>
    /// <param name="zombieTarget"> the target to be followed by the zombies</param>
    /// <param name="zombiePool"> the pool the zombies should be taken from</param>
    /// <returns></returns>
    public int AddZombiesToSpawn(int amountOfZombies, DamageableObject zombieTarget, ObjectPool zombiePool)
    {
        if (this.zombiePool != zombiePool) this.zombiePool = zombiePool;

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

    /// <summary>
    /// Whether it is currently spawning zombies or not.
    /// </summary>
    /// <returns>true if it is</returns>
    public bool IsSpawning() { return _zombiesToSpawn > 0; }
}
