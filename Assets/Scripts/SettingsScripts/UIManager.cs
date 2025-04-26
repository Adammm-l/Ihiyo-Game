using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Adam
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    PlayerControl playerControl;
    TimeManager timeManager;
    private GameObject currentOpenPanel;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            playerControl = FindObjectOfType<PlayerControl>();
            timeManager = FindObjectOfType<TimeManager>();
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

            playerControl.canMove = true;
            timeManager.canIncrementTime = true;
            return false;
        }
        if (currentOpenPanel != null && currentOpenPanel != panel)
        {
            currentOpenPanel.SetActive(false);
        }
        bool newState = forceState ? desiredState : true;
        panel.SetActive(newState);
        currentOpenPanel = newState ? panel : null;

        if (newState)
        {
            playerControl.canMove = false;
        }
        else
        {
            playerControl.canMove = true;
        }
        timeManager.canIncrementTime = playerControl.canMove;
        return newState;
    }

    public bool CanTogglePanel()
    {
        return !NPCInteraction.IsInteracting;
    }
}