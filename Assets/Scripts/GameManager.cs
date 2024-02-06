using System.Threading.Tasks;
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
    private PlayerController player;

    [SerializeField] private UnityEvent onStartGame;
    [SerializeField] private Mission mainMission;
    [SerializeField] private NavMeshSurface navMesh;
    [SerializeField] private VillageGenerator villageGenerator;
    [SerializeField] private GameObject spawn;
    [SerializeField] private Transform spawnPoint;

    private void Awake()
    {
        //objectivesManager = transform.Find("ObjectivesManager").GetComponent<ObjectivesManager>();

        zombieSpawnerManager = transform.Find("ZombieSpawnManager").GetComponent<ZombieSpawnerManager>();
    }

    public async void StartGame(PlayerInput playerInput)
    { 
        Debug.Log("Started Game");
        villageGenerator.Generate(necessaryBuildingsToPlace: mainMission.ObjectiveObjects().Select(obj => obj.gameObject).Append(spawn).ToList());
        zombieSpawnerManager.SetTarget(playerInput.GetComponent<DamageableObject>());
        player = playerInput.GetComponent<PlayerController>();
        player.GetComponent<CharacterController>().enabled = false;
        player.transform.position = spawnPoint.position;

        mainMission.StartMission();

        navMesh.BuildNavMesh();
        zombieSpawnerManager.BeginToSpawn();

        await Task.Delay(500);
        player.GetComponent<CharacterController>().enabled = true;

        player.SetMission(mainMission);
        onStartGame.Invoke();
    }
}
