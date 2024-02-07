using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Utility;
using Objectives;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Unity.AI.Navigation;
using MapGeneration.VillageGeneration;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private UnityEvent onStartGame;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private ZombieSpawnerManager zombieSpawnerManager;
    [SerializeField] private Mission mainMission;
    [SerializeField] private NavMeshSurface navMesh;
    [SerializeField] private VillageGenerator villageGenerator;
    [SerializeField] private Transform spawnPoint;

    [SerializeField] private List<GameObject> necessarybuildings;

    private void Awake()
    {
        StartGame();
    }

    public async void StartGame()
    { 
        Debug.Log("Started Level");
        villageGenerator.Generate(necessaryBuildingsToPlace: necessarybuildings);
        GameObject player = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
        zombieSpawnerManager.SetTarget(player.GetComponent<DamageableObject>());

        mainMission.StartMission();

        navMesh.BuildNavMesh();

        await Task.Delay(500);
        zombieSpawnerManager.BeginToSpawn();

        player.GetComponent<PlayerController>().SetMission(mainMission);
        onStartGame.Invoke();
    }

    public async void EndLevel()
    { 
        Debug.Log("end Level");

        SceneManager.LoadSceneAsync("Hub");        
    }
}
