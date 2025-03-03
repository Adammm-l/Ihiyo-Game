using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// Eri (Edwin)
public class TabController : MonoBehaviour
{
    public Image[] tabImages;
    public GameObject[] pages;
    private static bool tabExists; // All instances of this Player references the exact same variable

    // Start is called before the first frame update
    void Start()
    {
        ActivateTab(0); //Start Menu at Same Page
        if(!tabExists) { // If the player doesn't exist, then mark them as Don't Destroy on Load, handling duplicates

            tabExists = true;
            DontDestroyOnLoad(transform.gameObject);
        }

        else { // Eliminate Duplicate Objects
            Destroy(gameObject);
        }
    }


    public void ActivateTab(int tabNo) {
        for(int i = 0; i < pages.Length; i++) { //Gray out inactive tabs
            pages[i].SetActive(false);
            tabImages[i].color = Color.grey;

        }

//Highlights Current Tab
        pages[tabNo].SetActive(true);

        tabImages[tabNo].color = Color.white;
    }
}
