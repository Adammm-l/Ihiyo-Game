using UnityEngine;
using UnityEngine.SceneManagement;

public class UIPreserver : MonoBehaviour
{
    [Tooltip("Drag all UI GameObjects you want to preserve here")]
    public GameObject[] uiElementsToPreserve;

    private static bool hasPreservedUI = false;

    private void Awake()
    {
        // only preserve UI once (when loading from main menu)
        if (!hasPreservedUI && SceneManager.GetActiveScene().name == "MainMenu")
        {
            PreserveUIElements();
            hasPreservedUI = true;
        }
    }

    private void PreserveUIElements()
    {
        if (uiElementsToPreserve == null || uiElementsToPreserve.Length == 0)
        {
            Debug.LogWarning("No UI elements assigned to preserve in UIPreserver");
            return;
        }

        foreach (GameObject uiElement in uiElementsToPreserve)
        {
            if (uiElement != null)
            {
                DontDestroyOnLoad(uiElement);
                
                // optional: Keep the hierarchy clean by moving to a parent object
                if (GameObject.Find("PreservedUI") == null)
                {
                    new GameObject("PreservedUI");
                }
                GameObject preservedUIParent = GameObject.Find("PreservedUI");
                uiElement.transform.SetParent(preservedUIParent.transform);
            }
        }
    }
}
// IDK IT'S NOT WORKING RN