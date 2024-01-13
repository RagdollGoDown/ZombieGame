using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using Utility;
using System.Linq;

public class ZombieSpawnerManager : MonoBehaviour
{
    private static ZombieSpawnerManager CURRENT_SPAWNER;

    private static List<ZombieSpawner> worldSpawnersAvailable;

    [SerializeField] private ObjectPool zombiePool;
    [SerializeField] private int baseQuantityPerRound = 10;
    [SerializeField] private int finalQuantityPerRound = 100;
    [SerializeField] private float timeFromStartToEndOfEscalationCurveSec = 240;
    private float currentEscalationCurveTimeSec = 0;
    [SerializeField] private AnimationCurve initialSpawnQuantityEscalationCurve;
    private AnimationCurve currentSpawnQuantityEscalationCurve;
    [SerializeField] private AnimationCurve[] spawnQuantityEscalationCurves;


    //it's good to have this low so that the zombies are spread out
    [SerializeField] private int MaxZombiesGivenPerSpawner = 5;
    [SerializeField] private int timeBetweenSpawnsMilliSec = 50;
    [SerializeField] private float timeBetweenRounds = 2;

    private DamageableObject target;
    private Reaper zombieReaper;

    //------------------------------- unity events

    private void Awake()
    {
        if (spawnQuantityEscalationCurves.Length == 0) throw new System.ArgumentException("Need at least one curve");

        if (timeBetweenSpawnsMilliSec < 1) throw new System.ArgumentException("Time between rounds not bigger or equal to 1");
        if (timeBetweenRounds <= 0) throw new System.ArgumentException("Time before first round not strictly positiv");

        if (zombiePool == null) throw new System.NullReferenceException("No object pool to take zombies from");
        zombiePool.ReadyInitialObjectsAsync(finalQuantityPerRound);

        CURRENT_SPAWNER = this;
        zombieReaper = new();
    }

    private void OnDisable()
    {
        if (CURRENT_SPAWNER == this) CURRENT_SPAWNER = null;
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
            currentEscalationCurveTimeSec = 0;
            currentSpawnQuantityEscalationCurve = initialSpawnQuantityEscalationCurve;
            InvokeRepeating(nameof(SpawnZombies), timeBetweenRounds, timeBetweenRounds);
        }
        else
        {
            Debug.Log("No spawners created spawners");
        }
    }

    private async void SpawnZombies()
    {
        if (worldSpawnersAvailable.Count == 0)
        {
            Debug.Log("No active Spawners");
            return;
        }

        int zombiesToSpawn = EvaluateZombiesToBeSpawned();
        int tempAmount;

        if (zombiesToSpawn < 1 && target != null) { return; }

        while (zombiesToSpawn > 0)
        {

            tempAmount = Random.Range(1, Mathf.Min(MaxZombiesGivenPerSpawner, zombiesToSpawn + 1));
            zombiesToSpawn -= tempAmount;

            worldSpawnersAvailable[Random.Range(0, worldSpawnersAvailable.Count - 1)]
                .AddZombiesToSpawn(tempAmount, target, zombiePool, zombieReaper); ;

            await Task.Delay(timeBetweenSpawnsMilliSec);
        }
    }

    private bool IsSpawning()
    {
        bool b = false;

        foreach(ZombieSpawner zs in worldSpawnersAvailable)
        {
            b = zs.IsSpawning() || b;
        }

        return b;
    }

    private int EvaluateZombiesToBeSpawned()
    {
        currentEscalationCurveTimeSec += timeBetweenRounds;

        if (currentEscalationCurveTimeSec > timeFromStartToEndOfEscalationCurveSec)
        {
            currentEscalationCurveTimeSec = 0;
            currentSpawnQuantityEscalationCurve =
                spawnQuantityEscalationCurves[Random.Range(0, spawnQuantityEscalationCurves.Length - 1)];
        }

        float lerpTime = initialSpawnQuantityEscalationCurve.Evaluate(currentEscalationCurveTimeSec / timeFromStartToEndOfEscalationCurveSec);

        return ((int)Mathf.Lerp(baseQuantityPerRound, finalQuantityPerRound, lerpTime));
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

    public Reaper GetReaper()
    {
        return zombieReaper;
    }
}
