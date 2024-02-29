using Player;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.IO;

public class HubManager : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private Transform initSpawnPoint;
    private PlayerController player;

    public void Awake()
    {
        player = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity).GetComponent<PlayerController>();

        try
        {
            player.SetPlayerData(GameSaver.LoadData<PlayerSaveData>("player"));
        }
        catch (FileNotFoundException)
        {
            GameSaver.SaveData("player", player.GetSaveData());
        }
    }

    public void LoadSimpleLevel()
    {
        GameSaver.SaveData("player", player.GetSaveData());
        SceneManager.LoadSceneAsync("LevelScene");
    }
}
