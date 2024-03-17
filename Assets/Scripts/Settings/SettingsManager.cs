using UnityEngine;
using System.IO;

namespace Settings
{
    public class SettingsManager : MonoBehaviour
    {
        private static SettingsData settingsData;

        private JsonDataService jsonDataService;

        private void Awake()
        {
            jsonDataService = new JsonDataService();

            if (settingsData == null)
            {
                try
                {
                    settingsData = jsonDataService.LoadData<SettingsData>("/settings.json");
                }
                catch (FileNotFoundException)
                {
                    settingsData = new SettingsData();
                    jsonDataService.SaveData("/settings.json", settingsData);
                }
            }
        }

        public void SaveSettings()
        {
            jsonDataService.SaveData("/settings.json", settingsData);
        }

        public void ResetSettings()
        {
            settingsData = new SettingsData();
            jsonDataService.SaveData("/settings.json", settingsData);
        }

        //------- settings modification methods

        public void SetSensitivity(float sensitivity)
        {
            settingsData.sensitivity = sensitivity;
        }

        public void SetFOV(float fov)
        {
            settingsData.FOV = fov;
        }

        public void SetMasterVolume(float volume)
        {
            settingsData.masterVolume = volume;
        }

        public void SetMusicVolume(float volume)
        {
            settingsData.musicVolume = volume;
        }

        public void SetGraphicsQuality(GraphicsQuality quality)
        {
            settingsData.graphicsQuality = quality;
        }

        //------- settings retrieval methods

        public float GetSensitivity()
        {
            return settingsData.sensitivity;
        }

        public float GetMasterVolume()
        {
            return settingsData.masterVolume;
        }

        public float GetMusicVolume()
        {
            return settingsData.musicVolume;
        }

        public GraphicsQuality GetGraphicsQuality()
        {
            return settingsData.graphicsQuality;
        }
    }
}

