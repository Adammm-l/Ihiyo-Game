using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class KeybindManager : MonoBehaviour
{
    [Header("Keybinds")]
    Dictionary<string, KeyCode> currentKeybinds;
    Dictionary<string, KeyCode> tempKeybinds = new Dictionary<string, KeyCode>();
    Dictionary<string, KeyCode> defaultKeybinds = new Dictionary<string, KeyCode>();

    // Start is called before the first frame update
    void Start()
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
            {"Inventory", KeyCode.Tab}
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
        if (currentKeybinds.ContainsKey(action))
        {
            key = currentKeybinds[action];
        }

        // otherwise returns keycode None which is apparently a keybind
        else
        {
            key = KeyCode.None;
        }
        return key;
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
        currentKeybinds = new Dictionary<string, KeyCode>(tempKeybinds);
        SaveKeybinds();
    }

    public void ResetKeybinds()
    {
        // resets the temporary keybinds to original default keybinds
        tempKeybinds = new Dictionary<string, KeyCode>(defaultKeybinds);
    }
}