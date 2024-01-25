using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.AI;
using Utility;
using Objectives;
using UnityEngine.InputSystem;
using Unity.AI.Navigation;
using MapGeneration.VillageGeneration;

public class GameManager : MonoBehaviour
{
    private ZombieSpawnerManager zombieSpawnerManager;

    [SerializeField] private UnityEvent onStartGame;
    [SerializeField] private Mission mainMission;
    [SerializeField] private NavMeshSurface navMesh;
    [SerializeField] private VillageGenerator villageGenerator;

    private void Awake()
    {
        //objectivesManager = transform.Find("ObjectivesManager").GetComponent<ObjectivesManager>();

        zombieSpawnerManager = transform.Find("ZombieSpawnManager").GetComponent<ZombieSpawnerManager>();
    }

    public void StartGame(PlayerInput playerInput)
    { 
        Debug.Log("Started Game");
        mainMission.StartMission();
        villageGenerator.Generate(necessaryBuildingsToPlace: mainMission.ObjectiveObjects().Select(obj => obj.gameObject).ToList());
        zombieSpawnerManager.SetTarget(playerInput.GetComponent<DamageableObject>());

        navMesh.BuildNavMesh();

        zombieSpawnerManager.BeginToSpawn();
    }
}
