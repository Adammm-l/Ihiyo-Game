using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStartPoint : MonoBehaviour
{
    //private PlayerControl thePlayer;
    //private findPlayer theCam;

    // Start is called before the first frame update
    public void Start()
    {
        //thePlayer = FindObjectOfType<PlayerControl>();
        SaveController saveController = SaveController.Instance;
        if (!saveController.isLoadingSave)
        {
            GameObject thePlayer = GameObject.FindGameObjectWithTag("Player");
            thePlayer.transform.position = new Vector3(transform.position.x,transform.position.y,thePlayer.transform.position.z); // Set the player's location
        }

        //theCam = FindObjectOfType<findPlayer>();
        //theCam.transform.position = new Vector3(transform.position.x, transform.position.y, theCam.transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
