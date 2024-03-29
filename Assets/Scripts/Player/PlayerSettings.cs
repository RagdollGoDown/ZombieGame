using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using Utility.Settings;

namespace Player
{
    public class PlayerSettings : MonoBehaviour
    {
        private static float VOLUME_PROPORTIONALITY = 40.0f;
        private static float VOLUME_BIAS = -20.0f;

        private PlayerController player;

        [SerializeField] private SettingsManager settingsManager;

        [SerializeField] private AudioMixer audioMixer;

        private void Awake()
        {
            if (settingsManager == null)
            {
                throw new System.Exception("SettingsManager is not assigned to PlayerSettings");
            }

            if (audioMixer == null)
            {
                throw new System.Exception("AudioMixer is not assigned to PlayerSettings");
            }

            settingsManager.LoadSettings();
            player = GetComponent<PlayerController>();

            settingsManager.onSensitivityChanged.AddListener(player.SetSensitivity);
            settingsManager.onFOVChanged.AddListener(player.SetFOV);
            settingsManager.onMasterVolumeChanged.AddListener(SetMasterVolume);
            settingsManager.onMusicVolumeChanged.AddListener(SetMusicVolume);
            settingsManager.onGraphicsQualityChanged.AddListener(SetGraphicsQuality);

            settingsManager.onSensitivityChanged.Invoke(settingsManager.SettingsData.sensitivity);
            settingsManager.onFOVChanged.Invoke(settingsManager.SettingsData.FOV);
            settingsManager.onMasterVolumeChanged.Invoke(settingsManager.SettingsData.masterVolume);
            settingsManager.onMusicVolumeChanged.Invoke(settingsManager.SettingsData.musicVolume);
            settingsManager.onGraphicsQualityChanged.Invoke(settingsManager.SettingsData.graphicsQuality);
        }

        private void OnDestroy()
        {
            settingsManager.onSensitivityChanged.RemoveListener(player.SetSensitivity);
            settingsManager.onFOVChanged.RemoveListener(player.SetFOV);
            settingsManager.onMasterVolumeChanged.RemoveListener(SetMasterVolume);
            settingsManager.onMusicVolumeChanged.RemoveListener(SetMusicVolume);
            settingsManager.onGraphicsQualityChanged.RemoveListener(SetGraphicsQuality);
        }

        private void SetMasterVolume(float volume)
        {
            audioMixer.SetFloat("MasterVolume", volume * VOLUME_PROPORTIONALITY + VOLUME_BIAS);
        }

        private void SetMusicVolume(float volume)
        {
            audioMixer.SetFloat("MusicVolume", volume * VOLUME_PROPORTIONALITY + VOLUME_BIAS);
        }

        private void SetGraphicsQuality(GraphicsQuality quality)
        {
            QualitySettings.SetQualityLevel((int)quality);
        }
    }
}