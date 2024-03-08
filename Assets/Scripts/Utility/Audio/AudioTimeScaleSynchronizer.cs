using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utility.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioTimeScaleSynchronizer : MonoBehaviour
    {
        [SerializeField] private float pitchMultiplier = 1.0f;
        private AudioSource audioSource;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        private void Update()
        {
            audioSource.pitch = Time.timeScale * pitchMultiplier;
        }
    }
}
