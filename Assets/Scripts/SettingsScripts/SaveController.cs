using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System.IO;
// Terrence and Edwin (Eri)
public class SaveController : MonoBehaviour // Terrence Akinola / Edwin (Eri) Sotres
{
    public const int MAX_SLOTS = 3;
    const string ActiveSlotKey = "ActiveSaveSlot";
    private string saveLocation;
    Vector3 defaultPlayerPosition = new Vector3(-5.5f, 1.25f, 0f);

    // Start is called before the first frame update
    void Start()
    {
        saveLocation = Path.Combine(Application.persistentDataPath, "Saves");
        Directory.CreateDirectory(saveLocation);
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
        saveData.mapBoundary = FindObjectOfType<CinemachineConfiner>().m_BoundingShape2D.gameObject.name;

        File.WriteAllText(savePath, JsonUtility.ToJson(saveData)); //Writes the Data to a file
        Debug.Log($"Saved game on slot {activeSaveSlot}.");
    }

    public void CreateSave(int slot, string name) 
    {
        string savePath = Path.Combine(saveLocation, $"save_{slot}.json");
        
        SaveData saveData = new SaveData 
        {
            playerPosition = defaultPlayerPosition,
            mapBoundary = "Ihi_Room",
            saveName = name,
            currentDay = 1,
            isNight = false
        };

        File.WriteAllText(savePath, JsonUtility.ToJson(saveData)); //Writes the Data to a file
    }

    public bool LoadGame(int slot) {
        string savePath = Path.Combine(saveLocation, $"save_{slot}.json");
        SetSaveSlot(slot); // remember active save slot for saving ingame
        if (File.Exists(savePath)) {

            SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(savePath)); //Pull Existing File Information

            GameObject.FindGameObjectWithTag("Player").transform.position = saveData.playerPosition; //Sets Value of Player Position

            PolygonCollider2D savedMapBoundary = GameObject.Find(saveData.mapBoundary).GetComponent<PolygonCollider2D>();

            FindObjectOfType<CinemachineConfiner>().m_BoundingShape2D = savedMapBoundary;

            MapController_Dynamic.Instance?.GenerateMap(savedMapBoundary);

            return true;
        }

        else {

             SaveGame(); //If there isn't a save, start at initial save point
             MapController_Dynamic.Instance?.GenerateMap();
             return false;

         }
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
}
