using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
    private readonly float TIME_BETWEEN_SPAWNS;

    [SerializeField] private GameObject zombiePrefab;

    private int _zombiesToSpawn;
    private ZombieTarget _zombieTargetOnSpawn;
    private bool _isSpawning;

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

        ZombieBehaviour zb = Instantiate(zombiePrefab
            , spawnPositionsOffset[_zombiesToSpawn % spawnPositionsOffset.Length] + transform.position
            , Quaternion.identity).GetComponent<ZombieBehaviour>();

        zb.gameObject.SetActive(true);
        zb.StartChase(_zombieTargetOnSpawn);
        //zb.ChooseRandomMesh();

        _zombiesToSpawn--;
        Invoke("SpawnZombie", TIME_BETWEEN_SPAWNS);
    }

    //-----------------------------------------------getters

    public bool CanSpawn() { return _canSpawn; }

    public bool IsSpawning() { return _zombiesToSpawn > 0; }
}
