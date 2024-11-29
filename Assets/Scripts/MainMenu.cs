using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;
using System;

public class MainMenuController : MonoBehaviour
{
    [Header("UI References")]
    public GameObject mainMenu;
    public GameObject settingsMenu;
    public GameObject galleryMenu;
    public GameObject dialoguePopup;

    KeybindManager keybindManager;


    [Header("Levels")]
    public string newGameLevel;
    string level;

    void Start()
    {
        keybindManager = GetComponent<KeybindManager>();
        
        // allows user to click on each of the buttons listed and gives them functionality to their corresponding functions
        Dictionary<string, UnityEngine.Events.UnityAction> menuButtons = new Dictionary<string, UnityEngine.Events.UnityAction>()
        {
            {"NewGame", NewGameClicked},
            {"LoadGame", LoadGameClicked},
            {"Settings", SettingsClicked},
            {"Gallery", GalleryClicked},
            {"ExitGame", ExitGameClicked}
        };

        // assign listeners to buttons (what gives each button functionality)
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
        // activates the new game popup, and assigns functionality to the yes/no buttons
        GameObject newGamePopup = dialoguePopup.transform.Find("NewGamePopup").gameObject;
        mainMenu.SetActive(false);
        newGamePopup.SetActive(true);

        Button newGameYes = newGamePopup.transform.Find("Yes").GetComponent<Button>();
        Button newGameNo = newGamePopup.transform.Find("No").GetComponent<Button>();

        // assign listeners to yes and no buttons
        newGameYes.onClick.AddListener(newGameDialogYesClicked);
        newGameNo.onClick.AddListener(ReturnToMainMenu);
    }

    void newGameDialogYesClicked()
    {
        SceneManager.LoadScene(newGameLevel);
    }
    void ReturnToMainMenu()
    {
        // hides the active popup/menu and activates the main menu again
        GameObject newGamePopup = dialoguePopup.transform.Find("NewGamePopup").gameObject;
        mainMenu.SetActive(true);
        newGamePopup.SetActive(false);
        settingsMenu.SetActive(false);
        galleryMenu.SetActive(false);
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
        // hides the main menu and activates the settings section
        mainMenu.SetActive(false);
        settingsMenu.SetActive(true);

        GameObject settingsButtonHolder = settingsMenu.transform.Find("SettingPanel").gameObject;
        GameObject settingsOptions = settingsMenu.transform.Find("OptionPanel").gameObject;

        Button audioButton = settingsButtonHolder.transform.Find("AudioButton").GetComponent<Button>();
        Button viewButton = settingsButtonHolder.transform.Find("ViewButton").GetComponent<Button>();
        Button accessibilityButton = settingsButtonHolder.transform.Find("AccessibilityButton").GetComponent<Button>();

        Button resetButton = settingsOptions.transform.Find("Reset").GetComponent<Button>();
        Button confirmButton = settingsOptions.transform.Find("Confirm").GetComponent<Button>();

        GameObject audioSettings = settingsMenu.transform.Find("AudioSettings").gameObject;
        GameObject viewSettings = settingsMenu.transform.Find("ViewSettings").gameObject;
        GameObject accessibilitySettings = settingsMenu.transform.Find("AccessibilitySettings").gameObject;

        Button[] settingsButtons = {audioButton, viewButton, accessibilityButton};

        InitializeKeybindButtons(viewSettings);

        // set default "panel" and button (will be audio since it's the first)
        SetActiveSettingTab(audioSettings.gameObject, audioButton, settingsButtons);

        // assign listeners to settings buttons
        audioButton.onClick.AddListener(() => SetActiveSettingTab(audioSettings, audioButton, settingsButtons)); // lambda expression, allows a parameter to pass through the function
        viewButton.onClick.AddListener(() => SetActiveSettingTab(viewSettings, viewButton, settingsButtons));
        accessibilityButton.onClick.AddListener(() => SetActiveSettingTab(accessibilitySettings, accessibilityButton, settingsButtons));

        resetButton.onClick.AddListener(() => OpenSettingsPopup("reset"));
        confirmButton.onClick.AddListener(() => OpenSettingsPopup("confirm"));
    }

