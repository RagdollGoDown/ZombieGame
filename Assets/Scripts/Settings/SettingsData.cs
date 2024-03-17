using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Settings
{
    [Serializable]
    public class SettingsData
    {
        public float sensitivity = 1;

        public float FOV = 60;

        public float musicVolume = 1;
        public float masterVolume = 1;

        public GraphicsQuality graphicsQuality = GraphicsQuality.High;
    }

    [Serializable]
    public enum GraphicsQuality
    {
        Low,
        Medium,
        High
    }
}
