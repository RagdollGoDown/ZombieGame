using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;
using System.IO;

public class GameSaver
{
    private static readonly JsonDataService dataService = new();

    private static int MaxSaveFiles = 3;

    private static int currentSaveFileIndex = 1;

    public static int CurrentSaveFileIndex {
        get => currentSaveFileIndex;
        set {
            if (value > 0 && value <= MaxSaveFiles) {
                currentSaveFileIndex = value;
            }
            else{
                Debug.LogError($"Invalid save file index, must be between 1 and {MaxSaveFiles}.");
            }
        }
    }

    /// <summary>
    /// Save data to a file of relative path filename in the current save file.
    /// If the save file doesn't exist, it will create it
    /// </summary>
    /// <param name="fileName">the name of the file with it's relative path</param>
    /// <param name="data">the data to be stored</param>
    public static void SaveData(string fileName, object data){
        if (Directory.Exists(Application.persistentDataPath + $"/File{currentSaveFileIndex}"))
            Directory.CreateDirectory(Application.persistentDataPath + $"/File{currentSaveFileIndex}");

        dataService.SaveData($"/File{currentSaveFileIndex}/" + fileName, data);
    }

    /// <summary>
    /// Load data from a file of relative path filename in the current save file.
    /// </summary>
    /// <typeparam name="T">the object type to be saved</typeparam>
    /// <param name="fileName">the name of the file with the relative path starting from the persistent path and the file {current save file index}</param>
    /// <returns>the loaded data if it finds and can decrypt it or else an error</returns>
    public static T LoadData<T>(string fileName){
        return dataService.LoadData<T>($"/File{currentSaveFileIndex}/" + fileName);
    }
}
