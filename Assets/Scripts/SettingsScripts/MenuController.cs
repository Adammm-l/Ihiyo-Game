using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Eri (Edwin) + Adam
public class MenuController : MonoBehaviour
{
    [Header("References")]
    public GameObject keybindHolder;
    public GameObject menuCanvas;
    KeybindManager keybindManager;
    KeyCode gameMenuKey;
    private static bool menuExists; // All instances of this Player references the exact same variable
    private UIManager uiManager;

    // Start is called before the first frame update



    void Start()
    {
        keybindManager = keybindHolder.GetComponent<KeybindManager>();
        uiManager = FindObjectOfType<UIManager>();
        menuCanvas.SetActive(false); //So Menu isn't always on
        if(!menuExists) { // If the player doesn't exist, then mark them as Don't Destroy on Load, handling duplicates

            menuExists = true;
            DontDestroyOnLoad(transform.gameObject);
        }

        else { // Eliminate Duplicate Objects
            Destroy(gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {
        gameMenuKey = keybindManager.GetKeybind("GameMenu");
        if (Input.GetKeyDown(gameMenuKey))
        {
            bool willBeActive = !menuCanvas.activeSelf;

            if (willBeActive && uiManager != null)
            {
                uiManager.SetActivePanel(menuCanvas);
            }

            menuCanvas.SetActive(willBeActive);
        }
    }

}
