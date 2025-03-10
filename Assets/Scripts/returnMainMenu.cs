using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuButton : MonoBehaviour
{
    public void ReturnToMainMenu()
    {
        Debug.Log("Returning to Main Menu...");

        // Load the main menu scene
        string mainMenuSceneName = "MainMenu"; // Replace with the exact name of your main menu scene
        if (Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            Debug.LogError($"Scene '{mainMenuSceneName}' not found in Build Settings!");
        }
    }
}