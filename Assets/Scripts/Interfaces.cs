using UnityEngine;
using UnityEngine.Audio;

namespace Guarana.Interfaces
{
    public interface ISoundManager
    {
        public AudioSource MusicAudioSource { get; set; }
        public AudioSource SfxAudioSource { get; set; }
        public AudioClip Music { get; set; }
        public AudioMixerGroup MusicMixerGroup { get; set; }
        public AudioMixerGroup SfxMixerGroup { get; set; }
        
        void ChangeMusic(AudioClip audioClip, bool mainMusic = false);
        void PlaySound(AudioClip audioClip);
        void Reset();
    }
}