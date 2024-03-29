using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Utility.Settings
{
    [Serializable]
    public class SettingsData
    {
        public float sensitivity = 1;

        public float FOV = 0.5f;

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
