using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenu;
    public GameObject settingsMenu;
    public GameObject galleryMenu;
    public GameObject dialoguePopup;


    [Header("Levels")]
    public string newGameLevel;
    string level;

    void Start()
    {
        Dictionary<string, UnityEngine.Events.UnityAction> menuButtons = new Dictionary<string, UnityEngine.Events.UnityAction>()
        {
            {"NewGame", NewGameClicked},
            {"LoadGame", LoadGameClicked},
            {"Settings", SettingsClicked},
            {"Gallery", GalleryClicked},
            {"ExitGame", ExitGameClicked}
        };

        foreach (Transform child in mainMenu.transform)
        {
            Button button = child.GetComponent<Button>();
            if (menuButtons.ContainsKey(child.name))
            {
                button.onClick.AddListener(menuButtons[child.name]);
            }
        }

        // newGameYes.onClick.AddListener(newGameDialogYesClicked);
        // newGameNo.onClick.AddListener(newGameDialogNoClicked);

        // noSavesOkayButton.onClick.AddListener(CloseNoSavesDialogue);
    }

    void NewGameClicked()
    {
        GameObject newGamePopup = dialoguePopup.transform.Find("NewGamePopup").gameObject;
        mainMenu.SetActive(false);
        newGamePopup.SetActive(true);

        Button newGameYes = newGamePopup.transform.Find("Yes").GetComponent<Button>();
        Button newGameNo = newGamePopup.transform.Find("No").GetComponent<Button>();

        newGameYes.onClick.AddListener(newGameDialogYesClicked);
        newGameNo.onClick.AddListener(ReturnToMainMenu);
    }

    void newGameDialogYesClicked()
    {
        SceneManager.LoadScene(newGameLevel);
    }
    void ReturnToMainMenu()
    {
        GameObject newGamePopup = dialoguePopup.transform.Find("NewGamePopup").gameObject;
        mainMenu.SetActive(true);
        newGamePopup.SetActive(false);
        settingsMenu.SetActive(false);
    }

    void LoadGameClicked()
    {
        SceneManager.LoadScene(newGameLevel); // just use this for now
        // mainMenu.SetActive(false);
    }

    // void LoadGameSaveClicked()
    // {
    //     SceneManager.LoadScene(newGameLevel);
    // }

    void SettingsClicked()
    {
        mainMenu.SetActive(false);
        settingsMenu.SetActive(true);

        Button resetButton = settingsMenu.transform.Find("Reset").GetComponent<Button>();
        Button confirmButton = settingsMenu.transform.Find("Confirm").GetComponent<Button>();

        resetButton.onClick.AddListener(() => OpenSettingsPopup("reset")); // lambda expression, allows a parameter to pass through the function
        confirmButton.onClick.AddListener(() => OpenSettingsPopup("confirm"));
    }

    void OpenSettingsPopup(string action)
    {
        GameObject settingsPopup = dialoguePopup.transform.Find("SettingsPopup").gameObject;
        TextMeshProUGUI popupText = settingsPopup.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        popupText.text = $"Would you like to {action} all changes?";

        settingsPopup.SetActive(true);
        Button settingsYes = settingsPopup.transform.Find("Yes").GetComponent<Button>();
        Button settingsNo = settingsPopup.transform.Find("No").GetComponent<Button>();
        
        settingsYes.onClick.AddListener(() => SettingsChange(settingsPopup, action, "yes"));
        settingsNo.onClick.AddListener(() => SettingsChange(settingsPopup, action, "no"));
    }

    void SettingsChange(GameObject settingsPopup, string action, string choice)
    {
        if (action == "reset")
        {
            if (choice == "yes")
            {
                // reset values to original
            }
            settingsPopup.SetActive(false);
        }
        else
        {
            if (choice == "yes")
            {
                ReturnToMainMenu();
            }
            else
            {
                settingsPopup.SetActive(false);
            }
        }
    }

    void GalleryClicked()
    {
        mainMenu.SetActive(false);
        galleryMenu.SetActive(true);
    }

    void ExitGameClicked()
    {
        Application.Quit();
    }
}
