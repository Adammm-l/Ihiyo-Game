using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SnapMainCamera : MonoBehaviour
{
    // Start is called before the first frame update
    IEnumerator Start()
    {
        yield return null; // Wait a frame
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Camera mainCam  = Camera.main;

        if (player != null && mainCam != null) {
            mainCam.transform.position = player.transform.position + new Vector3(0f,2f,-10f);
        }
        else {
            Debug.LogWarning("Player or Main Camera could NOT be found");
        }
    }


}
