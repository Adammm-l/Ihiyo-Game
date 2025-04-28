using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TimeScheduleEntry
{
    public int startHour; // 24-hour format
    public int startMinute;
    public Transform[] waypoints;
}

public class NPCMovement : MonoBehaviour
{
    [SerializeField] private TimeScheduleEntry[] schedule;
    [SerializeField] private float movementSpeed = 2f;
    [SerializeField] private bool loop = true;
    [SerializeField] private float waitTime = 2f;

    private Transform[] currentWaypoints;
    private int currentWaypointIndex = 0;
    private bool isWaiting = false;
    private bool isPaused = false;
    private float resumeMovementTimer = 0f;
    private float pauseDuration = 0f;
    private TimeScheduleEntry currentSchedule;

    private void Start()
    {
        UpdateScheduleBasedOnTime();
    }

    private void Update()
    {
        UpdateScheduleBasedOnTime();

        if (isPaused)
        {
            if (resumeMovementTimer > 0)
            {
                resumeMovementTimer -= Time.deltaTime;
                if (resumeMovementTimer <= 0)
                {
                    isPaused = false;
                }
            }
            return;
        }

        if (!isWaiting && currentWaypoints != null && currentWaypoints.Length > 0)
        {
            MoveToWaypoint();
        }
    }

    private void UpdateScheduleBasedOnTime()
    {
        if (TimeManager.Instance == null) return;

        int currentHour = TimeManager.Instance.GetHour();
        int currentMinute = TimeManager.Instance.GetMinute();

        TimeScheduleEntry bestMatch = null;
        int bestMatchTime = -1;

        foreach (var entry in schedule)
        {
            int entryTimeValue = entry.startHour * 60 + entry.startMinute;
            int currentTimeValue = currentHour * 60 + currentMinute;

            // Find the most recent schedule that's not in the future
            if (entryTimeValue <= currentTimeValue && entryTimeValue > bestMatchTime)
            {
                bestMatch = entry;
                bestMatchTime = entryTimeValue;
            }
        }

        // If we found a matching schedule and it's different from the current one
        if (bestMatch != null && bestMatch != currentSchedule)
        {
            currentSchedule = bestMatch;
            currentWaypoints = bestMatch.waypoints;

            // Reset waypoint index if the waypoints changed
            if (currentWaypointIndex >= currentWaypoints.Length)
            {
                currentWaypointIndex = 0;
                isWaiting = false;
            }
        }
    }

    private void MoveToWaypoint()
    {
        Transform targetWaypoint = currentWaypoints[currentWaypointIndex];
        transform.position = Vector3.MoveTowards(transform.position, targetWaypoint.position, movementSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetWaypoint.position) < 0.1f)
        {
            StartCoroutine(WaitAtWaypoint());
        }
    }

    private IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTime);
        isWaiting = false;
        currentWaypointIndex++;

        if (currentWaypointIndex >= currentWaypoints.Length)
        {
            if (loop)
            {
                currentWaypointIndex = 0;
            }
            else
            {
                currentWaypointIndex = currentWaypoints.Length - 1;
                isWaiting = true;
            }
        }
    }

    public void PauseMovementWithTimer(float duration)
    {
        isPaused = true;
        pauseDuration = duration;
        resumeMovementTimer = duration;
    }

    public void PauseMovementInfinitely()
    {
        isPaused = true;
    }

    public void ResumeMovement()
    {
        isPaused = false;
    }
}