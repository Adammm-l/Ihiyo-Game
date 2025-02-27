using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
// Terrence
public class KeybindManager : MonoBehaviour // Terrence Akinola
{
    [Header("Keybinds")]
    Dictionary<string, KeyCode> currentKeybinds;
    Dictionary<string, KeyCode> tempKeybinds = new Dictionary<string, KeyCode>();
    Dictionary<string, KeyCode> defaultKeybinds = new Dictionary<string, KeyCode>();
    Dictionary<string, KeyCode> oldKeybinds = new Dictionary<string, KeyCode>();

    // putting it on awake fixes issues with keybindmanager being null on certain start calls
    private static bool keybindExists; // All instances of this Player references the exact same variable
    void Awake()
    {
        // initializes default keybinds
        defaultKeybinds = new Dictionary<string, KeyCode>
        {
            {"MoveUp", KeyCode.W},
            {"MoveLeft", KeyCode.A},
            {"MoveDown", KeyCode.S},
            {"MoveRight", KeyCode.D},
            {"Interact", KeyCode.E},
            {"QuestLog", KeyCode.Q},
            {"GameMenu", KeyCode.Tab},
            {"Inventory", KeyCode.I},
            {"Map", KeyCode.M},
            {"SwitchForm", KeyCode.T}
        };

        // loads the saved keybinds/defaults if saved keybinds do not exist.
        LoadKeybinds();
    }

    void LoadKeybinds()
    {
        currentKeybinds = new Dictionary<string, KeyCode>();

        // loads the saved keybinds if available and use defaults if not
        foreach (var keybind in defaultKeybinds)
        {
            string action = keybind.Key;
            KeyCode defaultKey = keybind.Value;

            // checks if a saved keybind exists for this action
            if (PlayerPrefs.HasKey(action))
            {
                string savedKey = PlayerPrefs.GetString(action);
                currentKeybinds[action] = (KeyCode)Enum.Parse(typeof(KeyCode), savedKey);
            }

            // otherwise load the existing default keybind
            else
            {
                currentKeybinds[action] = defaultKey;
            }
        }
        tempKeybinds = new Dictionary<string, KeyCode>(currentKeybinds);
    }

    void SaveKeybinds()
    {
        // saves the current keybinds, ensuring it persists between saves
        foreach (var keybind in currentKeybinds)
        {
            PlayerPrefs.SetString(keybind.Key, keybind.Value.ToString());
        }
        PlayerPrefs.Save();
    }

    public KeyCode GetKeybind(string action)
    {
        KeyCode key;

        // gets the keybind for the specified action if it exists.
        if (tempKeybinds.ContainsKey(action))
        {
            key = tempKeybinds[action];
        }

        // otherwise returns keycode None which is apparently a keybind
        else
        {
            key = KeyCode.None;
        }
        return key;
    }

    public Dictionary<string, KeyCode> GetTempKeybinds()
    {   
        return tempKeybinds;
    }

    public void UpdateKeybind(string action, KeyCode newKey)
    {
        // updates keybind for the specified action to be stored temporarily (until save or reset)
        if (tempKeybinds.ContainsKey(action))
        {
            tempKeybinds[action] = newKey;
        }
    }

    public void ConfirmKeybinds()
    {
        // saves any modified keybinds
        oldKeybinds = new Dictionary<string, KeyCode>(currentKeybinds);
        currentKeybinds = new Dictionary<string, KeyCode>(tempKeybinds);
        SaveKeybinds();
    }

    public void ResetKeybinds()
    {
        // resets the temporary keybinds to original default keybinds
        tempKeybinds = new Dictionary<string, KeyCode>(defaultKeybinds);
    }

    public bool HasKeybindChanged()
    {
        // compare each key between current and temporary keybinds
        foreach (var action in currentKeybinds)
        {
            // note that a keybind has changed if the current and temp actions are different
            var oldBinding = action.Value;
            var newBinding = tempKeybinds[action.Key];

            bool isNewKeybind = !tempKeybinds.ContainsKey(action.Key);

            if (isNewKeybind || newBinding != oldBinding)
            {
                return true;
            }
        }

        return false; // otherwise return false cuz no changes have been found
    }

    public void DiscardKeybindChanges()
    {  
        // reverts temp keybinds back to last saved state
        tempKeybinds = new Dictionary<string, KeyCode>(currentKeybinds);
    }

    public bool IsKeybindConflict(string action, KeyCode key)
    {
        // checks for instance of given keybind bound to a different action
        foreach (var keybind in tempKeybinds)
        {
            if (keybind.Value == key && keybind.Key != action)
            {
                return true;
            }
        }
        return false; // all keybinds are unique
    }

    public bool HasNewNullKeybinds()
    {
        foreach (var keybind in tempKeybinds)
        {
            string keyAction = keybind.Key;
            if (keybind.Value == KeyCode.None && oldKeybinds[keyAction] != tempKeybinds[keyAction])
            {
                return true;
            }
        }
        return false;
    }

    void Start() {
        if(!keybindExists) { // If the player doesn't exist, then mark them as Don't Destroy on Load, handling duplicates

            keybindExists = true;
            DontDestroyOnLoad(transform.gameObject);
        }

        else { // Eliminate Duplicate Objects
            Destroy(gameObject);
        }
}
}