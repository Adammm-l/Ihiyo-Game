using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.SceneManagement;

public class findInventory : MonoBehaviour
{
    private GameObject playerInventory;
    // Start is called before the first frame update

    void Start() {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        playerInventory = GameObject.FindGameObjectWithTag("Inventory");

        if(playerInventory != null) {

            //playerCamera.Follow = player.transform;
            //playerCamera.LookAt = player.transform;
            Debug.Log("Camera attached to Player");
        }

        else {
            Debug.LogWarning("Can't find Player");
        }
    }
}
