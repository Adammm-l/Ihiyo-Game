using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("---------- Audio Source ----------")] //Label for Clarity
    [SerializeField] AudioSource musicSource;
    
    [SerializeField] AudioSource SFXSource;

    [Header("---------- Audio Source ----------")] //Label for Clarity

    public AudioClip bg;
    public AudioClip footSteps;

    public AudioClip door;



    private void Start() { //We want the Background Music to immediately play when the scene loads in
        musicSource.clip = bg;
        musicSource.Play();
    }


    public void SFXPlay(AudioClip clip) { //Functionality for SFX

        SFXSource.PlayOneShot(clip);

    }

    public bool isSFXPlay(AudioClip audio) {

        return SFXSource.isPlaying && SFXSource.clip == audio;

    }



}