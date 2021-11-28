using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioScript : MonoBehaviour
{
    [SerializeField] AudioSource audioSource, musicSource;
    [SerializeField] List<AudioClip> audios;

    public void PlayAudio(AudioSource source, string clipName){
        AudioClip clip = audios.Find(audio => audio.name.Equals(clipName));

        if (clip != null) { 
            if (source == null) { audioSource.PlayOneShot(clip); }
            else { source.PlayOneShot(clip); }
        }
    }

    public void PlayMusic(string clipName){
        AudioClip clip = audios.Find(audio => audio.name.Equals(clipName));

        if (clip != null) { musicSource.PlayOneShot(clip); }
    }
}
