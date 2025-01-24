using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Eri (Edwin)
public class MenuController : MonoBehaviour
{
    [Header("References")]
    public GameObject keybindHolder;
    public GameObject menuCanvas;
    KeybindManager keybindManager;
    KeyCode gameMenuKey;

    // Start is called before the first frame update



    void Start()
    {
        keybindManager = keybindHolder.GetComponent<KeybindManager>();
        menuCanvas.SetActive(false); //So Menu isn't always on
    }

    // Update is called once per frame
    void Update()
    {
        gameMenuKey = keybindManager.GetKeybind("GameMenu");
        if(Input.GetKeyDown(gameMenuKey)) {
            
            menuCanvas.SetActive(!menuCanvas.activeSelf); //What the Canvas currently isn't
        }

    }

}
