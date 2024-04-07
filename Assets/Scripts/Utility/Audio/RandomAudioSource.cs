using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utility.Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class RandomAudioSource : MonoBehaviour
    {
        private AudioSource audioSource;

        [SerializeField] private List<AudioClip> clips;

        public void SelectRandomClip()
        {
            audioSource = GetComponent<AudioSource>();

            audioSource.clip = clips[Random.Range(0, clips.Count)];
        }

        public void Play()
        {
            SelectRandomClip();

            audioSource.Play();
        }
    }
}
