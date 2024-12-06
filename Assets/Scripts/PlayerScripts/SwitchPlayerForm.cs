using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwitchPlayerForm : MonoBehaviour
{
    public bool isGhost = false;

    [Header("Ghost Properties")]
    public float ghostSpeed = 3f;
    public float ghostOpacity = 0.5f;
    public LayerMask ghostLayer;

    [Header("Physical Properties")]
    public float physicalSpeed = 2f;
    Renderer playerRenderer;

    [Header("References")]
    public GameObject keybindHolder;
    Rigidbody rb;
    Collider playerCollider;

    [Header("Keybinds")]
    KeybindManager keybindManager;
    KeyCode switchForm;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerRenderer = GetComponent<Renderer>();
        playerCollider = GetComponent<Collider>();
        keybindManager = keybindHolder.GetComponent<KeybindManager>();
    }

    // Update is called once per frame
    void Update()
    {
        switchForm = keybindManager.GetKeybind("SwitchForm");
        if (Input.GetKeyDown(switchForm))
        {
            ToggleForm();
        }
    }

    void ToggleForm()
    {
        isGhost = !isGhost;
        if (isGhost)
        {
            EnterGhostForm();
        }
        else
        {
            EnterPhysicalForm();
        }
    }

    public float GetPlayerSpeed()
    {
        if (isGhost)
        {
            return ghostSpeed;
        }
        else
        {
            return physicalSpeed;
        }
    }
    void EnterGhostForm()
    {
        Debug.Log("Swapping to Ghost Form");
        Color color = playerRenderer.material.color;
        color.a = ghostOpacity; // modify transparency
        playerRenderer.material.color = color;

        Physics.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Default"), true);
    }

    void EnterPhysicalForm()
    {
        Debug.Log("Returning to Physical Form");
        
        Color color = playerRenderer.material.color;
        color.a = 1f; // reset transparency
        playerRenderer.material.color = color;  

        // enables collision with passable objects again
        int playerLayer = LayerMask.NameToLayer("Player");
        int ghostLayer = LayerMask.NameToLayer("GhostPassable");
        Physics.IgnoreLayerCollision(playerLayer, ghostLayer, false);
    }
}
