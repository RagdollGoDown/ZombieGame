using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
<<<<<<< Updated upstream
    private readonly float TIME_BETWEEN_SPAWNS;

=======
    [SerializeField] private float timeBetweenSpawns = 1;
>>>>>>> Stashed changes
    [SerializeField] private GameObject zombiePrefab;

    private int _zombiesToSpawn;
    private ZombieTarget _zombieTargetOnSpawn;
    private bool _isSpawning;

    private ObjectPool zombiePool;

    private bool _canSpawn;

    private int _maxZombiesToSpawn;

    //these positions are offsets from the gameobject's position
    [SerializeField] private Vector3[] spawnPositionsOffset;

    private void Awake()
    {
        _canSpawn = !(zombiePrefab == null);

        if (_canSpawn) {
            zombiePrefab.SetActive(false); }

        if (spawnPositionsOffset.Length == 0)
        {
            spawnPositionsOffset = new Vector3[]{Vector3.zero};
        }

        _maxZombiesToSpawn = spawnPositionsOffset.Length;
    }

    public int AddZombiesToSpawn(int amountOfZombies,ZombieTarget zombieTarget)
    {
        if (zombiePool == null) throw new System.NullReferenceException("No object pool to take zombies from");

        if (!_canSpawn) {return 0;}

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

        ZombieBehaviour zb = zombiePool.Pull().GetComponent<ZombieBehaviour>();
        zb.transform.position =
            //spawnPositionsOffset[_zombiesToSpawn % spawnPositionsOffset.Length] + transform.position;
            Vector3.zero;

        Debug.Log(zb.transform.position);
        zb.StartChase(_zombieTargetOnSpawn);
        _zombiesToSpawn--;
        Invoke("SpawnZombie", TIME_BETWEEN_SPAWNS);
    }

    //-----------------------------------------------get/setters

    public void SetZombiePool(ObjectPool zombiePool)
    {
        this.zombiePool = zombiePool;
    }

    public bool CanSpawn() { return _canSpawn; }

    public bool IsSpawning() { return _zombiesToSpawn > 0; }
}
