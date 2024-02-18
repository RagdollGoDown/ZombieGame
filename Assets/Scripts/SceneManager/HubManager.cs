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

    [SerializeField] private bool setSaveFile = true;
    [SerializeField] private int saveFileIndex = 1;

    private void Awake()
    {
        player = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity).GetComponent<PlayerController>();

        if (setSaveFile) GameSaver.SetCurrentSaveFile(saveFileIndex);

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
        SceneManager.LoadScene("SimpleLevel");
    }
}
