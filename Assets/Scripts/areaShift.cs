 using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class areaShift : MonoBehaviour
{
  public int sceneBuilderIndex;

  private void OnTriggerEnter2D(Collider2D other) {
    print ("Area Shift!");
    if (other.tag == "Player") {
        print("Swapping Areas: " + sceneBuilderIndex);
        SceneManager.LoadScene(sceneBuilderIndex, LoadSceneMode.Single);
    }
  }
}
