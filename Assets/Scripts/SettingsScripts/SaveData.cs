using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Eri and Terrence

[System.Serializable]

public class SaveData
{
    public Vector3 playerPosition;
    public string mapBoundary; //Boundary Name for Map
    public string saveName;
    public int currentDay;
    public bool isNight; // connect currentday and isnight to time system
    
    // probably need to add time, dialogue, completed quests, items, 
    // a whole bunch of other stuff now that i think about it
}
