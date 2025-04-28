using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    [HideInInspector] public GameObject currentOpenPanel;

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

    public bool TogglePanel(GameObject panel, bool forceState = false, bool desiredState = false, bool isNPCPanel = false)
    {
        // Check if we can toggle panels
        // Allow NPC panels like shops to open during interaction
        if (NPCInteraction.IsInteracting && !isNPCPanel)
        {
            // Only allow force closing during dialogue
            if (forceState && !desiredState)
            {
                panel.SetActive(false);
                if (currentOpenPanel == panel) currentOpenPanel = null;
                return false;
            }
            return panel.activeSelf;
        }

        // Rest of the function remains the same
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

    public void CloseAllPanels()
    {
        if (currentOpenPanel != null)
        {
            currentOpenPanel.SetActive(false);
            currentOpenPanel = null;
        }
    }
}