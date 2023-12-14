using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using Utility;
using System.Linq;

public class ZombieSpawnerManager : MonoBehaviour
{
    private static int ZOMBIESINWORLD;
    private static ZombieSpawnerManager CURRENT_SPAWNER;

    private static List<ZombieSpawner> worldSpawnersAvailable;


    private int _maxZombiesInWorld;
    [SerializeField] private ObjectPool zombiePool;
    [SerializeField] private int baseMaxZombiesPerRound = 10;
    [SerializeField] private int finalMaxZombiesPerRound = 100;

    //proximity of spawners to player
    [SerializeField] private int numberOfSpawnersClosestToTake = 20;

    //it's good to have this low so that the zombies are spread out
    [SerializeField] private int MaxZombiesGivenPerSpawner = 2;
    [SerializeField] private int escalationPerRound = 3;
    [SerializeField] private float timeBetweenSpawns = 1;
    [SerializeField] private float timeBeforeFirstRound = 2;

    private DamageableObject target;
    [SerializeField] private float targetVelocityBias = 2;

    public enum SpawnerState
    {
        MIDROUND,
        BREAK,
        UNKNOWN
    }
    private SpawnerState worldSpawnersAvailabletate;

    private int _currentRound;

    //------------------------------- unity events

    private void Awake()
    {
        if (numberOfSpawnersClosestToTake < 1) throw new System.ArgumentException("Number of spawners closest to take not bigger or equal to 1");

        if (timeBetweenSpawns < 1) throw new System.ArgumentException("Time between rounds not bigger or equal to 1");
        if (timeBeforeFirstRound <= 0) throw new System.ArgumentException("Time before first round not strictly positiv");

        if (zombiePool == null) throw new System.NullReferenceException("No object pool to take zombies from");
        zombiePool.ReadyInitialObjects(finalMaxZombiesPerRound);

        _maxZombiesInWorld = baseMaxZombiesPerRound;

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
        worldSpawnersAvailabletate = SpawnerState.BREAK;

        Invoke(nameof(EnterRound), timeBetweenSpawns);
    }

    private void EnterRound()
    {
        Debug.Log("Round");
        _currentRound++;
        worldSpawnersAvailabletate = SpawnerState.MIDROUND;

        StartCoroutine(nameof(SpawnZombies));
        //SpawnZombies();
    }

    public void BeginToSpawn()
    {
        if (CURRENT_SPAWNER != this) 
        {
            Debug.Log("There are two zombie spawners or none enabled");
        }
        else if (worldSpawnersAvailable.Count > 0)
        {
            Debug.Log("Started Spawning");
            Invoke(nameof(EnterRound), timeBeforeFirstRound);
        }
        else
        {
            Debug.Log("No spawners created spawners");
        }
    }

    private IEnumerator SpawnZombies()
    {
        if (worldSpawnersAvailable.Count == 0)
        {
            Debug.Log("No active Spawners");
            yield break;
        }

        //Debug.Log(ZOMBIESINWORLD + " zombiesInworld " + _maxZombiesInWorld + " max zombies");
        int zombiesToSpawn = _maxZombiesInWorld - ZOMBIESINWORLD;

        if (_maxZombiesInWorld < finalMaxZombiesPerRound) _maxZombiesInWorld += Mathf.Min(escalationPerRound, finalMaxZombiesPerRound - _maxZombiesInWorld);

        if (zombiesToSpawn < 1 && target != null) { yield break; }

        //We get the closest spawners of the aimed position
        Vector3 aimedPosition;
        List<ZombieSpawner> closest;

        int index = 0;

        while (zombiesToSpawn > 0)
        {

            aimedPosition = target.transform.position + target.GetPossibleCharacterController().velocity * targetVelocityBias;

            closest = worldSpawnersAvailable.
                OrderBy(zs => Vector3.Distance(zs.transform.position, aimedPosition)).
                Take(numberOfSpawnersClosestToTake).ToList();
            
            int a = closest[index].
                AddZombiesToSpawn(Random.Range(1, Mathf.Min(MaxZombiesGivenPerSpawner, zombiesToSpawn + 1)), target, zombiePool);

            zombiesToSpawn -= a;

            yield return new WaitForSeconds(timeBetweenSpawns);

            index = (++index & (closest.Count - 1));
        }

        StartCoroutine(nameof(SpawnZombies));
    }

    /*private void SpawnZombies()
    {
        if (worldSpawnersAvailable.Count == 0) 
        {
            Debug.Log("No active Spawners");
            return;
        }

        //Debug.Log(ZOMBIESINWORLD + " zombiesInworld " + _maxZombiesInWorld + " max zombies");
        int zombiesToSpawn = _maxZombiesInWorld - ZOMBIESINWORLD;

        if (_maxZombiesInWorld < finalMaxZombiesPerRound) _maxZombiesInWorld += Mathf.Min(escalationPerRound, finalMaxZombiesPerRound - _maxZombiesInWorld);

        if (zombiesToSpawn < 1 && target != null) { return; }

        //We get the closest spawners of the aimed position
        Vector3 aimedPosition = target.transform.position + target.GetPossibleRigidbody().velocity * velocityBias;
        aimepos = aimedPosition;
        List<ZombieSpawner> closest = worldSpawnersAvailable.
            OrderBy(zs => Vector3.Distance(zs.transform.position, aimedPosition)).
            Take(numberOfSpawnersClosestToTake).ToList();

        int index = 0;

        while (zombiesToSpawn > 0)
        {
            int a = closest[index].
                AddZombiesToSpawn(Random.Range(1, Mathf.Min(MaxZombiesGivenPerSpawner, zombiesToSpawn + 1)), target, zombiePool);

            zombiesToSpawn -= a;

            index = (++index & (closest.Count-1));
        }
    }*/

    private bool IsSpawning()
    {
        bool b = false;

        foreach(ZombieSpawner zs in worldSpawnersAvailable)
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

    //-------------------------------set/getters

    /// <summary>
    /// Adds the new spawner to a list that keeps the spawners in the scene for the manager
    /// </summary>
    /// <param name="newZombieSpawner">the spawner to be added</param>
    public static void AddSpawner(ZombieSpawner newZombieSpawner)
    {
        if (worldSpawnersAvailable == null) worldSpawnersAvailable = new();

        worldSpawnersAvailable.Add(newZombieSpawner);
    }

    /// <summary>
    /// Removes the spawner
    /// </summary>
    /// <param name="newZombieSpawner"></param>
    public static void RemoveSpawner(ZombieSpawner newZombieSpawner)
    {
        worldSpawnersAvailable?.Remove(newZombieSpawner);
    }

    public static SpawnerState GetCurrentSpawnerState() {
        if (CURRENT_SPAWNER) return CURRENT_SPAWNER.worldSpawnersAvailabletate;
        else return SpawnerState.UNKNOWN;
    }

    /// <summary>
    /// The target for all the zombies
    /// It needs to have a rigidbody to take it's velocity from
    /// </summary>
    /// <param name="target">the selected target</param>
    public void SetTarget(DamageableObject target)
    {
        if (target.GetPossibleCharacterController() == null) throw new System.NullReferenceException("The player doesn't have a CharacterController to get the velocity from");

        this.target = target;
    }
}
