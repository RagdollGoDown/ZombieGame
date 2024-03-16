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
    [SerializeField] private List<ZombieSpawnerManager> zombieSpawnerManagers;
    

    [Header("Player")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private bool playerUsesSaveData = false;
    private PlayerController player;
    [SerializeField] private Transform spawnPoint;

    [Header("Missions")]
    [SerializeField] private Mission waitMission;
    [SerializeField] private List<Mission> possibleMissions;
    private Mission selectedMission;

    [SerializeField] private GameObject exitDoor;


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
        Debug.Log("Started Level");
        villageGenerator.Generate(necessaryBuildingsToPlace: necessarybuildings);
        player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation).GetComponent<PlayerController>();

        if (playerUsesSaveData) player.SetPlayerData(GameSaver.LoadData<PlayerSaveData>("player"));

        foreach (var spawnerManager in zombieSpawnerManagers)
        {
            spawnerManager.SetTarget(player.GetComponent<DamageableObject>());
        }

        CloseExitDoor();

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
        CloseExitDoor();

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
        OpenExitDoor();
        player.AddMoney(selectedMission.RewardMoney);
        //here is where we give the choice
        Debug.Log("wait");
        waitMission.StartMission();
        player.SetMission(waitMission);
    }

    public void EndLevel()
    { 
        Debug.Log("end Level");

        GameSaver.SaveData("player", player.GetSaveData());

        SceneManager.LoadSceneAsync("HubScene");        
    }

    private void OpenExitDoor()
    {
        exitDoor.SetActive(true);
    }

    private void CloseExitDoor()
    {
        exitDoor.SetActive(false);
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
