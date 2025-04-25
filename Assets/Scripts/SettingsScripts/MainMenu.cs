using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine;
using TMPro;
using System;
using UnityEditor;

// can't navigate to menu on save/load screens if not on center (ui fix) - can't get this to happen anymore so i think it's fixed?
// fix video settings

public class MainMenuController : MonoBehaviour // Terrence Akinola
{
    [Header("UI References")]
    public GameObject mainMenu;
    public GameObject savesMenu;
    public GameObject settingsMenu;
    public GameObject galleryMenu;
    public GameObject dialoguePopup;
    
    GameObject lastSelectedButton;
    VolumeSettings volumeSettings;
    EventSystem eventSystem;
    TextMeshProUGUI menuTextLabel;

    KeybindManager keybindManager;
    SaveController saveManager;
    Coroutine currentKeybindCoroutine;
    Button[] modifiableButtons;
    
    List<Selectable> disabledSelectables = new List<Selectable>();
    Dictionary<Selectable, ColorBlock> originalSelectableColors = new Dictionary<Selectable, ColorBlock>();


    bool cursorWasVisible;
    Vector3 lastMousePosition;
    Vector2 offScreenPosition = new Vector2(-1000, -1000);

    void Start()
    {
        eventSystem = EventSystem.current;
        cursorWasVisible = Cursor.visible;
        lastMousePosition = Input.mousePosition;
        
        menuTextLabel = dialoguePopup.transform.Find("MenuText").GetComponent<TextMeshProUGUI>();
        keybindManager = KeybindManager.Instance;
        saveManager = SaveController.Instance;
        volumeSettings = GetComponent<VolumeSettings>();
        currentKeybindCoroutine = null;

        lastSelectedButton = mainMenu.transform.GetChild(1).gameObject;
        GameObject cover = mainMenu.transform.parent.gameObject;
        if (cover != null)
        {
            Button[] buttons = cover.GetComponentsInChildren<Button>(true);
            Dictionary<Button, Color> originalDisabledColors = new Dictionary<Button, Color>();
            ColorBlock buttonColors = new ColorBlock
            {
                normalColor = new Color(0.75f, 0.75f, 0.75f),
                highlightedColor = Color.white,
                pressedColor = new Color(0.5f, 0.5f, 0.5f),
                selectedColor = Color.white,
                disabledColor = new Color(0.45f, 0.45f, 0.45f),
                colorMultiplier = 1f,
                fadeDuration = 0.1f
            };

            // apply colors to all buttons
            foreach (Button button in buttons)
            {
                button.colors = buttonColors;
                originalDisabledColors[button] = button.colors.disabledColor;
                AddButtonEventTriggers(buttons, button, originalDisabledColors);
                AddHoverEventTriggers(button);
            }

            Slider[] sliders = settingsMenu.GetComponentsInChildren<Slider>(true);
            foreach (Slider slider in sliders)
            {
                AddHoverEventTriggers(slider);
            }
        }

        // assign listeners to buttons (what gives each button functionality)
        Dictionary<string, UnityEngine.Events.UnityAction> menuButtons = new Dictionary<string, UnityEngine.Events.UnityAction>()
        {
            {"NewGame", NewGameClicked},
            {"LoadGame", LoadGameClicked},
            {"Settings", SettingsClicked},
            {"Gallery", GalleryClicked},
            {"ExitGame", ExitGameClicked}
        };

        foreach (Transform option in mainMenu.transform)
        {
            Button button = option.GetComponent<Button>();
            if (menuButtons.ContainsKey(option.name))
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(menuButtons[option.name]);
            }
        }
    }

    void Update()
    {
        HandleKeyboardNavigation();
        
        // Track mouse movement
        if (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
        {
            // Mouse moved - make cursor visible and reset position
            Cursor.visible = true;
            lastMousePosition = Input.mousePosition;
        }
        
        // Hide cursor and move off-screen when using keyboard
        if (eventSystem.currentSelectedGameObject != null && 
            (Input.GetKeyDown(KeyCode.UpArrow) || 
            Input.GetKeyDown(KeyCode.DownArrow) || 
            Input.GetKeyDown(KeyCode.LeftArrow) || 
            Input.GetKeyDown(KeyCode.RightArrow) ||
            Input.GetKeyDown(KeyCode.Tab) ||
            Input.GetKeyDown(KeyCode.Return)))
        {
            cursorWasVisible = Cursor.visible;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Confined;
            Mouse.current.WarpCursorPosition(offScreenPosition);
        }
        
        // Restore cursor when mouse moves
        if (Cursor.visible == false && 
            (Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0))
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Mouse.current.WarpCursorPosition(lastMousePosition);
        }

        // Existing selection tracking
        if (eventSystem.currentSelectedGameObject != null && 
            eventSystem.currentSelectedGameObject != lastSelectedButton)
        {
            lastSelectedButton = eventSystem.currentSelectedGameObject;
        }

        // Handle case when no UI element is selected
        if (eventSystem.currentSelectedGameObject == null && Input.GetMouseButtonDown(0))
        {
            if (lastSelectedButton != null)
            {
                eventSystem.SetSelectedGameObject(lastSelectedButton);
            }
            else
            {
                SelectFirstButton();
            }
        }
    }

   void AddHoverEventTriggers(Selectable selectable)
    {
        EventTrigger eventTrigger = selectable.gameObject.GetComponent<EventTrigger>();
        if (eventTrigger == null)
        {
            eventTrigger = selectable.gameObject.AddComponent<EventTrigger>();
        }

        // create PointerEnter event for mouse hover
        EventTrigger.Entry pointerEnterEntry = new EventTrigger.Entry();
        pointerEnterEntry.eventID = EventTriggerType.PointerEnter;
        pointerEnterEntry.callback.AddListener((data) => OnButtonHover(selectable));
        eventTrigger.triggers.Add(pointerEnterEntry);
    }

    void OnButtonHover(Selectable hoveredSelectable)
    {
        // set the hovered selectable (changed from button)
        if (hoveredSelectable.interactable)
        {
            eventSystem.SetSelectedGameObject(hoveredSelectable.gameObject);
            lastSelectedButton = hoveredSelectable.gameObject;
        }
    }

    void DisableButtonsOutsidePopup(GameObject popup)
    {
        disabledSelectables.Clear();
        originalSelectableColors.Clear();
        
        // get all selectable elements in scene
        Selectable[] allSelectables = FindObjectsOfType<Selectable>();
        
        foreach (Selectable selectable in allSelectables)
        {
            if (!selectable.interactable)
            {
                continue;
            }
            
            if (selectable.transform.IsChildOf(popup.transform))
            {
                continue;
            }
                
            disabledSelectables.Add(selectable);
            originalSelectableColors.Add(selectable, selectable.colors);
            
            // make selectable appear enabled but disable it
            ColorBlock colors = selectable.colors;
            Color enabledColor = colors.normalColor;
            
            colors.disabledColor = enabledColor;
            selectable.colors = colors;

            selectable.interactable = false;
        }
    }

    void RestoreDisabledButtons()
    {
        foreach (Selectable selectable in disabledSelectables)
        {
            if (selectable != null)
            {
                // restore original state and colors
                selectable.interactable = true;
                if (originalSelectableColors.ContainsKey(selectable))
                {
                    selectable.colors = originalSelectableColors[selectable];
                }
            }
        }

        disabledSelectables.Clear();
        originalSelectableColors.Clear();
    }

    void HandleKeyboardNavigation()
    {
        bool arrowKeyIsPressed = Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow);

        if (arrowKeyIsPressed)
        {
            List<string> buttonNames = new List<string>() {null, "AudioButton", "VideoButton", "ControlsButton"};
            if (buttonNames.Contains(eventSystem.currentSelectedGameObject.name)) // if focus on menu button, switch to first button on tab after keyboard press
            {
                SelectFirstButton();
            }
        }
        
        bool isRebinding = false;
        if (settingsMenu.activeSelf)
        {
            GameObject audioSettings = settingsMenu.transform.Find("AudioSettings").gameObject;
            GameObject videoSettings = settingsMenu.transform.Find("VideoSettings").gameObject;
            GameObject controlsSettings = settingsMenu.transform.Find("ControlsSettings").gameObject;

            foreach (Transform keybindObject in controlsSettings.transform)
            {
                Button button = keybindObject.GetComponentInChildren<Button>();
                if (button != null && !button.interactable)
                {
                    isRebinding = true;
                    break;
                }
            }

            if (!isRebinding && Input.GetKeyDown(KeyCode.Tab))
            {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) // shift + tab: move to the previous tab
                {
                    SwitchToPreviousTab(audioSettings, videoSettings, controlsSettings);
                }
                else // tab: move to the next tab
                {
                    SwitchToNextTab(audioSettings, videoSettings, controlsSettings);
                }
            }
        }

        if (!isRebinding && Input.GetKeyDown(KeyCode.Escape)) // go back to the previous menu
        {
            GoBack();
        }
    }

    void SwitchToNextTab(GameObject audioSettings, GameObject videoSettings, GameObject controlsSettings)
    {
        
        GameObject settingsButtonHolder = settingsMenu.transform.Find("SettingPanel").gameObject;
        Button audioButton = settingsButtonHolder.transform.Find("AudioButton").GetComponent<Button>();
        Button videoButton = settingsButtonHolder.transform.Find("VideoButton").GetComponent<Button>();
        Button controlsButton = settingsButtonHolder.transform.Find("ControlsButton").GetComponent<Button>();

        GameObject activeTab = GetActiveTab(audioSettings, videoSettings, controlsSettings);
        Button[] settingsButtons = {audioButton, videoButton, controlsButton};
        
        if (activeTab == audioSettings)
        {
            SetActiveSettingTab(videoSettings, videoButton, settingsButtons);
        }
        else if (activeTab == videoSettings)
        {
            SetActiveSettingTab(controlsSettings, controlsButton, settingsButtons);
        }
        else if (activeTab == controlsSettings)
        {
            SetActiveSettingTab(audioSettings, audioButton, settingsButtons);
        }
    }

    void SwitchToPreviousTab(GameObject audioSettings, GameObject videoSettings, GameObject controlsSettings)
    {
        GameObject settingsButtonHolder = settingsMenu.transform.Find("SettingPanel").gameObject;
        Button audioButton = settingsButtonHolder.transform.Find("AudioButton").GetComponent<Button>();
        Button videoButton = settingsButtonHolder.transform.Find("VideoButton").GetComponent<Button>();
        Button controlsButton = settingsButtonHolder.transform.Find("ControlsButton").GetComponent<Button>();

        Button[] settingsButtons = {audioButton, videoButton, controlsButton};
        GameObject activeTab = GetActiveTab(audioSettings, videoSettings, controlsSettings); // get currently active tab

        // determine previous tab based on the current tab
        if (activeTab == audioSettings)
        {
            SetActiveSettingTab(controlsSettings, controlsButton, settingsButtons);
        }
        else if (activeTab == videoSettings)
        {
            SetActiveSettingTab(audioSettings, audioButton, settingsButtons);
        }
        else if (activeTab == controlsSettings)
        {
            SetActiveSettingTab(videoSettings, videoButton, settingsButtons);
        }
    }

    GameObject GetActiveTab(GameObject audioSettings, GameObject videoSettings, GameObject controlsSettings)
    {
        if (audioSettings.activeSelf)
        {
            return audioSettings;
        }
        else if (videoSettings.activeSelf)
        {
            return videoSettings;
        }
        else if (controlsSettings.activeSelf)
        {
            return controlsSettings;
        }

        return null; // couldn't find active tab
    }

    void SelectFirstButton()
    {
        GameObject firstButton = null;

        if (mainMenu.activeSelf)
        {
            firstButton = mainMenu.transform.GetChild(1).gameObject;
        }
        else if (savesMenu.activeSelf)
        {
            firstButton = SelectFirstEnabledSaveButton();
        }
        else if (settingsMenu.activeSelf)
        {
            if (settingsMenu.transform.GetChild(2).gameObject.activeSelf) // audio tab
            {
                firstButton = settingsMenu.transform.Find("AudioSettings/MasterVolume/MasterSlider").gameObject;
            }
            if (settingsMenu.transform.GetChild(4).gameObject.activeSelf) // controls tab
            {
                firstButton = settingsMenu.transform.Find("ControlsSettings/UpKeybind/UpButton").gameObject;
            }
        }
        else if (galleryMenu.activeSelf)
        {
            firstButton = galleryMenu.transform.Find("ReturnToMain").gameObject;
        }
        else
        {
            SelectDialogueButton();
            return;
        }
    
        eventSystem.SetSelectedGameObject(firstButton);
        lastSelectedButton = firstButton; // update the last selected button
    }

    GameObject SelectFirstEnabledSaveButton()
    {
        foreach (Transform saveSlot in savesMenu.transform)
        {
            Button slotButton = saveSlot.GetComponent<Button>();
            if (slotButton.interactable)
            {
                eventSystem.SetSelectedGameObject(slotButton.gameObject);
                return slotButton.gameObject;
            }
        }
        return null;
    }

    void SelectDialogueButton()
    {
        foreach (Transform popUp in dialoguePopup.transform)
        {
            if (popUp.name == "MenuText")
            {
                continue;
            }

            if (popUp.gameObject.activeSelf)
            {
                Transform firstButton = popUp.GetChild(1); // ignore first child because it will always be text
                eventSystem.SetSelectedGameObject(firstButton.gameObject);
                return;
            }
        }
    }

    void GoBack()
    {
        // check if any popup is active
        bool popupActive = false;
        GameObject activePopup = null;
        
        foreach (Transform popup in dialoguePopup.transform)
        {
            if (popup.name == "MenuText")
                continue;
                
            if (popup.gameObject.activeSelf)
            {
                popupActive = true;
                activePopup = popup.gameObject;
                break;
            }
        }
        
        // if popup is active, return to previous menu
        if (popupActive && activePopup != null)
        {
            if (activePopup.name == "NewGamePopup" || activePopup.name == "ExitGamePopup" || activePopup.name == "NoSavesFoundPopup")
            {
                RestoreDisabledButtons();
                activePopup.SetActive(false);
                
                // if we're in the main menu, just close the popup
                if (mainMenu.activeSelf)
                {
                    SelectFirstButton();
                }
                else
                {
                    ReturnToMainMenu();
                }
            }
            else if (activePopup.name == "OverwriteSavePopup")
            {
                // return to save menu
                RestoreDisabledButtons();
                activePopup.SetActive(false);
                SelectFirstEnabledSaveButton();
            }
            else if (activePopup.name == "CreateSaveNamePopup")
            {
                // return to save menu
                RestoreDisabledButtons();
                activePopup.SetActive(false);
                SelectFirstEnabledSaveButton();
            }
            else if (activePopup.name == "SettingsPopup" || activePopup.name == "NoChangesPopup" || activePopup.name == "ConflictPopup")
            {
                ReturnToSettingsPage();
            }
            else
            {
                RestoreDisabledButtons();
                activePopup.SetActive(false);
                SelectFirstButton();
            }
        }
        else
        {
            if (settingsMenu.activeSelf)
            {
                OpenSettingsPopup("exit");
            }
            else if (savesMenu.activeSelf)
            {
                ReturnToMainMenu();
            }
            else if (galleryMenu.activeSelf)
            {
                ReturnToMainMenu();
            }
        }
    }

    void AddButtonEventTriggers(Button[] buttons, Button button, Dictionary<Button, Color> originalDisabledColors) // fixes being able to hover over other buttons when holding down one button
    {
        EventTrigger eventTrigger = button.gameObject.GetComponent<EventTrigger>();
        if (eventTrigger == null)
        {
            eventTrigger = button.gameObject.AddComponent<EventTrigger>();
        }

        // create PointerDown event for mouse
        EventTrigger.Entry pointerDownEntry = new EventTrigger.Entry();
        pointerDownEntry.eventID = EventTriggerType.PointerDown;
        pointerDownEntry.callback.AddListener((data) => OnButtonPressed(buttons, button));
        eventTrigger.triggers.Add(pointerDownEntry);

        // create PointerUp event for mouse
        EventTrigger.Entry pointerUpEntry = new EventTrigger.Entry();
        pointerUpEntry.eventID = EventTriggerType.PointerUp;
        pointerUpEntry.callback.AddListener((data) => OnButtonReleased(buttons, button, originalDisabledColors));
        eventTrigger.triggers.Add(pointerUpEntry);
    }

    void OnButtonPressed(Button[] buttons, Button pressedButton)
    {
        // store interactable buttons in modifiableButtons
        List<Button> interactableButtons = new List<Button>();
        foreach (Button button in buttons)
        {
            if (button != null && button.interactable && button != pressedButton)
            {
                interactableButtons.Add(button);
            }
        }
        modifiableButtons = interactableButtons.ToArray();

        // return early if the pressed button is not interactable
        if (!pressedButton.interactable)
        {
            return;
        }

        // disable all other buttons
        foreach (Button button in modifiableButtons)
        {
            button.interactable = false;

            // change disabled color to match normal color
            ColorBlock colors = button.colors;
            colors.disabledColor = colors.normalColor;
            button.colors = colors;
        }
        Debug.Log($"{pressedButton.name} is pressed.");
    }

    void OnButtonReleased(Button[] buttons, Button releasedButton, Dictionary<Button, Color> originalDisabledColors)
    {
        // re-enable buttons stored in modifiableButtons
        if (modifiableButtons != null)
        {
            foreach (Button button in modifiableButtons)
            {
                if (button != null)
                {
                    button.interactable = true;

                    // restore the original disabled color
                    if (originalDisabledColors.ContainsKey(button))
                    {
                        ColorBlock colors = button.colors;
                        colors.disabledColor = originalDisabledColors[button];
                        button.colors = colors;
                    }
                }
            }
        }
        Debug.Log($"{releasedButton.name} is released.");
    }

    void FocusOnPopup(GameObject popup)
    {
        DisableButtonsOutsidePopup(popup);

        if (popup == null || !popup.activeSelf)
            return;
            
        // find first button in popup
        Button firstButton = null;
        foreach (Transform child in popup.transform)
        {
            Button button = child.GetComponent<Button>();
            if (button != null && button.interactable)
            {
                firstButton = button;
                break;
            }
        }
        
        if (firstButton != null)
        {
            // set focus to the first button
            eventSystem.SetSelectedGameObject(firstButton.gameObject);
            lastSelectedButton = firstButton.gameObject;
        }
    }

    void NewGameClicked()
    {
        // activates the new game popup, and assigns functionality to the yes/no buttons
        GameObject newGamePopup = dialoguePopup.transform.Find("NewGamePopup").gameObject;
        mainMenu.SetActive(false);
        newGamePopup.SetActive(true);
        
        FocusOnPopup(newGamePopup);

        Button newGameYes = newGamePopup.transform.Find("Yes").GetComponent<Button>();
        Button newGameNo = newGamePopup.transform.Find("No").GetComponent<Button>();

        // assign listeners to yes and no buttons
        newGameYes.onClick.RemoveAllListeners();
        newGameNo.onClick.RemoveAllListeners();

        newGameYes.onClick.AddListener(OpenSavesMenu);
        newGameNo.onClick.AddListener(ReturnToMainMenu);
    }

    void ReturnToMainMenu()
    {
        RestoreDisabledButtons();

        // hides the active popup/menu and activates the main menu again
        mainMenu.SetActive(true);
        
        // disable all popups in dialoguePopup
        foreach (Transform popup in dialoguePopup.transform)
        {
            // skip the MenuText object since it's not a popup
            if (popup.name == "MenuText")
            {
                continue;
            }
                
            popup.gameObject.SetActive(false);
        }
        
        // disable all other menus
        settingsMenu.SetActive(false);
        galleryMenu.SetActive(false);
        savesMenu.SetActive(false);

        SelectFirstButton();
    }

    void LoadGameClicked()
    {
        // opens prompt to make new save if zero saves exist, otherwise opens save menu
        bool hasSaves = false;
        for (int i = 1; i <= 3; i++)
        {
            if (saveManager.SaveExists(i))
            {
                hasSaves = true;
                break;
            }
        }

        if (!hasSaves)
        {
            OpenNewSavePopup();
        }
        else
        {
            OpenSaveSlotSelection("load");
        }
    }

    void OpenNewSavePopup()
    {
        GameObject noSavesPopup = dialoguePopup.transform.Find("NoSavesFoundPopup").gameObject;
        noSavesPopup.SetActive(true);

        FocusOnPopup(noSavesPopup);

        Button noSaveYes = noSavesPopup.transform.Find("Yes").GetComponent<Button>();
        Button noSaveNo = noSavesPopup.transform.Find("No").GetComponent<Button>();

        noSaveYes.onClick.RemoveAllListeners();
        noSaveNo.onClick.RemoveAllListeners();

        noSaveYes.onClick.AddListener(OpenSavesMenu);
        noSaveNo.onClick.AddListener(ReturnToMainMenu);
    }

    void OpenSavesMenu()
    {
        GameObject noSavesPopup = dialoguePopup.transform.Find("NoSavesFoundPopup").gameObject; // not a param cuz it won't always be called
        GameObject newGamePopup = dialoguePopup.transform.Find("NewGamePopup").gameObject;
        
        RestoreDisabledButtons();
        noSavesPopup.SetActive(false);
        newGamePopup.SetActive(false);

        int saveCount = 0;
        for (int i = 1; i <= SaveController.MAX_SLOTS; i++) // count number of valid saves
        {
            if (saveManager.SaveExists(i))
            {
                saveCount++;
            }
        }

        if (saveCount == SaveController.MAX_SLOTS) // apparently const variables need to be called with THE WHOLE class and not an instance
        {
            OpenSaveSlotSelection("overwrite");
        }
        else
        {
            OpenSaveSlotSelection("save");
        }
    }

    void UpdateSaveSlots()
    {
        // update text boxes for save slots
        int slotIndex = 0;
        foreach (Transform saveSlot in savesMenu.transform)
        {
            if (slotIndex > 2) // ignore the return to main menu button at the end
            {
                break;
            }
            GameObject nameLabel = saveSlot.Find("Name").gameObject;
            GameObject dayNightLabel = saveSlot.Find("DayNight").gameObject;
            GameObject noSavesLabel = saveSlot.Find("NoSaves").gameObject;
            Button slotButton = saveSlot.GetComponent<Button>();

            slotIndex++;
            int currentSlot = slotIndex;

            SaveData saveData = saveManager.GetSaveData(currentSlot);
            bool slotHasSave = saveData != null;

            if (slotHasSave)
            {
                nameLabel.GetComponent<TMP_Text>().text = saveData.saveName;
                dayNightLabel.GetComponent<TMP_Text>().text = ConvertDayNightToString(saveData.currentDay, saveData.isNight);
            }

            // if save is empty (display empty save text), otherwise display name and current day/night
            nameLabel.SetActive(slotHasSave);
            dayNightLabel.SetActive(slotHasSave);
            noSavesLabel.SetActive(!slotHasSave);
        }
    }

    string ConvertDayNightToString(int currentDay, bool isNight)
    {
        int lastDigit = currentDay % 10;
        string dayNightText;

        switch (lastDigit) // sup is for subscript
        {
            case 1:
                dayNightText = $"{currentDay}<sup>st</sup>";
                break;
            case 2:
                dayNightText = $"{currentDay}<sup>nd</sup>";
                break;
            case 3:
                dayNightText = $"{currentDay}<sup>rd</sup>";
                break;
            default:
                dayNightText = $"{currentDay}<sup>th</sup>";
                break;
        }
        
        if (isNight)
        {
            dayNightText += " Night";
        }
        else
        {
            dayNightText += " Day";
        }
        return dayNightText;
    }

    void OpenOverwritePopup(int currentSlot)
    {
        GameObject overwritePopup = dialoguePopup.transform.Find("OverwriteSavePopup").gameObject;
        overwritePopup.SetActive(true);

        FocusOnPopup(overwritePopup);

        Button overwriteYes = overwritePopup.transform.Find("Yes").GetComponent<Button>();
        Button overwriteNo = overwritePopup.transform.Find("No").GetComponent<Button>();

        overwriteYes.onClick.RemoveAllListeners();
        overwriteNo.onClick.RemoveAllListeners();

        overwriteYes.onClick.AddListener(() => OpenPromptForGameName(currentSlot));
        overwriteNo.onClick.AddListener(() => returnFromOverwritePopup(overwritePopup));

        // perhaps text prompt informing user that they must click a file to overwrite "Click the file you would like to overwrite"
    }

    void returnFromOverwritePopup(GameObject overwritePopup)
    {
        RestoreDisabledButtons();
        overwritePopup.SetActive(false);
        SelectFirstButton();
        SelectFirstEnabledSaveButton();
    }

    void OpenSaveSlotSelection(string action)
    {
        Button returnToMain = savesMenu.transform.Find("ReturnToMain").GetComponent<Button>();
        returnToMain.onClick.RemoveAllListeners();
        returnToMain.onClick.AddListener(ReturnToMainMenu);

        int slotIndex = 0;
        foreach (Transform saveSlot in savesMenu.transform) // initialize save slots
        {
            if (slotIndex > 2)
            {
                break;
            }
            Button slotButton = saveSlot.GetComponent<Button>();
            slotIndex++;
            int currentSlot = slotIndex; // apparently lambda functions capture variables by REFERENCE not value

            slotButton.onClick.RemoveAllListeners();
            slotButton.interactable = true; // just set as true by default

            if (action == "save")
            {
                if (saveManager.SaveExists(currentSlot))
                {
                    slotButton.onClick.AddListener(() => OpenOverwritePopup(currentSlot));
                }
                else
                {
                    slotButton.onClick.AddListener(() => OpenPromptForGameName(currentSlot));
                }
            }
            if (action == "load")
            {
                if (saveManager.SaveExists(currentSlot))
                {
                    slotButton.onClick.AddListener(() => LoadSave(currentSlot));
                }
                else
                {
                    slotButton.interactable = false;
                }
            }
            if (action == "overwrite")
            {
                slotButton.onClick.AddListener(() => OpenOverwritePopup(currentSlot));
            }
        }

        savesMenu.SetActive(true); // at the bottom instead of the top so buttons don't change when loading in
        mainMenu.SetActive(false);

        UpdateSaveSlots();
        SelectFirstButton();
    }
    
    void OpenPromptForGameName(int currentSlot)
    {
        GameObject namePopup = dialoguePopup.transform.Find("CreateSaveNamePopup").gameObject;
        GameObject overwritePopup = dialoguePopup.transform.Find("OverwriteSavePopup").gameObject;

        Button cancelNewSave = namePopup.transform.Find("Cancel").GetComponent<Button>();
        Button finishedName = namePopup.transform.Find("Done").GetComponent<Button>();

        TMP_InputField nameInputField = namePopup.transform.Find("NameInputField").GetComponent<TMP_InputField>();

        RestoreDisabledButtons();
        overwritePopup.SetActive(false);

        namePopup.SetActive(true);
        DisableButtonsOutsidePopup(namePopup);

        eventSystem.SetSelectedGameObject(nameInputField.gameObject);

        finishedName.onClick.RemoveAllListeners();
        cancelNewSave.onClick.RemoveAllListeners();
        
        finishedName.onClick.AddListener(() => SaveEnteredName(namePopup, currentSlot, nameInputField));
        cancelNewSave.onClick.AddListener(() => CancelNewSave(namePopup, nameInputField));
    }

    void CancelNewSave(GameObject namePopup, TMP_InputField nameInputField)
    {
        RestoreDisabledButtons();

        nameInputField.text = "";
        namePopup.SetActive(false);
        SelectFirstButton();
    }

    void SaveEnteredName(GameObject namePopup, int saveSlot, TMP_InputField nameInputField)
    {
        string enteredName = nameInputField.text.Trim();

        if (string.IsNullOrEmpty(enteredName))
        {
            TriggerFadeMessage("Cannot save game without a name.");
            return; // listener so i don't have to recursively call it
        }

        RestoreDisabledButtons();
        namePopup.SetActive(false);
        if (saveManager.SaveExists(saveSlot))
        {
            saveManager.DeleteSave(saveSlot);
            Debug.Log($"Save in slot {saveSlot} was overwritten with new save: {enteredName}");
        }
        else
        {
            Debug.Log($"New game saved to slot {saveSlot} with name '{enteredName}'");
        }
        saveManager.CreateSave(saveSlot, enteredName);
        LoadSave(saveSlot);
    }

    void LoadSave(int saveSlot)
    {
        // open scene and load data from save slot
        string startingScene = saveManager.GetSavedSceneName(saveSlot);

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(startingScene);

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            saveManager.LoadGame(saveSlot);
            Debug.Log($"Loaded slot {saveSlot}.");
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    void SettingsClicked()
    {
        // hides the main menu and activates the settings section
        mainMenu.SetActive(false);
        settingsMenu.SetActive(true);
        menuTextLabel.text = "";

        GameObject settingsButtonHolder = settingsMenu.transform.Find("SettingPanel").gameObject;
        GameObject settingsOptions = settingsMenu.transform.Find("OptionPanel").gameObject;

        Button audioButton = settingsButtonHolder.transform.Find("AudioButton").GetComponent<Button>();
        Button videoButton = settingsButtonHolder.transform.Find("VideoButton").GetComponent<Button>();
        Button controlsButton = settingsButtonHolder.transform.Find("ControlsButton").GetComponent<Button>();

        Button resetButton = settingsOptions.transform.Find("Reset").GetComponent<Button>();
        Button exitButton = settingsOptions.transform.Find("Exit").GetComponent<Button>();
        Button saveButton = settingsOptions.transform.Find("Save").GetComponent<Button>();

        GameObject audioSettings = settingsMenu.transform.Find("AudioSettings").gameObject;
        GameObject videoSettings = settingsMenu.transform.Find("VideoSettings").gameObject;
        GameObject controlsSettings = settingsMenu.transform.Find("ControlsSettings").gameObject;

        Button[] settingsButtons = {audioButton, videoButton, controlsButton};

        InitializeKeybindButtons(controlsSettings);

        // set default "panel" and button (will be audio since it's the first)
        SetActiveSettingTab(audioSettings.gameObject, audioButton, settingsButtons);
        if (volumeSettings != null) // update sliders
        {
            volumeSettings.LoadVolume();
        }

        // reassign listeners to settings buttons to avoid issues
        audioButton.onClick.RemoveAllListeners();
        videoButton.onClick.RemoveAllListeners();
        controlsButton.onClick.RemoveAllListeners();

        audioButton.onClick.AddListener(() => SetActiveSettingTab(audioSettings, audioButton, settingsButtons)); // lambda expression, allows a parameter to pass through the function
        videoButton.onClick.AddListener(() => SetActiveSettingTab(videoSettings, videoButton, settingsButtons));
        controlsButton.onClick.AddListener(() => SetActiveSettingTab(controlsSettings, controlsButton, settingsButtons));

        resetButton.onClick.RemoveAllListeners();
        exitButton.onClick.RemoveAllListeners();
        saveButton.onClick.RemoveAllListeners();

        resetButton.onClick.AddListener(() => OpenSettingsPopup("reset"));
        exitButton.onClick.AddListener(() => OpenSettingsPopup("exit"));
        saveButton.onClick.AddListener(() => OpenSettingsPopup("save"));
    }

    void InitializeKeybindButtons(GameObject controlsSettings)
    {
        // find buttons in videoSettings object
        foreach (Transform keybindObject in controlsSettings.transform)
        {
            Button button = keybindObject.GetComponentInChildren<Button>();

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
        Button pressedButton = GetOtherPressedButton(keyButton);
        if (pressedButton != null) // realized i didn't need an entire other function to do basically the same thing
        {
            Debug.Log("Another button was pressed.");
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
        bool ignoreNextKeyPress = true;

        UpdateButtonStyle(keyButton, keyButton.interactable);

        // waits until a key is pressed and assigned
        while (!isKeySet)
        {
            // check if conflict popup is active
            GameObject conflictPopup = dialoguePopup.transform.Find("ConflictPopup").gameObject;
            if (conflictPopup.activeSelf)
            {
                yield break;
            }
            
            if (Input.GetKeyDown(KeyCode.Mouse0)) // ignore key press if it's just the left mouse button
            {
                break;
            }
            
            // skip first frame to ignore enter key
            if (ignoreNextKeyPress)
            {
                ignoreNextKeyPress = false;
                yield return null;
                continue;
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
                        yield break;
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

    Button GetOtherPressedButton(Button currentButton)
    {
        GameObject controlsSettings = settingsMenu.transform.Find("ControlsSettings").gameObject;
        Button[] keybindButtons = controlsSettings.GetComponentsInChildren<Button>();

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
        Color defaultColor = new Color(0.75f, 0.75f, 0.75f);
        
        // reset button colors to default
        ColorBlock colors = keyButton.colors;
        colors.normalColor = defaultColor;
        keyButton.colors = colors;
        
        // refresh button
        keyButton.gameObject.SetActive(false);
        keyButton.gameObject.SetActive(true);

        // highlight button on release
        eventSystem.SetSelectedGameObject(keyButton.gameObject);
        lastSelectedButton = keyButton.gameObject;
    }

    void OpenConflictPopup(Button keyButton, KeyCode newKey)
    {
        // stop current keybind coroutine if running
        if (currentKeybindCoroutine != null)
        {
            StopCoroutine(currentKeybindCoroutine);
            currentKeybindCoroutine = null;
        }

        GameObject conflictPopup = dialoguePopup.transform.Find("ConflictPopup").gameObject;
        conflictPopup.SetActive(true);

        FocusOnPopup(conflictPopup);

        Button conflictYes = conflictPopup.transform.Find("Yes").GetComponent<Button>();
        Button conflictNo = conflictPopup.transform.Find("No").GetComponent<Button>();

        conflictYes.onClick.RemoveAllListeners();
        conflictNo.onClick.RemoveAllListeners();

        string buttonAction = GetButtonKeybind(keyButton);
        
        conflictYes.onClick.AddListener(() => resolveConflict(buttonAction, newKey, keyButton));  
        conflictNo.onClick.AddListener(() => ignoreKeybindChange(keyButton));
    }

    void resolveConflict(string buttonAction, KeyCode newKey, Button keyButton)
    {
        // apply original conflicting key
        keybindManager.UpdateKeybind(buttonAction, newKey);
        keyButton.GetComponentInChildren<TextMeshProUGUI>().text = newKey.ToString();
        
        CancelKeybindUpdate(keyButton);
        RemoveDuplicateKeys(keyButton, newKey);
        RefreshAllKeybindButtons();
        
        ReturnToSettingsPage(keyButton.gameObject);

        eventSystem.SetSelectedGameObject(keyButton.gameObject);
        lastSelectedButton = keyButton.gameObject;
    }

    void ignoreKeybindChange(Button keyButton)
    {
        CancelKeybindUpdate(keyButton);
        ReturnToSettingsPage(keyButton.gameObject);

        eventSystem.SetSelectedGameObject(keyButton.gameObject);
        lastSelectedButton = keyButton.gameObject;
    }

    void RefreshAllKeybindButtons()
    {
        GameObject controlsSettings = settingsMenu.transform.Find("ControlsSettings").gameObject;
        
        foreach (Transform keybindObject in controlsSettings.transform)
        {
            Button button = keybindObject.GetComponentInChildren<Button>();
            if (button != null)
            {
                string keybind = GetButtonKeybind(button);
                if (keybind != null)
                {
                    KeyCode key = keybindManager.GetKeybind(keybind);
                    string displayText = (key == KeyCode.None) ? "Not Bound" : key.ToString();
                    button.GetComponentInChildren<TextMeshProUGUI>().text = displayText;
                }
            }
        }
    }

    void RemoveDuplicateKeys(Button keyButton, KeyCode originalKey)
    {
        string originalAction = GetButtonKeybind(keyButton);
        Dictionary<string, KeyCode> keybinds = keybindManager.GetTempKeybinds();
        List<string> actionsToUnbind = new List<string>();

        // find all actions that use same key
        foreach (var keybind in keybinds)
        {
            string action = keybind.Key;
            KeyCode key = keybind.Value;

            // add action to list if keys match (dupe) and it's not the original action
            if (key == originalKey && action != originalAction)
            {
                actionsToUnbind.Add(action);
            }
        }

        // unbind all duplicate actions
        foreach (string action in actionsToUnbind)
        {
            keybindManager.UpdateKeybind(action, KeyCode.None);
            Debug.Log($"Duplicate key '{originalKey}' removed from action '{action}'.");
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

            case "GameMenuButton":
                return "GameMenu";
            
            case "InventoryButton":
                return "Inventory";

            case "MapButton":
                return "Map";
            
            case "SwitchFormButton":
                return "SwitchForm";

            default:
                return null;
        }
    }

    void SetActiveSettingTab(GameObject selectedPanel, Button activeButton, Button[] buttons)
    {
        // deactivate all settings panels
        settingsMenu.transform.Find("AudioSettings").gameObject.SetActive(false);
        settingsMenu.transform.Find("VideoSettings").gameObject.SetActive(false);
        settingsMenu.transform.Find("ControlsSettings").gameObject.SetActive(false);
        
        GameObject controlsSettings = settingsMenu.transform.Find("ControlsSettings").gameObject;
        Button[] keybindButtons = controlsSettings.GetComponentsInChildren<Button>();
        foreach (Button button in keybindButtons)
        {
            CancelKeybindUpdate(button);
        }

        // activate the selected panel
        selectedPanel.SetActive(true);

        // update button states
        UpdateButtonStates(activeButton, buttons);
        SelectFirstButton();
    }

    IEnumerator FadeMessage(string message)
    {
        menuTextLabel.text = message;
        menuTextLabel.alpha = 1f; // still don't get why floats need fs on them

        float duration = 2f;
        while (menuTextLabel.alpha > 0)
        {
            menuTextLabel.alpha -= Time.deltaTime / duration;
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
        Color defaultColor = new Color(0.75f, 0.75f, 0.75f);

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
        GameObject returnButton = eventSystem.currentSelectedGameObject;

        // activates the settings pop up to reset/confirm changes made and continues based off user response
        GameObject settingsPopup = dialoguePopup.transform.Find("SettingsPopup").gameObject;
        TextMeshProUGUI popupTextLabel = settingsPopup.transform.GetChild(0).GetComponent<TextMeshProUGUI>();

        // creates pop up text depending on which button was clicked
        if (action == "reset")
        {
            popupTextLabel.text = "Would you like to restore all settings to default?";
            settingsPopup.SetActive(true);
            FocusOnPopup(settingsPopup);
        }
        
        if (action == "exit")
        {
            bool unsavedChanges = false;
            if (keybindManager.HasKeybindChanged())
            {
                unsavedChanges = true;
            }
            if (volumeSettings != null && volumeSettings.HasSliderChanged())
            {
                unsavedChanges = true;
            }

            if (unsavedChanges)
            {
                // prompt user to save keybinds if at least one keybind has changed
                popupTextLabel.text = "You have unsaved changes. Are you sure you want to exit?";
                settingsPopup.SetActive(true);
                FocusOnPopup(settingsPopup);
            }
            else if (keybindManager.HasNewNullKeybinds())
            {
                popupTextLabel.text = "You have at least one unbound keybind. Are you sure you want to exit?";
                settingsPopup.SetActive(true);
                FocusOnPopup(settingsPopup);
            }

            // if there's no conflict or unsaved changes, just return to the main menu
            else
            {
                ReturnToMainMenu();
            }
        }

        if (action == "save")
        {
            bool hasChangesToSave = false;

            if (keybindManager.HasKeybindChanged())
            {
                hasChangesToSave = true;
            }
            if (volumeSettings != null && volumeSettings.HasSliderChanged())
            {
                hasChangesToSave = true;
            }

            if (hasChangesToSave)
            {
                popupTextLabel.text = "Would you like to save all changes?";
                settingsPopup.SetActive(true);
                FocusOnPopup(settingsPopup);
            }
            else
            {
                GameObject noChangesPopup = dialoguePopup.transform.Find("NoChangesPopup").gameObject;
                noChangesPopup.SetActive(true);
                FocusOnPopup(noChangesPopup);

                Button settingsOkay = noChangesPopup.transform.Find("Okay").GetComponent<Button>();
                settingsOkay.onClick.RemoveAllListeners();
                settingsOkay.onClick.AddListener(() => ReturnToSettingsPage(returnButton));
                return;
            }
        }

        Button settingsYes = settingsPopup.transform.Find("Yes").GetComponent<Button>();
        Button settingsNo = settingsPopup.transform.Find("No").GetComponent<Button>();
        
        // fixes issue with buttons not correctly responding to user input
        settingsYes.onClick.RemoveAllListeners();
        settingsNo.onClick.RemoveAllListeners();

        // assign listeners to yes/no buttons
        settingsYes.onClick.AddListener(() => SettingsChange(action, "yes", returnButton));
        settingsNo.onClick.AddListener(() => SettingsChange(action, "no", returnButton));
    }

    void ReturnToSettingsPage(GameObject buttonToFocus = null) // optional parameter
    {
        RestoreDisabledButtons();

        GameObject noChangesPopup = dialoguePopup.transform.Find("NoChangesPopup").gameObject;
        GameObject settingsPopup = dialoguePopup.transform.Find("SettingsPopup").gameObject;
        GameObject conflictPopup = dialoguePopup.transform.Find("ConflictPopup").gameObject;

        settingsPopup.SetActive(false);
        noChangesPopup.SetActive(false);
        conflictPopup.SetActive(false);

        if (buttonToFocus != null && buttonToFocus.activeInHierarchy)
        {
            eventSystem.SetSelectedGameObject(buttonToFocus);
            lastSelectedButton = buttonToFocus;
        }
        else
        {
            SelectFirstButton();
        }
    }

    void SettingsChange(string action, string choice, GameObject returnButton)
    {
        // functionality for buttons in settings pop up
        Debug.Log($"{action}, {choice}");
        ReturnToSettingsPage(returnButton);

        // different cases for settings button presses
        GameObject buttonToSelect = lastSelectedButton;
        switch (action)
        {
            case "reset":
                if (choice == "yes")
                {
                    keybindManager.ResetKeybinds();
                    RefreshKeybindButtons(); // visually resets the keybinds
                    volumeSettings.resetSliderValues();

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
                    
                    // revert audio slider changes
                    if (volumeSettings != null && volumeSettings.HasSliderChanged())
                    {
                        volumeSettings.RevertSliderChanges();
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

                    if (volumeSettings != null)
                    {
                        volumeSettings.SetMasterVol();
                        volumeSettings.SetMusicVol();
                        volumeSettings.SetSFXVol();
                        volumeSettings.SaveCurrentSliderValues();
                    }
                    TriggerFadeMessage("Changes have been saved.");
                    Debug.Log("Changes have been saved.");
                }
                break;
        }
        if (buttonToSelect != null && buttonToSelect.activeInHierarchy)
        {
            eventSystem.SetSelectedGameObject(buttonToSelect);
            lastSelectedButton = buttonToSelect;
        }
        else
        {
            SelectFirstButton();
        }
    }

    void RefreshKeybindButtons()
    {
        Transform controlsSettings = settingsMenu.transform.Find("ControlsSettings");
        foreach (Transform keybindObject in controlsSettings)
        {
            Button button = keybindObject.GetComponentInChildren<Button>();
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
        Button returnToMain = galleryMenu.transform.Find("ReturnToMain").GetComponent<Button>();

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
        
        FocusOnPopup(exitGamePopup);

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
        Debug.Log("Closed Game."); // can't actually close game in editor
    }
}