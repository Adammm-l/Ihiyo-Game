using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStartPoint : MonoBehaviour
{
    //private PlayerControl thePlayer;
    //private findPlayer theCam;

    // Start is called before the first frame update
    void Start()
    {
        //thePlayer = FindObjectOfType<PlayerControl>();
        GameObject thePlayer = GameObject.FindGameObjectWithTag("Player");
        thePlayer.transform.position = new Vector3(transform.position.x,transform.position.y,thePlayer.transform.position.z); // Set the player's location 

        //theCam = FindObjectOfType<findPlayer>();
        //theCam.transform.position = new Vector3(transform.position.x, transform.position.y, theCam.transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
