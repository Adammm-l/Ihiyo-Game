using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class TimeManipulator : MonoBehaviour
{
    public bool canChangeTime;
    void Awake()
    {
        SaveController saveManager = FindObjectOfType<SaveController>();
        int activeSlot = saveManager.GetActiveSlot();
        SaveData saveData = saveManager.GetSaveData(activeSlot);

        canChangeTime = saveData.canChangeTime;
    }

    void Update()
    {
        if (canChangeTime)
        {
            this.gameObject.SetActive(true);
        }
        else
        {
            this.gameObject.SetActive(false);
        }
    }
}
