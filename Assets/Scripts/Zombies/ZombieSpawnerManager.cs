using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using Utility;
using Utility.Observable;

public class ZombieSpawnerManager : MonoBehaviour
{
    private static int ZOMBIESINWORLD;
    private static ZombieSpawnerManager CURRENT_SPAWNER;


    private int _maxZombiesInWorld;
    [SerializeField] private ObjectPool zombiePool;
    [SerializeField] private int baseMaxZombiesPerRound;
    [SerializeField] private int escalationPerRound;
    [SerializeField] private int timeBetweenRounds = 10;
    [SerializeField] private int timeBeforeFirstRound = 2;

    private DamageableObject target;

    public enum SpawnerState
    {
        MIDROUND,
        BREAK,
        UNKNOWN
    }
    private SpawnerState _spawnerState;

    private ZombieSpawner[] _spawners;
    private int _indexInSpawners;

    private int _currentRound;

    //-------------------------------events

    private void Awake()
    {
        if (timeBetweenRounds < 1) throw new System.ArgumentException("Time between rounds not bigger or equal to 1");
        if (timeBeforeFirstRound <= 0) throw new System.ArgumentException("Time before first round not strictly positiv");
        if (zombiePool == null) throw new System.NullReferenceException("No object pool to take zombies from");

        _maxZombiesInWorld = baseMaxZombiesPerRound;

        _spawners = GetComponentsInChildren<ZombieSpawner>();

        foreach (var spawner in _spawners)
        {
            spawner.SetZombiePool(zombiePool);
        }

        _currentRound = 1;

        CURRENT_SPAWNER = this;
    }

    private void OnDisable()
    {
        if (CURRENT_SPAWNER == this) CURRENT_SPAWNER = null;
        ZOMBIESINWORLD = 0;
    }

    private void EnterBreakTime()
    {
        Debug.Log("Break");
        _spawnerState = SpawnerState.BREAK;

        Invoke("EnterRound", timeBetweenRounds);
    }

    private void EnterRound()
    {
        Debug.Log("Round");
        _currentRound++;
        _spawnerState = SpawnerState.MIDROUND;

        SpawnZombies();
    }

    public void BeginToSpawn()
    {
        if (CURRENT_SPAWNER != this) 
        {
            Debug.Log("There are two zombie spawners");
        }
        else if (_spawners.Length > 0 && CanSpawn())
        {
            Debug.Log("Started Spawning");
            Invoke(nameof(EnterRound), timeBeforeFirstRound);
        }
        else
        {
            Debug.Log("No spawners created or no active spawners");
        }
    }

    private void SpawnZombies()
    {
        if (!CanSpawn()) 
        {
            Debug.Log("No active Spawners");
            return;
        }

        //Debug.Log(ZOMBIESINWORLD + " zombiesInworld " + _maxZombiesInWorld + " max zombies");
        int zombiesToSpawn = _maxZombiesInWorld - ZOMBIESINWORLD;

        _maxZombiesInWorld += escalationPerRound;

        if (zombiesToSpawn < 1 && target != null) { return; }

        while (zombiesToSpawn > 0)
        {
            int a = _spawners[_indexInSpawners].
                AddZombiesToSpawn(Random.Range(1, zombiesToSpawn + 1), target);

            zombiesToSpawn -= a;

            _indexInSpawners = (_indexInSpawners+1) % _spawners.Length;
        }

        EnterBreakTime();
    }

    private bool CanSpawn()
    {
        bool hasActiveSpawner = false;

        foreach (ZombieSpawner zs in _spawners)
        {
            if (zs.CanSpawn()) hasActiveSpawner = true;
            break;
        }
        return hasActiveSpawner;
    }

    private bool IsSpawning()
    {
        bool b = false;

        foreach(ZombieSpawner zs in _spawners)
        {
            b = zs.IsSpawning() || b;
        }

        return b;
    }

    //---------------------------------setters

    //the zombie spawners add the zombies to the count but the zombies remove themselves from it
    public static void AddZombie(int amount)
    {
        ZOMBIESINWORLD += amount;
    }
    public static void RemoveZombie()
    {
        if (ZOMBIESINWORLD > 0) ZOMBIESINWORLD--;

        /*if (ZOMBIESINWORLD == 0 && 
            CURRENT_SPAWNER && !CURRENT_SPAWNER.IsSpawning()) 
            CURRENT_SPAWNER.EnterBreakTime();*/
    }

    //-------------------------------getters

    public static SpawnerState GetCurrentSpawnerState() {
        if (CURRENT_SPAWNER) return CURRENT_SPAWNER._spawnerState;
        else return SpawnerState.UNKNOWN;
    }

    public void SetTarget(DamageableObject target)
    {
        this.target = target;
    }
}
