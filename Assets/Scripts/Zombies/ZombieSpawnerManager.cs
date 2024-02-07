using System.Threading.Tasks;
using System.Threading;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using Utility;
using System.Linq;

public class ZombieSpawnerManager : MonoBehaviour
{
    private static Vector3 SPAWNER_SECTOR_WIDTH = new(20, 100, 20);

    private static SectorCollection<ZombieSpawner> worldSpawnersAvailableSectors;

    [SerializeField] private ObjectPool zombiePool;
    [SerializeField] private int minQuantity = 10;
    [SerializeField] private int maxQuantity = 100;
    
    private int quantitySpawned;

    private int currentLivingQuantity;

    [SerializeField] private float timeFromStartToEndOfEscalationCurveSec = 240;
    private float currentEscalationCurveTimeSec = 0;
    [SerializeField] private AnimationCurve initialSpawnQuantityEscalationCurve;
    private AnimationCurve currentSpawnQuantityEscalationCurve;
    [SerializeField] private AnimationCurve[] spawnQuantityEscalationCurves;


    //it's good to have this low so that the zombies are spread out
    [SerializeField] private int maxZombiesGivenPerSpawner = 5;
    [SerializeField] private int maxZombiesGivenPerRound = 5;
    [SerializeField] private int timeBetweenSpawnsMilliSec = 50;
    [SerializeField] private float timeBetweenRounds = 2;
    [SerializeField] private int radiusOfSectorsTaken = 1;
    [SerializeField] private float velocityBias = 0;

    private CancellationTokenSource spawnZombiesAsyncCancelTokenSource;
    private CancellationToken spawnZombiesAsyncCancelToken;

    private DamageableObject target;
    private Reaper zombieReaper;

    //------------------------------- unity events

    private void Awake()
    {
        if (spawnQuantityEscalationCurves.Length == 0) throw new System.ArgumentException("Need at least one curve");

        if (timeBetweenSpawnsMilliSec < 1) throw new System.ArgumentException("Time between rounds not bigger or equal to 1");
        if (timeBetweenRounds <= 0) throw new System.ArgumentException("Time before first round not strictly positiv");

        if (zombiePool == null) throw new System.NullReferenceException("No object pool to take zombies from");
        zombiePool.ReadyInitialObjectsAsync(maxQuantity);

        zombieReaper = new();
    }

    private void OnDisable()
    {
        spawnZombiesAsyncCancelTokenSource?.Cancel();
    }

    //--------------------------------------------spawning methods
    public void BeginToSpawn()
    {
        if (worldSpawnersAvailableSectors.Count > 0)
        {
            Debug.Log("Started Spawning " + name);

            spawnZombiesAsyncCancelTokenSource = new();
            spawnZombiesAsyncCancelToken = spawnZombiesAsyncCancelTokenSource.Token;
            currentEscalationCurveTimeSec = 0;
            currentSpawnQuantityEscalationCurve = initialSpawnQuantityEscalationCurve;
            RepeatZombieSpawning();
        }
        else
        {
            Debug.Log("No spawners created spawners");
        }
    }

    private async void RepeatZombieSpawning()
    {
        while(!spawnZombiesAsyncCancelToken.IsCancellationRequested){
            SpawnZombies();
            await Task.Delay((int)(timeBetweenRounds * 1000));
        }
    } 

    private async void SpawnZombies()
    {
        if (worldSpawnersAvailableSectors.Count == 0)
        {
            Debug.Log("No active Spawners");
            return;
        }

        int zombiesToSpawn = EvaluateZombiesToBeSpawned();
        int tempAmount;

        if (zombiesToSpawn < 1 && target != null) { return; }

        //find spawners near the sector
        List<ZombieSpawner> spawnersInSector;
        int radius = radiusOfSectorsTaken;
        Vector3 spawnedPosition = target.transform.position;

        if (velocityBias != 0){
            spawnedPosition += target.GetPossibleCharacterController().velocity * velocityBias;
        }

        while(!worldSpawnersAvailableSectors.TryGetValue(spawnedPosition, out spawnersInSector, new Vector3(radius,1,radius++)))
        {
            if (radius > 10 + radiusOfSectorsTaken)
            {
                Debug.Log("No spawners found in the sector");
                return;
            }
        }
        

        while (zombiesToSpawn > 0)
        {
            tempAmount = Random.Range(1, Mathf.Min(maxZombiesGivenPerSpawner, zombiesToSpawn + 1));
            tempAmount = spawnersInSector[Random.Range(0, spawnersInSector.Count - 1)].AddZombiesToSpawn(tempAmount, target, zombiePool, zombieReaper);
            zombiesToSpawn -= tempAmount;
            quantitySpawned += tempAmount;

            await Task.Delay(timeBetweenSpawnsMilliSec);
        }
    }

    private bool IsSpawning()
    {
        bool b = false;

        foreach(ZombieSpawner zs in worldSpawnersAvailableSectors.Values.SelectMany(x => x))
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

        int currentLivingQuantity = quantitySpawned - zombieReaper.GetReapedObjects().Count;
        
        return Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(minQuantity, maxQuantity, lerpTime)) - currentLivingQuantity,0,maxZombiesGivenPerRound);
    }

    //-------------------------------set/getters

    /// <summary>
    /// Adds the new spawner to a list that keeps the spawners in the scene for the manager
    /// </summary>
    /// <param name="newZombieSpawner">the spawner to be added</param>
    public static void AddSpawner(ZombieSpawner newZombieSpawner)
    {
        worldSpawnersAvailableSectors ??= new(SPAWNER_SECTOR_WIDTH);

        worldSpawnersAvailableSectors.Add(newZombieSpawner);
    }

    /// <summary>
    /// Removes the spawner
    /// </summary>
    /// <param name="newZombieSpawner"></param>
    public static void RemoveSpawner(ZombieSpawner newZombieSpawner)
    {
        worldSpawnersAvailableSectors?.Remove(newZombieSpawner);
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
