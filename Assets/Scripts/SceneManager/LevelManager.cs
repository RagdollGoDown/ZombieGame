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
    [SerializeField] private ZombieSpawnerManager zombieSpawnerManager;

    [Header("Missions")]
    [SerializeField] private Mission mainMission;
    [SerializeField] private Mission waitMission;
    [SerializeField] private List<Mission> possibleMissions;
    private Mission selectedMission;

    [SerializeField] private NavMeshSurface navMesh;
    [SerializeField] private VillageGenerator villageGenerator;
    [SerializeField] private Transform spawnPoint;

    [SerializeField] private List<GameObject> necessarybuildings;

    [SerializeField] private bool playerUsesSaveData = true;

    private PlayerController player;

    private int moneyToBeWon;

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

        zombieSpawnerManager.SetTarget(player.GetComponent<DamageableObject>());

        navMesh.BuildNavMesh();

        waitMission.onCompleted.AddListener(GiveNewPlayerRandomMission);
        foreach (Mission mission in possibleMissions)
        {
            mission.onCompleted.AddListener(OnMissionCompleted);
        }

        await Task.Delay(500);
        zombieSpawnerManager.BeginToSpawn();
        GiveNewPlayerRandomMission();
        onStartGame.Invoke();
    }

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
        moneyToBeWon += selectedMission.RewardMoney;
        //here is where we give the choice
        Debug.Log("wait");
        waitMission.StartMission();
        player.SetMission(waitMission);
    }

    public async void EndLevel()
    { 
        Debug.Log("end Level");
        player.AddMoney(moneyToBeWon);

        GameSaver.SaveData("player", player.GetSaveData());

        SceneManager.LoadSceneAsync("Hub");        
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
