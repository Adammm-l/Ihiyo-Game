using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System.IO;

public class SaveController : MonoBehaviour
{
    
    private string saveLocation;
    // Start is called before the first frame update
    void Start()
    {
        saveLocation = Path.Combine(Application.persistentDataPath, "saveData.json");
        LoadGame();
    }

    

    public void SaveGame() {
        SaveData saveData = new SaveData {

            playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position, //Connect Player position

            mapBoundary = FindObjectOfType<CinemachineConfiner>().m_BoundingShape2D.gameObject.name
        };

        File.WriteAllText(saveLocation,JsonUtility.ToJson(saveData)); //Writes the Data to a file
    }


    public void LoadGame() {
        if (File.Exists(saveLocation)) {

            SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveLocation)); //Pull Existing File Information

            GameObject.FindGameObjectWithTag("Player").transform.position = saveData.playerPosition; //Sets Value of Player Position

            FindObjectOfType<CinemachineConfiner>().m_BoundingShape2D = GameObject.Find(saveData.mapBoundary).GetComponent<PolygonCollider2D>();

        }

        else {

            SaveGame(); //If there isn't a save, start at initial save point

        }
    }

}
