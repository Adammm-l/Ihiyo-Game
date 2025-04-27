using UnityEngine;

public class AutoSaveManager : MonoBehaviour
{
    [Header("Settings")]
    public float saveInterval = 300f; // default: 5 minutes
    public bool isInitialized;
    float timeSinceLastSave;

    public void Initialize()
    {
        if (SaveController.Instance == null)
        {
            Debug.LogError("Save controller not found!");
            enabled = false;
            return;
        }
        
        isInitialized = true;
        timeSinceLastSave = 0f;
    }

    void Update()
    {
        if (!isInitialized) return;
        
        timeSinceLastSave += Time.deltaTime;
        
        if (timeSinceLastSave >= saveInterval)
        {
            TriggerSave();
            timeSinceLastSave = 0f;
        }
    }

    public void TriggerSave()
    {
        Debug.Log($"Auto-saving at {Time.time} seconds");
        SaveController.Instance.SaveGame();
    }

    public void OnManualSave()
    {
        timeSinceLastSave = 0f;
    }
}