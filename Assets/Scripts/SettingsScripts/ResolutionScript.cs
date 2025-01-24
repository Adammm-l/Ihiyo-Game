using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// Eri (Edwin)
public class ResolutionScript : MonoBehaviour
{

    Resolution[] resolutions; // Storing list of screen resolutions for game

    public TMPro.TMP_Dropdown resolutionDropdown; // Reference our UI dropdown menu
    
    // Start is called before the first frame update
    void Start()
    {
        
        resolutions = Screen.resolutions;

        resolutionDropdown.ClearOptions(); // Clean the dropdown at first

        // Have to convert this into String since AddOptions() only accpets string params

        List<string> resOptions = new List<string>();

        int currentIndex  = 0; // Current Res

        for (int i = 0; i < resolutions.Length; i++) {

            string option = resolutions[i].width + " x " + resolutions[i].height; // Converting the given resolutions into a list of strings

            resOptions.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height) { // Verify Resolution

                currentIndex = i;

            }
        }

        // Now we need to add our resolutions options to the dropdown menu and display the current selected resolution

        resolutionDropdown.AddOptions(resOptions);

        resolutionDropdown.value = currentIndex;

        resolutionDropdown.RefreshShownValue();

    }

    public void setRes(int resIndex) { // Function to actually set and change the game resolution

        Resolution resolution = resolutions[resIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);

    }

}
