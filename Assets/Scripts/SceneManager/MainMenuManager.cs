using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public static void SetCurrentSaveFile(int saveFile)
    {
        GameSaver.CurrentSaveFileIndex = saveFile;
    }

    public static void StartGame()
    {
        SceneManager.LoadSceneAsync("HubScene");
    }

    public static void Quit()
    {
        Application.Quit();
    }
}
