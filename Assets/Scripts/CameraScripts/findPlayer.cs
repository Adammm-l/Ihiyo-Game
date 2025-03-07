using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.SceneManagement;

public class findPlayer : MonoBehaviour
{
    private CinemachineVirtualCamera playerCamera;
    // Start is called before the first frame update
    void Start() {
        DontDestroyOnLoad(transform.gameObject);
    }

    private void Awake() {
        playerCamera = GetComponent<CinemachineVirtualCamera>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        GameObject player = GameObject.FindGameObjectWithTag("Player"); 

        if(player != null) {

            playerCamera.Follow = player.transform;
            playerCamera.LookAt = player.transform;
            Debug.Log("Camera attached to Player");
        }

        else {
            Debug.LogWarning("Can't find Player");
        }
    }
}
