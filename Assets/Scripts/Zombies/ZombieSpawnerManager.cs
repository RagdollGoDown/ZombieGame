using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieSpawnerManager : MonoBehaviour
{
    private static ZombieTarget PLAYER_TARGET;
    private static int ZOMBIESINWORLD;

    private int _maxZombiesInWorld;
    [SerializeField] private int baseMaxZombiesPerRound;
    [SerializeField] private int escalationPerRound;
    [SerializeField] private int timeBetweenRounds = 10;

    private enum SpawnerState
    {
        SPAWNING,
        MIDROUND,
        BREAK
    }
    private SpawnerState _spawnerState;

    private ZombieSpawner[] _spawners;
    [SerializeField]private ZombieSpawner spawner;
    private int _indexInSpawners;


    //-------------------------------events

    private void Awake()
    {
        _maxZombiesInWorld = baseMaxZombiesPerRound;

        if (timeBetweenRounds < 1) { timeBetweenRounds = 1; }

        _spawners = GetComponentsInChildren<ZombieSpawner>();
    }

    private void OnDisable()
    {
        ZOMBIESINWORLD = 0;
    }

    private void EnterBreakTime()
    {
        Invoke("EnterRound", timeBetweenRounds);
    }

    private void EnterRound()
    {
        _spawnerState = SpawnerState.SPAWNING;

        BeginToSpawn();
    }

    public void BeginToSpawn()
    {
        if (_spawners.Length > 0 && CanSpawn())
        {
            InvokeRepeating("SpawnZombies", timeBetweenRounds, timeBetweenRounds);
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

        if (zombiesToSpawn < 1) { return; }

        while (zombiesToSpawn > 0)
        {
            int a = _spawners[_indexInSpawners].AddZombiesToSpawn(Random.Range(1, zombiesToSpawn + 1), PLAYER_TARGET);
            zombiesToSpawn -= a;

            _indexInSpawners = (_indexInSpawners+1) % _spawners.Length;
        }
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

    //---------------------------------setters

    //the zombie spawners add the zombies to the count but the zombies remove themselves from it
    public static void AddZombie(int amount)
    {
        ZOMBIESINWORLD += amount;
    }
    public static void RemoveZombie()
    {
        if (ZOMBIESINWORLD > 0) ZOMBIESINWORLD--;
    }

    public static void BecomeTarget(ZombieTarget newTarget)
    {
        PLAYER_TARGET = newTarget;
    } 

}
