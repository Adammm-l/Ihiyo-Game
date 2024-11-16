using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabController : MonoBehaviour
{
    public Image[] tabImages;
    public GameObject[] pages;


    // Start is called before the first frame update
    void Start()
    {
        ActivateTab(0); //Start Menu at Same Page
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
