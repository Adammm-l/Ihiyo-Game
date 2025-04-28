using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class TimeManipulator : MonoBehaviour
{
    public bool canChangeTime;
    GameObject timeButton;

    void Start()
    {
        SaveController saveManager = FindObjectOfType<SaveController>();
        int activeSlot = saveManager.GetActiveSlot();
        SaveData saveData = saveManager.GetSaveData(activeSlot);

        canChangeTime = saveData.canChangeTime;

        Transform child = transform.Find("TimeManipulatorButton");
        if (child != null)
        {
            timeButton = child.gameObject;
        }
    }

    void Update()
    {
        if (timeButton != null)
        {
            timeButton.SetActive(canChangeTime);
        }
    }
}