    void InitializeKeybindButtons(GameObject viewSettings)
    {
        // find buttons in viewSettings object
        foreach (Transform child in viewSettings.transform)
        {
            Button button = child.GetComponentInChildren<Button>();

            // set button text based on keybind
            string keybind = GetButtonKeybind(button);
            button.GetComponentInChildren<TextMeshProUGUI>().text = keybindManager.GetKeybind(keybind).ToString();

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => RebindKey(keybind, button)); // apparently you can't call a coroutine from a lambda expression ???
            Debug.Log(button.GetComponentInChildren<TextMeshProUGUI>().text);
        }
    }

    void RebindKey(string action, Button keyButton)
    {
        // starts coroutine which waits for a key press to rebind action
        StartCoroutine(WaitForKeyPress(action, keyButton));
    }

    IEnumerator WaitForKeyPress(string action, Button keyButton)
    {
        bool isKeySet = false;

        // waits until a key is pressed and assigned
        while (!isKeySet)
        {
            foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
            {
                // checks if current key is pressed down and updates keybind
                if (Input.GetKeyDown(key))
                {
                    keybindManager.UpdateKeybind(action, key);
                    keyButton.GetComponentInChildren<TextMeshProUGUI>().text = key.ToString();

                    isKeySet = true;
                    break;
                }
            }

            // waits for next frame before checking again
            yield return null;
        }
    }


    string GetButtonKeybind(Button button)
    {
        // match the button's name to the corresponding keybind
        switch (button.name)
        {
            case "UpButton":
                return "MoveUp";
                
            case "LeftButton":
                return "MoveLeft";

            case "DownButton":
                return "MoveDown";

            case "RightButton":
                return "MoveRight";

            case "InteractButton":
                return "Interact";

            case "QuestLogButton":
                return "QuestLog";

            case "InventoryButton":
                return "Inventory";

            default:
                return null;
        }
    }

    void SetActiveSettingTab(GameObject selectedPanel, Button activeButton, Button[] buttons)
    {
        // deactivate all settings panels
        settingsMenu.transform.Find("AudioSettings").gameObject.SetActive(false);
        settingsMenu.transform.Find("ViewSettings").gameObject.SetActive(false);
        settingsMenu.transform.Find("AccessibilitySettings").gameObject.SetActive(false);

        // activate the selected panel
        selectedPanel.SetActive(true);

        // update button states
        UpdateButtonStates(activeButton, buttons);
    }

    void UpdateButtonStates(Button activeButton, Button[] buttons)
    {
        // update each of the button colors
        foreach (Button button in buttons)
        {
            ColorBlock colors = button.colors;

            // active button appears pressed and is disabled, other inactive buttons use different color
            if (button == activeButton)
            {
                button.interactable = false;
            }
            else
            {
                button.interactable = true;
            }
            UpdateButtonStyle(button, button.interactable);

        }
    }
    
    void UpdateButtonStyle(Button button, bool isActive)
    {
        // updates button style depending on state of the button
        ColorBlock colors = button.colors;
        if (isActive)
        {
            colors.normalColor = colors.pressedColor; // color for active button
        }
        else
        {
            colors.normalColor = colors.disabledColor; // color for inactive button
        }
        button.colors = colors;
    }

    void OpenSettingsPopup(string action)
    {
        // activates the settings pop up to reset/confirm changes made and continues based off user response
        GameObject settingsPopup = dialoguePopup.transform.Find("SettingsPopup").gameObject;
        TextMeshProUGUI popupText = settingsPopup.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        popupText.text = $"Would you like to {action} all changes?";

        settingsPopup.SetActive(true);
        Button settingsYes = settingsPopup.transform.Find("Yes").GetComponent<Button>();
        Button settingsNo = settingsPopup.transform.Find("No").GetComponent<Button>();
        
        // fixes issue with buttons not correctly responding to user input
        settingsYes.onClick.RemoveAllListeners();
        settingsNo.onClick.RemoveAllListeners();

        // assign listeners to yes/no buttons
        settingsYes.onClick.AddListener(() => SettingsChange(settingsPopup, action, "yes"));
        settingsNo.onClick.AddListener(() => SettingsChange(settingsPopup, action, "no"));
    }

    void SettingsChange(GameObject settingsPopup, string action, string choice)
    {
        // functionality for reset and confirm buttons for settings pop up
        Debug.Log($"{action}, {choice}");
        settingsPopup.SetActive(false);
        if (action == "reset" && choice == "yes")
        {
            // reset values to original
            keybindManager.ResetKeybinds();

            // implement visual change to keybinds before exit

            return;
        }
        if (action == "confirm" && choice == "yes")
        {
            ReturnToMainMenu();
            keybindManager.ConfirmKeybinds();
        }
    }

    void GalleryClicked()
    {
        // hides the main menu and activates the gallery
        mainMenu.SetActive(false);
        galleryMenu.SetActive(true);

        Button galleryBack = galleryMenu.transform.Find("Back").GetComponent<Button>();
        Button galleryNext = galleryMenu.transform.Find("Next").GetComponent<Button>();
        Button returnToMain = galleryMenu.transform.Find("MainMenu").GetComponent<Button>();

        // assign listeners to gallery buttons
        // galleryBack.onClick.AddListener();
        // galleryNext.onClick.AddListener();
        returnToMain.onClick.AddListener(ReturnToMainMenu);
    }

    void ExitGameClicked() // closes game
    {
        Application.Quit();
    }
}