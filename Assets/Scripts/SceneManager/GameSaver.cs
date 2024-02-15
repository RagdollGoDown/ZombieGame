using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;

public class GameSaver
{
    private static readonly JsonDataService dataService = new();

    public static void SavePlayerData(PlayerController player){
       SavePlayerData(player.GetSaveData());
    }

    public static void SavePlayerData(PlayerSaveData player){
        dataService.SaveData("player.json", player);
    }

    public static void LoadPlayerDataToPlayer(PlayerController player){
        player.SetPlayerData(dataService.LoadData<PlayerSaveData>("player.json"));
    }

    public static PlayerSaveData LoadPlayerData(){
        return dataService.LoadData<PlayerSaveData>("player.json");
    }
}
