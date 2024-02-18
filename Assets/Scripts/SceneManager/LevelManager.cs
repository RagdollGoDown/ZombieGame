using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Utility;
using Objectives;
using UnityEngine.SceneManagement;
using Unity.AI.Navigation;
using MapGeneration.VillageGeneration;
using Player;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private UnityEvent onStartGame;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private List<ZombieSpawnerManager> zombieSpawnerManagers;
    [SerializeField] private Mission mainMission;
    [SerializeField] private NavMeshSurface navMesh;
    [SerializeField] private VillageGenerator villageGenerator;
    [SerializeField] private Transform spawnPoint;

    [SerializeField] private List<GameObject> necessarybuildings;

    [SerializeField] private bool playerUsesSaveData = true;

    private PlayerController player;

    private void Awake()
    {
        StartGame();
    }

    public async void StartGame()
    { 
        Debug.Log("Started Level");
        villageGenerator.Generate(necessaryBuildingsToPlace: necessarybuildings);
        player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation).GetComponent<PlayerController>();

        if (playerUsesSaveData) player.SetPlayerData(GameSaver.LoadData<PlayerSaveData>("player"));

        foreach (var spawnerManager in zombieSpawnerManagers)
        {
            spawnerManager.SetTarget(player.GetComponent<DamageableObject>());
        }

        mainMission.StartMission();

        navMesh.BuildNavMesh();

        await Task.Delay(500);
        
        foreach (var spawnerManager in zombieSpawnerManagers)
        {
            spawnerManager.BeginToSpawn();
        }

        player.SetMission(mainMission);
        onStartGame.Invoke();
    }

    public async void EndLevel()
    { 
        Debug.Log("end Level");
        player.AddMoney(mainMission.RewardMoney);

        GameSaver.SaveData("player", player.GetSaveData());

        SceneManager.LoadSceneAsync("Hub");        
    }
}
