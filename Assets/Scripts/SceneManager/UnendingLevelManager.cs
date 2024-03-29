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

public class UnendingLevelManager : MonoBehaviour
{
    [SerializeField] private UnityEvent onStartGame;
    [SerializeField] private List<ZombieSpawnerManager> zombieSpawnerManagers;
    

    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;
    private PlayerController player;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform[] possibleRewards;

    [Header("Missions")]
    [SerializeField] private Mission waitMission;
    [SerializeField] private List<Mission> possibleMissions;
    private Mission selectedMission;

    [Header("Village Generation")]
    [SerializeField] private List<GameObject> necessarybuildings;
    [SerializeField] private NavMeshSurface navMesh;
    [SerializeField] private VillageGenerator villageGenerator;


    private void Awake()
    {
        StartGame();
    }

    public async void StartGame()
    { 
        Time.timeScale = 1;
        
        Debug.Log("Started Level");
        villageGenerator.Generate(necessaryBuildingsToPlace: necessarybuildings);
        player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation).GetComponent<PlayerController>();
        player.OnRestartDemanded.AddListener(RestartLevel);

        foreach (var spawnerManager in zombieSpawnerManagers)
        {
            spawnerManager.SetTarget(player.GetComponent<DamageableObject>());
        }

        waitMission.onCompleted.AddListener(GiveNewPlayerRandomMission);

        foreach (Mission mission in possibleMissions)
        {
            mission.onCompleted.AddListener(OnMissionCompleted);
        }

        navMesh.BuildNavMesh();

        await Task.Delay(500);
        
        GiveNewPlayerRandomMission();

        foreach (var spawnerManager in zombieSpawnerManagers)
        {
            spawnerManager.BeginToSpawn();
        }

        onStartGame.Invoke();
    }

    //------------------------------------------mission methods
    private void GiveNewPlayerRandomMission()
    {
        Mission newMission;

        //we don't want the same mission twice
        do
        {
            newMission = possibleMissions[Random.Range(0, possibleMissions.Count - 1)];
        } while (selectedMission == newMission && possibleMissions.Count > 1);

        selectedMission = newMission;
        selectedMission.StartMission();

        player.SetMission(selectedMission);
    }

    private void OnMissionCompleted()
    {
        //here is where we give the choice
        Debug.Log("wait");
        waitMission.StartMission();
        GiveReward();
        player.SetMission(waitMission);
    }

    private void GiveReward()
    {
        if (possibleRewards.Length == 0){
            Debug.LogWarning("No rewards to give");
            return;
        }

        Transform reward = possibleRewards[Random.Range(0, possibleRewards.Length - 1)];
        Instantiate(reward, player.transform.position + Vector3.up * 100, Quaternion.identity);
    }

    private void RestartLevel()
    {
        SceneManager.LoadSceneAsync("UnEndingLevelScene");
    }

    //------------------------------------------unity events
    private void OnDestroy()
    {
        waitMission.onCompleted?.RemoveListener(GiveNewPlayerRandomMission);

        foreach (Mission mission in possibleMissions)
        {
            mission.onCompleted?.RemoveListener(OnMissionCompleted);
        }
    }
}
