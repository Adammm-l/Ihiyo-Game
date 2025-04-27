using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// Terrence and Edwin (Eri)
public class SaveController : MonoBehaviour // Terrence Akinola / Edwin (Eri) Sotres
{
    public const int MAX_SLOTS = 3;
    const string ActiveSlotKey = "ActiveSaveSlot";
    private string saveLocation;
    SaveData defaultSaveData;

    // data to change
    TimeManager timeManager;
    SwitchPlayerForm switchForm;
    public AutoSaveManager autoSaveManager;

    public static SaveController Instance;
    //private static bool saveExists; // All instances of this Player references the exact same variable

    // Start is called before the first frame update

    void Awake() 
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Initialize here instead of Start
            saveLocation = Path.Combine(Application.persistentDataPath, "Saves");
            Directory.CreateDirectory(saveLocation);

            defaultSaveData = new SaveData
            {
                playerPosition = new Vector3(-5.5f, 1.25f, 0f),
                sceneName = "Ihi_House",
                mapBoundary = "Ihi_Room",
                currentTime = "8:00 AM",
                isGhost = false,
                canTransform = false,
                canChangeTime = false
            };

            autoSaveManager = GetComponent<AutoSaveManager>();
        }
        else if (Instance != this)
        {
            DestroyImmediate(gameObject);
            return;
        }
    }

    public void SaveGame() 
    {
        int activeSaveSlot = GetActiveSlot();


        saveLocation = Path.Combine(Application.persistentDataPath, "Saves");
        string savePath = Path.Combine(saveLocation, $"save_{activeSaveSlot}.json");
        
        SaveData saveData;
        if (File.Exists(savePath))
        {
            saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(savePath));
        }
        else
        {
            saveData = new SaveData(); // initialize new save data if no file exists (literally just to please the compiler)
        }

        timeManager = FindObjectOfType<TimeManager>();
        saveData.currentTime = timeManager.GetFormattedTime();

        saveData.playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position;
        saveData.sceneName = SceneManager.GetActiveScene().name;
        saveData.mapBoundary = FindObjectOfType<CinemachineConfiner>().m_BoundingShape2D.gameObject.name;

        switchForm = FindObjectOfType<SwitchPlayerForm>();
        saveData.isGhost = switchForm.isGhost;
        saveData.canTransform = switchForm.canTransform;
        // canChangeTime = defaultSaveData.canChangeTime

        File.WriteAllText(savePath, JsonUtility.ToJson(saveData)); //Writes the Data to a file
        Debug.Log($"Saved game on slot {activeSaveSlot}.");
    }

    public void CreateSave(int slot, string name) 
    {
        string savePath = Path.Combine(saveLocation, $"save_{slot}.json");
        
        SaveData saveData = new SaveData 
        {
            playerPosition = defaultSaveData.playerPosition,
            sceneName = defaultSaveData.sceneName,
            mapBoundary = defaultSaveData.mapBoundary,
            saveName = name,
            currentTime = defaultSaveData.currentTime,
            isGhost = defaultSaveData.isGhost,
            canTransform = defaultSaveData.canTransform,
            canChangeTime = defaultSaveData.canChangeTime
        };

        File.WriteAllText(savePath, JsonUtility.ToJson(saveData)); //Writes the Data to a file
    }

    public bool LoadGame(int slot) {
        string savePath = Path.Combine(saveLocation, $"save_{slot}.json");
        SetSaveSlot(slot); // remember active save slot for saving ingame

        // Button saveButton = GameObject.Find("SaveButton").GetComponent<Button>();
        // if (saveButton != null) {
        //     saveButton.onClick.AddListener(SaveGame);
        //     Debug.Log("Button Reconnected!");
        // }
        // else { 
        //     Debug.LogError("Button not found");
        // }
        
        if (File.Exists(savePath)) {

            SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(savePath)); //Pull Existing File Information

            GameObject[] wavePoints = GameObject.FindGameObjectsWithTag("WavePoint"); // Tag all wavepoints in the scene
            foreach (GameObject wavePoint in wavePoints) { // Disable all active wave points to prevent the player from randomly teleporting upon spawn
                wavePoint.SetActive(false);
            }

            StartCoroutine(AssignDataAfterSceneLoad(saveData));
            return true;
        }

        else {
            GameObject[] wavePoints = GameObject.FindGameObjectsWithTag("WavePoint"); // Tag all wavepoints in the scene
            foreach (GameObject wavePoint in wavePoints) { // Disable all active wave points to prevent the player from randomly teleporting upon spawn
                wavePoint.SetActive(false);
            }
             SaveGame(); //If there isn't a save, start at initial save point
             MapController_Dynamic.Instance?.GenerateMap();
             return false;

         }
    }

    private IEnumerator AssignDataAfterSceneLoad(SaveData saveData)
    {
        yield return null;

        GameObject boundaryObj = GameObject.Find(saveData.mapBoundary);
        if (boundaryObj == null)
        {
            Debug.LogError($"Boundary '{saveData.mapBoundary}' not found");
            yield break;
        }
        PolygonCollider2D polyCollider = boundaryObj.GetComponent<PolygonCollider2D>();
        if (polyCollider == null)
        {
            Debug.LogError($"No PolygonCollider2D on '{saveData.mapBoundary}'");
            yield break;
        }
        CinemachineConfiner confiner = FindFirstObjectByType<CinemachineConfiner>();
        if (confiner != null)
        {
            confiner.m_BoundingShape2D = polyCollider;
            confiner.InvalidatePathCache();
        }

        if (MapController_Dynamic.Instance != null)
        {
            yield return new WaitUntil(() => MapController_Dynamic.Instance.gameObject.activeInHierarchy);
        }
        if (MapController_Dynamic.Instance != null)
        {
            while (MapController_Dynamic.Instance == null || !MapController_Dynamic.Instance.gameObject.activeInHierarchy)
            {
                yield return null;
            }
        }
        
        timeManager = FindObjectOfType<TimeManager>();
        if (timeManager != null)
        {
            string[] timeParts = saveData.currentTime.Split(' ', ':'); 
            if (timeParts.Length >= 3)
            {
                int hour = int.Parse(timeParts[0]);
                int minute = int.Parse(timeParts[1]);
                string period = timeParts[2];
                timeManager.SetTime(hour, minute, period);
            }
        }

        switchForm = FindObjectOfType<SwitchPlayerForm>();
        switchForm.isGhost = saveData.isGhost;
        switchForm.SwitchForm();

        switchForm.canTransform = saveData.canTransform;

        VolumeSettings volumeController = FindObjectOfType<VolumeSettings>();
        volumeController.LoadVolume();

        autoSaveManager.Initialize();
        GameObject.FindGameObjectWithTag("Player").transform.position = saveData.playerPosition;
    }

    public void DeleteSave(int slot)
    {
        string savePath = Path.Combine(saveLocation, $"save_{slot}.json");
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log($"Deleted save in slot {slot}.");
        }
        else
        {
            Debug.Log($"No save found in slot {slot} to delete.");
        }
    }

    public bool SaveExists(int slot)
    {
        string savePath = Path.Combine(saveLocation, $"save_{slot}.json");
        return File.Exists(savePath);
    }

    public SaveData GetSaveData(int slot)
    {
        saveLocation = Path.Combine(Application.persistentDataPath, "Saves");
        string savePath = Path.Combine(saveLocation, $"save_{slot}.json");
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            return JsonUtility.FromJson<SaveData>(json);
        }
        return null; // just return null if no save exists
    }

    public void SetSaveSlot(int activeSlot)
    {
        PlayerPrefs.SetInt(ActiveSlotKey, activeSlot);
        PlayerPrefs.Save();
    }

    public string GetSavedSceneName(int slot)
    {
        string savePath = Path.Combine(saveLocation, $"save_{slot}.json");
        if (!File.Exists(savePath))
        {
            return defaultSaveData.sceneName; // default
        }

        SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(savePath));
        return saveData.sceneName;
    }

    public int GetActiveSlot()
    {
        return PlayerPrefs.GetInt(ActiveSlotKey);
    }
}
