using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCMovement : MonoBehaviour
{
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float movementSpeed = 2f;
    [SerializeField] private bool loop = true; //Loop through waypoints or stop at the end
    [SerializeField] private float waitTime = 2f; //how long to wait at a waypoint before moving again

    private int currentWaypointIndex = 0;
    private bool isWaiting = false; //is idle at a waypoint
    private bool isPaused = false; //paused for interaction with something
    private float resumeMovementTimer = 0f;
    private float pauseDuration = 0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        if (isPaused)
        {
            if (resumeMovementTimer > 0) //pauses while interacting for a set time in NPCInteraction
            {
                resumeMovementTimer -= Time.deltaTime;
                if (resumeMovementTimer <= 0)
                {
                    isPaused = false;
                }
            }
            return;
        }

        if (!isWaiting)
        {
            MoveToWaypoint();
        }
    }
    private void MoveToWaypoint()
    {
        Transform targetWaypoint = waypoints[currentWaypointIndex]; //finds waypoint
        transform.position = Vector3.MoveTowards(transform.position, targetWaypoint.position, movementSpeed * Time.deltaTime); //move towards waypoint

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

        if (currentWaypointIndex >= waypoints.Length) //handles looping or stop
        {
            if (loop) //loop true
            {
                currentWaypointIndex = 0;
            }
            else //loop false
            {
                currentWaypointIndex = waypoints.Length - 1;
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

