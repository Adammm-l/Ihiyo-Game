using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.SceneManagement;

public class findPlayer : MonoBehaviour
{
    private CinemachineVirtualCamera playerCamera;
    private Transform defaultFollowTarget; // Stores the default follow target (player)

    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(transform.gameObject);
    }

    private void Awake()
    {
        playerCamera = GetComponent<CinemachineVirtualCamera>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            // Set the default follow target to the player
            defaultFollowTarget = player.transform;
            playerCamera.Follow = defaultFollowTarget;
            playerCamera.LookAt = defaultFollowTarget;
            Camera.main.transform.position = defaultFollowTarget.position + new Vector3(0f,2f,-10f); 
            Debug.Log("Camera attached to Player");

            //Snap the Main Camera to the player's position with default camera offset

            
        }
        else
        {
            Debug.LogWarning("Can't find Player");
        }
    }

    // Public method to temporarily change the camera's follow target
    public void SetCameraTarget(Transform newTarget)
    {
        playerCamera.Follow = newTarget;
        playerCamera.LookAt = newTarget;
    }

    // Public method to reset the camera's follow target to the player
    public void ResetCameraTarget()
    {
        playerCamera.Follow = defaultFollowTarget;
        playerCamera.LookAt = defaultFollowTarget;
    }
}