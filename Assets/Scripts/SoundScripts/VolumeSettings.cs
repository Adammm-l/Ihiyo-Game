using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    // These are needed so that we can connect the slider and audio mixer to this script

    [SerializeField] private AudioMixer settingsMixer;

    [SerializeField] private Slider musicSlider;

    [SerializeField] private Slider sfxSlider;

    private void Start() {
        if (PlayerPrefs.HasKey("musicVolume")) { // Loads Volume when game starts
            LoadVolume(); 
        }

        else { // If there wasn't a saved volume, then set to default
            
            SetMusicVol();
            SetSFXVol();

        }
    }

// Connecting the Slider to the Audio Mixer, letting us change volume

    public void SetMusicVol() {

        float volume = musicSlider.value;
        settingsMixer.SetFloat("music", Mathf.Log10(volume)*20); // Needed since the values of Audio Mixing progress in log and not linearly
        PlayerPrefs.SetFloat("musicVolume", volume); // Save the music level setting
    }

// Same behavior but for SFX
    public void SetSFXVol() {

        float volume = sfxSlider.value;
        settingsMixer.SetFloat("sfx", Mathf.Log10(volume)*20); // Needed since the values of Audio Mixing progress in log and not linearly
        PlayerPrefs.SetFloat("sfxVolume", volume); // Save the music level setting
    }

// Load up previous sound volume that was set
    private void LoadVolume() {
        
        musicSlider.value = PlayerPrefs.GetFloat("musicVolume"); // Set initial value
        sfxSlider.value = PlayerPrefs.GetFloat("sfxVolume"); // Set initial value

        SetMusicVol(); // Calls to actually set value
        SetSFXVol();

    }

}