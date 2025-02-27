using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
// Eri (Edwin)


public class MapTransition : MonoBehaviour
{
    [SerializeField] PolygonCollider2D mapBound;

    CinemachineConfiner confiner;

    [SerializeField] Direction direction;

    [SerializeField] float additivePos = 5f;

    enum Direction { Up, Down, Left, Right }

   
    private static bool camExists;

    private void Awake() { //Runs Script when loaded
        confiner = FindObjectOfType<CinemachineConfiner>();
        

    }


    private void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.CompareTag("Player")) { //If the camera bounds touch the Player
            confiner.m_BoundingShape2D = mapBound; // Swap out new camera bounds
            UpdatePlayerPosition(collision.gameObject); //Teleports Player to new scene

            MapController_Dynamic.Instance?.UpdateCurrentArea(mapBound.name);
        }
    }

    private void UpdatePlayerPosition(GameObject player) {
        Vector3 newPos = player.transform.position;

        switch (direction) { //Cases for teleporting the player via scene transitions
            case Direction.Up:
                newPos.y += additivePos;
                break;
            case Direction.Down:
                newPos.y -= additivePos;
                break;
            case Direction.Left:
                newPos.x += additivePos;
                break;
            case Direction.Right:
                newPos.x -= additivePos;
                break;
        }

        player.transform.position = newPos; //Update the Player's location
    }
}
