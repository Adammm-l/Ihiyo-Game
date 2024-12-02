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
    TextMeshProUGUI fadeText;

    KeybindManager keybindManager;
    Coroutine currentKeybindCoroutine;


    [Header("Levels")]
    public string newGameLevel;
    string level;

    void Start()
    {
        fadeText = settingsMenu.transform.Find("FadeText").GetComponent<TextMeshProUGUI>();
        keybindManager = GetComponent<KeybindManager>();
        currentKeybindCoroutine = null;
        
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
                button.onClick.RemoveAllListeners();
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
        newGameYes.onClick.RemoveAllListeners();
        newGameNo.onClick.RemoveAllListeners();

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
        GameObject exitGamePopup = dialoguePopup.transform.Find("ExitGamePopup").gameObject;
        mainMenu.SetActive(true);
        
        exitGamePopup.SetActive(false);
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
        fadeText.text = "";

        GameObject settingsButtonHolder = settingsMenu.transform.Find("SettingPanel").gameObject;
        GameObject settingsOptions = settingsMenu.transform.Find("OptionPanel").gameObject;

        Button audioButton = settingsButtonHolder.transform.Find("AudioButton").GetComponent<Button>();
        Button viewButton = settingsButtonHolder.transform.Find("ViewButton").GetComponent<Button>();
        Button accessibilityButton = settingsButtonHolder.transform.Find("AccessibilityButton").GetComponent<Button>();

        Button resetButton = settingsOptions.transform.Find("Reset").GetComponent<Button>();
        Button exitButton = settingsOptions.transform.Find("Exit").GetComponent<Button>();
        Button saveButton = settingsOptions.transform.Find("Save").GetComponent<Button>();

        GameObject audioSettings = settingsMenu.transform.Find("AudioSettings").gameObject;
        GameObject viewSettings = settingsMenu.transform.Find("ViewSettings").gameObject;
        GameObject accessibilitySettings = settingsMenu.transform.Find("AccessibilitySettings").gameObject;

        Button[] settingsButtons = {audioButton, viewButton, accessibilityButton};

        InitializeKeybindButtons(viewSettings);

        // set default "panel" and button (will be audio since it's the first)
        SetActiveSettingTab(audioSettings.gameObject, audioButton, settingsButtons);

        // reassign listeners to settings buttons to avoid issues
        audioButton.onClick.RemoveAllListeners();
        viewButton.onClick.RemoveAllListeners();
        accessibilityButton.onClick.RemoveAllListeners();

        audioButton.onClick.AddListener(() => SetActiveSettingTab(audioSettings, audioButton, settingsButtons)); // lambda expression, allows a parameter to pass through the function
        viewButton.onClick.AddListener(() => SetActiveSettingTab(viewSettings, viewButton, settingsButtons));
        accessibilityButton.onClick.AddListener(() => SetActiveSettingTab(accessibilitySettings, accessibilityButton, settingsButtons));

        resetButton.onClick.RemoveAllListeners();
        exitButton.onClick.RemoveAllListeners();
        saveButton.onClick.RemoveAllListeners();

        resetButton.onClick.AddListener(() => OpenSettingsPopup("reset"));
        exitButton.onClick.AddListener(() => OpenSettingsPopup("exit"));
        saveButton.onClick.AddListener(() => OpenSettingsPopup("save"));
    }

    void InitializeKeybindButtons(GameObject viewSettings)
    {
        // find buttons in viewSettings object
        foreach (Transform child in viewSettings.transform)
        {
            Button button = child.GetComponentInChildren<Button>();

            // set button text based on keybind
            string keybind = GetButtonKeybind(button);
            string updatedKeyString;
            KeyCode key = keybindManager.GetKeybind(keybind);
            
            if (key == KeyCode.None)
            {
                updatedKeyString = "Not Bound";
            }
            else
            {
                updatedKeyString = key.ToString();
            }
            button.GetComponentInChildren<TextMeshProUGUI>().text = updatedKeyString;

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => RebindKey(keybind, button)); // apparently you can't call a coroutine from a lambda expression ???
            Debug.Log(button.GetComponentInChildren<TextMeshProUGUI>().text);
        }
    }

    void RebindKey(string action, Button keyButton)
    {
        // starts coroutine which waits for a key press to rebind action
        StartCoroutine(StartKeybindUpdate(action, keyButton));
    }

    IEnumerator StartKeybindUpdate(string action, Button keyButton)
    {
        // check if another button is pressed simultaneously
        if (IsOtherButtonPressed(keyButton))
        {
            Debug.Log("Another button was pressed.");
            Button pressedButton = GetOtherPressedButton(keyButton);
            CancelKeybindUpdate(pressedButton);

            // stop currentKeybind coroutine before starting it again
            if (currentKeybindCoroutine != null)
            {
                StopCoroutine(currentKeybindCoroutine);
            }
            currentKeybindCoroutine = StartCoroutine(WaitForKeyPress(action, keyButton));

            yield break;
        }
        currentKeybindCoroutine = StartCoroutine(WaitForKeyPress(action, keyButton));
    }

    IEnumerator WaitForKeyPress(string action, Button keyButton)
    {
        bool isKeySet = false;
        keyButton.interactable = false;

        UpdateButtonStyle(keyButton, keyButton.interactable);

        // waits until a key is pressed and assigned
        while (!isKeySet)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0)) // ignore key press if it's just the left mouse button
            {
                break;
            }
            foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
            {
                // checks if current key is pressed down and updates keybind
                if (Input.GetKeyDown(key))
                {
                    // deals with matching keybinds for different keys
                    if (keybindManager.IsKeybindConflict(action, key))
                    {
                        OpenConflictPopup(keyButton, key);
                        yield return null; // literally fixed everything breaking (inputs not properly registering)
                        break;
                    }

                    keybindManager.UpdateKeybind(action, key);
                    keyButton.GetComponentInChildren<TextMeshProUGUI>().text = key.ToString();
                    CancelKeybindUpdate(keyButton);

                    isKeySet = true;
                    break;
                }
            }

            yield return null; // waits for next frame before checking again
        }
    }

    bool IsOtherButtonPressed(Button currentButton)
    {
        GameObject viewSettings = settingsMenu.transform.Find("ViewSettings").gameObject;
        Button[] keybindButtons = viewSettings.GetComponentsInChildren<Button>();

        foreach (Button button in keybindButtons)
        {
            if (button == currentButton)
            {
                continue; // ignore actively pushed button
            }
            if (!button.interactable)
            {
                return true; // other button was pressed
            }
        }
        return false;
    }

    Button GetOtherPressedButton(Button currentButton)
    {
        GameObject viewSettings = settingsMenu.transform.Find("ViewSettings").gameObject;
        Button[] keybindButtons = viewSettings.GetComponentsInChildren<Button>();

        foreach (Button button in keybindButtons)
        {
            if (button == currentButton)
            {
                continue;
            }
            if (!button.interactable)
            {
                return button;
            }
        }
        return null;
    }

    void CancelKeybindUpdate(Button keyButton)
    {
        keyButton.interactable = true;
        Color defaultColor = Color.white;

        ResetButtonStyle(keyButton, Color.white);
    }

    void OpenConflictPopup(Button keyButton, KeyCode newKey)
    {
        GameObject conflictPopup = dialoguePopup.transform.Find("ConflictPopup").gameObject;
        conflictPopup.SetActive(true);

        Button conflictYes = conflictPopup.transform.Find("Yes").GetComponent<Button>();
        Button conflictNo = conflictPopup.transform.Find("No").GetComponent<Button>();

        conflictYes.onClick.RemoveAllListeners();
        conflictNo.onClick.RemoveAllListeners();

        conflictYes.onClick.AddListener(() => resolveConflict("yes", keyButton, newKey));
        conflictNo.onClick.AddListener(() => resolveConflict("no", keyButton, newKey));
    }

    void resolveConflict(string action, Button keyButton, KeyCode newKey)
    {
        CancelKeybindUpdate(keyButton);
        ReturnToSettingsPage();

        GameObject viewSettings = settingsMenu.transform.Find("ViewSettings").gameObject;
        Transform actionHolder = keyButton.transform.parent.GetChild(0);
        string buttonAction = actionHolder.name;
        
        if (action == "no")
        {
            return;
        }
        if (action == "yes")
        {
            keybindManager.UpdateKeybind(buttonAction, newKey);
            RemoveDuplicateKeys(keyButton, newKey);
            InitializeKeybindButtons(viewSettings);
        }
    }

    void RemoveDuplicateKeys(Button keyButton, KeyCode originalKey)
    {
        Dictionary<string, KeyCode> keybinds = keybindManager.GetTempKeybinds();
        List<string> actionsToUnbind = new List<string>();

        foreach (var keybind in keybinds)
        {
            string action = keybind.Key; // i can't tell you how confusing it is to have keys not represents keys
            KeyCode key = keybind.Value;

            // add action to list if keys match (duplicate)
            if (key == originalKey)
            {
                actionsToUnbind.Add(action);
            }
        }

        // remove all keys aside from original key
        foreach (string action in actionsToUnbind)
        {
            string originalAction = GetButtonKeybind(keyButton);
            if (action != originalAction)
            {
                keybindManager.UpdateKeybind(action, KeyCode.None);
                Debug.Log($"Duplicate key '{originalKey}' removed from action '{action}'.");
            }
        }
    }

    void ResetButtonStyle(Button button, Color defaultColor)
    {
        // reset modified colors to default
        ColorBlock colors = button.colors;
        colors.normalColor = defaultColor;
        
        button.colors = colors;
        button.interactable = true;
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

            case "GameMenuButton":
                return "GameMenu";
            
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
        
        GameObject viewSettings = settingsMenu.transform.Find("ViewSettings").gameObject;
        Button[] keybindButtons = viewSettings.GetComponentsInChildren<Button>();
        foreach (Button button in keybindButtons)
        {
            CancelKeybindUpdate(button);
        }

        // activate the selected panel
        selectedPanel.SetActive(true);

        // update button states
        UpdateButtonStates(activeButton, buttons);
    }

    IEnumerator FadeMessage(string message)
    {
        fadeText.text = message;
        fadeText.alpha = 1f; // still don't get why floats need fs on them

        float duration = 2f;
        while (fadeText.alpha > 0)
        {
            fadeText.alpha -= Time.deltaTime / duration;
            yield return null;
        }
    }

    void TriggerFadeMessage(string message)
    {
        StopAllCoroutines();
        StartCoroutine(FadeMessage(message));
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
        Color defaultColor = Color.white;

        if (isActive)
        {
            colors.normalColor = defaultColor; // color for active button
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

        // creates pop up text depending on which button was clicked
        if (action == "reset")
        {
            popupText.text = "Would you like to restore all settings to default?";
            settingsPopup.SetActive(true);
        }
        
        if (action == "exit")
        {
            if (keybindManager.HasKeybindChanged())
            {
                // prompt user to save keybinds if at least one keybind has changed
                popupText.text = "You have unsaved changes. Are you sure you want to exit?";
                settingsPopup.SetActive(true);
            }
            else if (keybindManager.HasNewNullKeybinds())
            {
                popupText.text = "You have at least one unbound keybind. Are you sure you want to exit?";
                settingsPopup.SetActive(true);
            }

            // also note to implement conflict stuff
            // if there's no conflict or unsaved changes, just return to the main menu
            else
            {
                ReturnToMainMenu();
            }
        }

        if (action == "save")
        {
            if (keybindManager.HasKeybindChanged())
            {
                popupText.text = "Would you like to save all changes?";
                settingsPopup.SetActive(true);
            }

            // return to settings menu if there are no changes
            else
            {
                GameObject noChangesPopup = dialoguePopup.transform.Find("NoChangesPopup").gameObject;
                noChangesPopup.SetActive(true);

                Button settingsOkay = noChangesPopup.transform.Find("Okay").GetComponent<Button>();
                settingsOkay.onClick.RemoveAllListeners();
                settingsOkay.onClick.AddListener(ReturnToSettingsPage);
                return;
            }
        }

        Button settingsYes = settingsPopup.transform.Find("Yes").GetComponent<Button>();
        Button settingsNo = settingsPopup.transform.Find("No").GetComponent<Button>();
        
        // fixes issue with buttons not correctly responding to user input
        settingsYes.onClick.RemoveAllListeners();
        settingsNo.onClick.RemoveAllListeners();

        // assign listeners to yes/no buttons
        settingsYes.onClick.AddListener(() => SettingsChange(action, "yes"));
        settingsNo.onClick.AddListener(() => SettingsChange(action, "no"));
    }

    void ReturnToSettingsPage()
    {
        GameObject noChangesPopup = dialoguePopup.transform.Find("NoChangesPopup").gameObject;
        GameObject settingsPopup = dialoguePopup.transform.Find("SettingsPopup").gameObject;
        GameObject conflictPopup = dialoguePopup.transform.Find("ConflictPopup").gameObject;

        settingsPopup.SetActive(false);
        noChangesPopup.SetActive(false);
        conflictPopup.SetActive(false);
    }

    void SettingsChange(string action, string choice)
    {
        // functionality for buttons in settings pop up
        Debug.Log($"{action}, {choice}");
        ReturnToSettingsPage();

        // different cases for settings button presses
        switch (action)
        {
            case "reset":
                if (choice == "yes")
                {
                    keybindManager.ResetKeybinds();
                    RefreshKeybindButtons(); // visually resets the keybinds
                    TriggerFadeMessage("Settings have been reset to default.");
                    Debug.Log("Settings have been reset to default.");
                }
                break;

            case "exit":
                if (choice == "yes")
                {
                    // exits to main menu discarding unsaved changes
                    if (keybindManager.HasKeybindChanged())
                    {
                        keybindManager.DiscardKeybindChanges();
                    }

                    // otherwise just returns to menu
                    ReturnToMainMenu();
                    Debug.Log("Changes have been reverted, exiting to main menu.");
                }
                break;

            case "save":
                if (choice == "yes")
                {
                    // saves changes to keybinds without exiting
                    keybindManager.ConfirmKeybinds();
                    RefreshKeybindButtons();
                    TriggerFadeMessage("Changes have been saved.");
                    Debug.Log("Changes have been saved.");
                }
                break;
        }
    }

    void RefreshKeybindButtons()
    {
        Transform viewSettings = settingsMenu.transform.Find("ViewSettings");
        foreach (Transform child in viewSettings)
        {
            Button button = child.GetComponentInChildren<Button>();
            string keybind = GetButtonKeybind(button);
            string updatedKeyString;
            KeyCode key = keybindManager.GetKeybind(keybind);
            
            if (key == KeyCode.None)
            {
                updatedKeyString = "Not Bound";
            }
            else
            {
                updatedKeyString = key.ToString();
            }
            button.GetComponentInChildren<TextMeshProUGUI>().text = updatedKeyString;
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

        returnToMain.onClick.RemoveAllListeners();
        returnToMain.onClick.AddListener(ReturnToMainMenu);
    }

    void ExitGameClicked()
    {
        // opens popup prompting user to exit game
        GameObject exitGamePopup = dialoguePopup.transform.Find("ExitGamePopup").gameObject;
        exitGamePopup.SetActive(true);

        Button exitGameYes = exitGamePopup.transform.Find("Yes").GetComponent<Button>();
        Button exitGameNo = exitGamePopup.transform.Find("No").GetComponent<Button>();

        exitGameYes.onClick.RemoveAllListeners();
        exitGameNo.onClick.RemoveAllListeners();

        exitGameYes.onClick.AddListener(ExitGame);
        exitGameNo.onClick.AddListener(ReturnToMainMenu);
    }

    void ExitGame()
    {
        Application.Quit(); // closes game
    }
}