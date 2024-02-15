using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System;

public class JsonDataService : IDataService
{
    public bool SaveData<T>(string relativePath, T data, bool encrypted = true)
    {
        string path = Application.persistentDataPath + relativePath;

        try
        {
            if (File.Exists(path))
            {
                Debug.Log("File already exists, deleting it");
                File.Delete(path);
            }
            else
            {
                Debug.Log("Creating File: " + relativePath + " for the first time");
            }
            using FileStream stream = File.Create(path);
            stream.Close();
            File.WriteAllText(path, JsonConvert.SerializeObject(data));
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("Error saving file: " + e.Message);
            return false;
        }

    }

    public T LoadData<T>(string RelativePath, bool encrypted = true)
    {
        string path = Application.persistentDataPath + RelativePath;

        if (!File.Exists(path))
        {
            Debug.LogError("File not found: " + path);
            throw new FileNotFoundException("File not found: " + path);
        } 

        try
        {
            T data = JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError("Error loading file: " + e.Message);
            throw e;
        }
    }
}