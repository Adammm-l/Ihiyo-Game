using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    // These are needed so that we can connect the slider and audio mixer to this script

    [SerializeField] private AudioMixer settingsMixer;
    [SerializeField] private Slider masterSlider;

    [SerializeField] private Slider musicSlider;

    [SerializeField] private Slider sfxSlider;


    // store the last saved music volume
    float savedMasterVolume;
    float savedMusicVolume;
    float savedSFXVolume;

    private void Start() {
        if (PlayerPrefs.HasKey("musicVolume")) { // Loads Volume when game starts
            LoadVolume(); 
        }

        else { // If there wasn't a saved volume, then set to default
            
            SetMusicVol();
            SetSFXVol();
            SetMasterVol();
        }

        // save initial slider values
        savedMasterVolume = masterSlider.value;
        savedMusicVolume = musicSlider.value;
        savedSFXVolume = sfxSlider.value;
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

    public void SetMasterVol()
    {
        float volume = masterSlider.value; // should probably set the right slider
        settingsMixer.SetFloat("master", Mathf.Log10(volume)*20);
        PlayerPrefs.SetFloat("masterVolume", volume);
    }

// Load up previous sound volume that was set
    public void LoadVolume() {
        masterSlider.value = PlayerPrefs.GetFloat("masterVolume");
        musicSlider.value = PlayerPrefs.GetFloat("musicVolume"); // Set initial value
        sfxSlider.value = PlayerPrefs.GetFloat("sfxVolume"); // Set initial value

        SetMusicVol(); // Calls to actually set value
        SetSFXVol();
        SetMasterVol();

    }

    public bool HasSliderChanged()
    {
        bool masterSliderChanged = !Mathf.Approximately(masterSlider.value, savedMasterVolume);
        bool musicSliderChanged = !Mathf.Approximately(musicSlider.value, savedMusicVolume);
        bool sfxSliderChanged = !Mathf.Approximately(sfxSlider.value, savedSFXVolume);
        return musicSliderChanged || sfxSliderChanged || masterSliderChanged;
    }

    public void SaveCurrentSliderValues()
    {
        savedMasterVolume = masterSlider.value;
        savedMusicVolume = musicSlider.value;
        savedSFXVolume = sfxSlider.value;
    }

    public void resetSliderValues()
    {
        masterSlider.value = 0.5f;
        musicSlider.value = 0.5f;
        sfxSlider.value = 0.5f;
    }

    public void RevertSliderChanges()
    {
        LoadVolume();
        
        SetMasterVol();
        SetMusicVol();
        SetSFXVol();
    }
}