using UnityEngine;
using UnityEngine.Events;
using System.IO;

namespace Utility.Settings
{
    public class SettingsManager : MonoBehaviour
    {
        private static SettingsData settingsData;
        public SettingsData SettingsData {
            get => settingsData;
        }

        public UnityEvent<float> onSensitivityChanged;
        public UnityEvent<float> onFOVChanged;
        public UnityEvent<float> onMasterVolumeChanged;
        public UnityEvent<float> onMusicVolumeChanged;
        public UnityEvent<GraphicsQuality> onGraphicsQualityChanged;

        private readonly JsonDataService jsonDataService = new();

        public void LoadSettings()
        {
            try
            {
                settingsData = jsonDataService.LoadData<SettingsData>("/settings.json");
                
                if (settingsData == null)
                {
                    ResetSettings();
                }
            }
            catch (FileNotFoundException)
            {
                ResetSettings();
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
            onSensitivityChanged.Invoke(sensitivity);
        }

        public void SetFOV(float fov)
        {
            settingsData.FOV = fov;
            onFOVChanged.Invoke(fov);
        }

        public void SetMasterVolume(float volume)
        {
            settingsData.masterVolume = volume;
            onMasterVolumeChanged.Invoke(volume);
        }

        public void SetMusicVolume(float volume)
        {
            settingsData.musicVolume = volume;
            onMusicVolumeChanged.Invoke(volume);
        }

        public void SetGraphicsQuality(GraphicsQuality quality)
        {
            settingsData.graphicsQuality = quality;
            onGraphicsQualityChanged.Invoke(quality);
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

