using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieSpawnerManager : MonoBehaviour
{
    private static ZombieTarget PLAYER_TARGET;
    private static int ZOMBIESINWORLD;
    private static ZombieSpawnerManager CURRENT_SPAWNER;

    private int _maxZombiesInWorld;
    [SerializeField] private int baseMaxZombiesPerRound;
    [SerializeField] private int escalationPerRound;
    [SerializeField] private int timeBetweenRounds = 10;
    [SerializeField] private int timeBeforeFirstRound = 2;

    private enum SpawnerState
    {
        MIDROUND,
        BREAK
    }
    private SpawnerState _spawnerState;

    private ZombieSpawner[] _spawners;
    [SerializeField]private ZombieSpawner spawner;
    private int _indexInSpawners;

    private int _currentRound;

    //-------------------------------events

    private void Awake()
    {
        _maxZombiesInWorld = baseMaxZombiesPerRound;

        if (timeBetweenRounds <= 0) throw new System.ArgumentException("Time between rounds not strictly positiv");
        if (timeBeforeFirstRound <= 0) throw new System.ArgumentException("Time before first round not strictly positiv");

        if (timeBetweenRounds < 1) { timeBetweenRounds = 1; }

        _spawners = GetComponentsInChildren<ZombieSpawner>();

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
        Debug.Log((CURRENT_SPAWNER == this));
        if (CURRENT_SPAWNER != this) return;

        if (_spawners.Length > 0 && CanSpawn())
        {
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

        if (ZOMBIESINWORLD == 0 && 
            CURRENT_SPAWNER && !CURRENT_SPAWNER.IsSpawning()) 
            CURRENT_SPAWNER.EnterBreakTime();
    }

    public static void BecomeTarget(ZombieTarget newTarget)
    {
        PLAYER_TARGET = newTarget;
    } 

}
