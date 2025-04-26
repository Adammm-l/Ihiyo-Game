using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Adam
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    private GameObject currentOpenPanel;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool TogglePanel(GameObject panel, bool forceState = false, bool desiredState = false)
    {
        if (panel.activeSelf && !forceState)
        {
            panel.SetActive(false);
            currentOpenPanel = null;
            return false;
        }
        if (currentOpenPanel != null && currentOpenPanel != panel)
        {
            currentOpenPanel.SetActive(false);
        }
        bool newState = forceState ? desiredState : true;
        panel.SetActive(newState);
        currentOpenPanel = newState ? panel : null;
        return newState;
    }

    public bool CanTogglePanel()
    {
        return !NPCInteraction.IsInteracting;
    }
}