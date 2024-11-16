using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    public GameObject menuCanvas;

    // Start is called before the first frame update



    void Start()
    {
        menuCanvas.SetActive(false); //So Menu isn't always on
    }

    // Update is called once per frame
    void Update()
    {

        if(Input.GetKeyDown(KeyCode.Tab)) {
            
            menuCanvas.SetActive(!menuCanvas.activeSelf); //What the Canvas currently isn't
        }

    }

}
