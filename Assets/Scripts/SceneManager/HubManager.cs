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

    private void Awake()
    {
        player = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity).GetComponent<PlayerController>();

        try 
        {
            player.SetPlayerData(GameSaver.LoadPlayerData());
        }
        catch (FileNotFoundException)
        {
            GameSaver.SavePlayerData(player);
        }
    }

    public void LoadSimpleLevel()
    {
        GameSaver.SavePlayerData(player);
        SceneManager.LoadScene("SimpleLevel");
    }
}
