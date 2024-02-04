using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using Utility;

public class ZombieSpawner : MonoBehaviour
{
    [SerializeField] private int timeBetweenSpawnsMilliSec = 1000;

    private int _zombiesToSpawn;
    private DamageableObject _zombieTargetOnSpawn;
    private ObjectPool zombiePool;

    private CancellationTokenSource spawnZombiesAsyncCancelTokenSource;
    private CancellationToken spawnZombiesAsyncCancelToken;

    //these positions are offsets from the gameobject's position
    [SerializeField] private Vector3[] spawnPositionsOffset;

    [SerializeField] private bool limitNumberOfSpawns = false; 
    [SerializeField] private int numberOfSpawns = 10; 
    [SerializeField] private bool limitZombiesPerSpawn = false; 
    [SerializeField] private int zombiesPerSpawnLimit = 10; 

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
        spawnZombiesAsyncCancelTokenSource?.Cancel();
        ZombieSpawnerManager.RemoveSpawner(this);
    }

    //-------------------------------------------spawning functions

    /// <summary>
    /// This adds zombie to be spawned. It will then spawn them with an interval of time between.
    /// </summary>
    /// <param name="amountOfZombies"> the number of total zombies to spawn</param>
    /// <param name="zombieTarget"> the target to be followed by the zombies</param>
    /// <param name="zombiePool"> the pool the zombies should be taken from</param>
    /// <param name="zombieReaper"> the reaper to inform upon the zombies death</param>
    /// <returns></returns>
    public int AddZombiesToSpawn(int amountOfZombies, DamageableObject zombieTarget, ObjectPool zombiePool, 
        Reaper zombieReaper = null)
    {
        spawnZombiesAsyncCancelTokenSource = new();
        spawnZombiesAsyncCancelToken = spawnZombiesAsyncCancelTokenSource.Token;

        if (this.zombiePool != zombiePool) this.zombiePool = zombiePool;

        if (limitNumberOfSpawns){
            numberOfSpawns--;
            if (numberOfSpawns < 0) return 0;
        }

        if (limitZombiesPerSpawn){
            amountOfZombies = Mathf.Min(amountOfZombies, zombiesPerSpawnLimit);
        }

        _zombiesToSpawn += amountOfZombies;
        _zombieTargetOnSpawn = zombieTarget;

        SpawnZombiesAsync(zombieReaper);

        return amountOfZombies;
    }

    private async void SpawnZombiesAsync(Reaper zombieReaper = null)
    {
        while (_zombiesToSpawn > 0  && !spawnZombiesAsyncCancelToken.IsCancellationRequested)
        {
            await Task.Delay(timeBetweenSpawnsMilliSec);
            SpawnZombie(zombieReaper);
        }
    }

    private void SpawnZombie(Reaper zombieReaper = null)
    {
        ZombieBehaviour zb = zombiePool.Pull(enabled:false).GetComponent<ZombieBehaviour>();

        zb.transform.position =
            spawnPositionsOffset[_zombiesToSpawn % spawnPositionsOffset.Length] + transform.position;
        zb.gameObject.SetActive(true);
        zb.StartChase(_zombieTargetOnSpawn);
        _zombiesToSpawn--;

        if (zombieReaper != null) ReapToZombieOnDeathEvent(zb, zombieReaper);
    }

    private void ReapToZombieOnDeathEvent(ZombieBehaviour zb, Reaper zombieReaper)
    {
        if (zombieReaper == null || zb == null) return;
        zb.OnDeath.RemoveListener(z => zombieReaper.Reap(z.GetLastDeadBodyPart()));
        zb.OnDeath.AddListener(z => zombieReaper.Reap(z.GetLastDeadBodyPart()));
    }
    //-----------------------------------------------get/setters

    /// <summary>
    /// Whether it is currently spawning zombies or not.
    /// </summary>
    /// <returns>true if it is</returns>
    public bool IsSpawning() { return _zombiesToSpawn > 0; }
}
