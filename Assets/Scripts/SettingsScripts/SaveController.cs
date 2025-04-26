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

    public static SaveController Instance;
    //private static bool saveExists; // All instances of this Player references the exact same variable

    // Start is called before the first frame update

    void Awake() 
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            DestroyImmediate(gameObject);
            return;
        }
    }
    void Start()
    {
        saveLocation = Path.Combine(Application.persistentDataPath, "Saves");
        Directory.CreateDirectory(saveLocation);

        defaultSaveData = new SaveData
        {
            playerPosition = new Vector3(-5.5f, 1.25f, 0f),
            sceneName = "Ihi_House",
            mapBoundary = "Ihi_Room",
            currentDay = 1,
            isNight = false,
        };
    }

    public void SaveGame() 
    {
        int activeSaveSlot = PlayerPrefs.GetInt(ActiveSlotKey);
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
        saveData.playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position;
        saveData.sceneName = SceneManager.GetActiveScene().name;
        saveData.mapBoundary = FindObjectOfType<CinemachineConfiner>().m_BoundingShape2D.gameObject.name;

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
            currentDay = defaultSaveData.currentDay,
            isNight = defaultSaveData.isNight,
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

            GameObject.FindGameObjectWithTag("Player").transform.position = saveData.playerPosition; //Sets Value of Player Position

            StartCoroutine(AssignConfinerAfterSceneLoad(saveData));

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

    private IEnumerator AssignConfinerAfterSceneLoad(SaveData saveData)
    {
        yield return null;
        yield return null;

        GameObject boundaryObj = null;
        int attempts = 0;
        while (boundaryObj == null && attempts < 5)
        {
            boundaryObj = GameObject.Find(saveData.mapBoundary);
            attempts++;
            if (boundaryObj == null) 
            {
                yield return null;
            }
        }

        if (boundaryObj == null)
        {
            Debug.LogError($"Boundary '{saveData.mapBoundary}' not found after {attempts} attempts");
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
            while (MapController_Dynamic.Instance == null || !MapController_Dynamic.Instance.gameObject.activeInHierarchy)
            {
                yield return null;
            }
        }

        VolumeSettings volumeController = FindObjectOfType<VolumeSettings>();
        volumeController.LoadVolume();
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
}
