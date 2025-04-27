using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

//Adam
public class TimeBasedObjectManager : MonoBehaviour
{
    [System.Serializable]
    public class ManagedTimeObject
    {
        public GameObject targetObject;
        public int startHour;
        public int startMinute;
        public int endHour;
        public int endMinute;
        public bool activeInRange = true;
    }

    [SerializeField] private List<ManagedTimeObject> managedObjects = new List<ManagedTimeObject>();

    private TimeManager timeManager;

    private void Start()
    {
        timeManager = TimeManager.Instance;
        UpdateAllObjects();
    }

    private void Update()
    {
        UpdateAllObjects();
    }

    private void UpdateAllObjects()
    {
        int currentHour = timeManager.GetHour();
        int currentMinute = timeManager.GetMinute();

        foreach (var obj in managedObjects)
        {
            if (obj.targetObject != null)
            {
                bool isInTimeRange = IsWithinTimeRange(currentHour, currentMinute, obj);
                bool shouldBeActive = obj.activeInRange ? isInTimeRange : !isInTimeRange;

                if (obj.targetObject.activeSelf != shouldBeActive)
                {
                    obj.targetObject.SetActive(shouldBeActive);
                }
            }
        }
    }

    private bool IsWithinTimeRange(int hour, int minute, ManagedTimeObject obj)
    {
        int currentTimeInMinutes = hour * 60 + minute;
        int startTimeInMinutes = obj.startHour * 60 + obj.startMinute;
        int endTimeInMinutes = obj.endHour * 60 + obj.endMinute;

        return (currentTimeInMinutes >= startTimeInMinutes && currentTimeInMinutes < endTimeInMinutes);
    }

    public void AddManagedObject(GameObject target, int startHour, int startMinute, int endHour, int endMinute, bool activeInRange = true)
    {
        var newObj = new ManagedTimeObject
        {
            targetObject = target,
            startHour = startHour,
            startMinute = startMinute,
            endHour = endHour,
            endMinute = endMinute,
            activeInRange = activeInRange
        };
        managedObjects.Add(newObj);
    }
}