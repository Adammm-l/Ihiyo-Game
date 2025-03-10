using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class SwitchPlayerForm : MonoBehaviour
{
    public bool isGhost = false;
    public bool isPossessing = false;

    [Header("Ghost Properties")]
    public float ghostSpeed = 3f;
    public Color ghostColor = new Color(0.5f, 0.8f, 1f, 0.5f);
    public LayerMask ghostLayer;
    public LayerMask ghostPossessableLayer;
    public float possessRange = 3f;

    [Header("Physical Properties")]
    public float physicalSpeed = 2f;
    Renderer playerRenderer;

    [Header("References")]
    public GameObject keybindHolder;
    public CinemachineVirtualCamera cmVirtualCamera;
    Rigidbody rb;
    Collider playerCollider;
    PlayerControl playerMovement;

    [Header("Keybinds")]
    KeybindManager keybindManager;
    KeyCode switchForm;
    KeyCode possessKey = KeyCode.P;

    GameObject possessedObject;
    Renderer[] possessableRenderers;
    Color[] originalColors;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerRenderer = GetComponent<Renderer>();
        playerCollider = GetComponent<Collider>();

        keybindManager = keybindHolder.GetComponent<KeybindManager>();
        playerMovement = GetComponent<PlayerControl>();
        UpdatePossessableObjects();
    }

    // Update is called once per frame
    void Update()
    {
        switchForm = keybindManager.GetKeybind("SwitchForm");
        if (Input.GetKeyDown(switchForm))
        {
            ToggleForm();
        }

        if (isGhost && !isPossessing) // handle possessing objects
        {
            HighlightPossessableObjects();

            if (Input.GetKeyDown(possessKey))
            {
                TryPossessObject();
            }
        }
        else if (isPossessing && Input.GetKeyDown(possessKey)) // handle releasing possessed objects
        {
            ReleaseObject();
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
        playerRenderer.material.color = ghostColor;

        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("GhostPassable"), true);
        UpdatePossessableObjects();
    }

    void EnterPhysicalForm()
    {
        Debug.Log("Returning to Physical Form");
        playerRenderer.material.color = Color.white;

        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("GhostPassable"), false);
        ResetPossessableObjectColors();
        ReleaseObject();
    }

    void UpdatePossessableObjects()
    {
        // find objects on the ghostPossessable layer
        Collider2D[] possessableColliders = Physics2D.OverlapCircleAll(transform.position, possessRange, ghostPossessableLayer);
        possessableRenderers = new Renderer[possessableColliders.Length];
        originalColors = new Color[possessableColliders.Length];

        for (int i = 0; i < possessableColliders.Length; i++)
        {
            possessableRenderers[i] = possessableColliders[i].GetComponent<Renderer>();
            if (possessableRenderers[i] != null)
            {
                originalColors[i] = possessableRenderers[i].material.color;
            }
        }
    }

    void HighlightPossessableObjects()
    {
        if (possessableRenderers != null)
        {
            foreach (Renderer renderer in possessableRenderers)
            {
                if (renderer != null)
                {
                    renderer.material.color = Color.Lerp(originalColors[System.Array.IndexOf(possessableRenderers, renderer)], ghostColor, 0.5f); // apply blue tint
                }
            }
        }
    }

    void ResetPossessableObjectColors()
    {
        if (possessableRenderers != null && originalColors != null)
        {
            for (int i = 0; i < possessableRenderers.Length; i++)
            {
                if (possessableRenderers[i] != null)
                {
                    possessableRenderers[i].material.color = originalColors[i]; // reset to original color
                }
            }
        }
    }

    void TryPossessObject()
    {
        Collider2D[] possessableColliders = Physics2D.OverlapCircleAll(transform.position, possessRange, ghostPossessableLayer);
        if (possessableColliders.Length > 0)
        {
            // possess first object in range
            possessedObject = possessableColliders[0].gameObject;
            isPossessing = true;

            if (cmVirtualCamera != null)
            {
                cmVirtualCamera.Follow = possessedObject.transform; // switch camera focus to the possessed object
                Debug.Log($"Possessed {possessedObject.name}. Camera target set to {possessedObject.name}");
            }
            else
            {
                Debug.LogError("CinemachineVirtualCamera not assigned.");
            }

            // disable player movement
            if (playerMovement != null)
            {
                playerMovement.enabled = false;
            }

            // enable movement for possessed object
            PossessedObjectMovement possessedMovement = possessedObject.GetComponent<PossessedObjectMovement>();
            if (possessedMovement == null)
            {
                possessedMovement = possessedObject.AddComponent<PossessedObjectMovement>();
            }
            possessedMovement.enabled = true;
        }
        else
        {
            Debug.Log("No possessable objects in range.");
        }
    }

    void ReleaseObject()
    {
        if (possessedObject != null)
        {
            if (cmVirtualCamera != null)
            {
                cmVirtualCamera.Follow = transform; // switch camera focus back to the player
                Debug.Log($"Released {possessedObject.name}. Camera target reset to player.");
            }
            else
            {
                Debug.LogError("CinemachineVirtualCamera not assigned.");
            }

            // re-enable player movement
            if (playerMovement != null)
            {
                playerMovement.enabled = true;
            }

            // disable movement for possessed object
            PossessedObjectMovement possessedMovement = possessedObject.GetComponent<PossessedObjectMovement>();
            if (possessedMovement != null)
            {
                possessedMovement.enabled = false;
            }

            possessedObject = null;
            isPossessing = false;
        }
    }

    // void OnDrawGizmosSelected()
    // {
    //     // draw the possess range in the editor
    //     Gizmos.color = Color.blue;
    //     Gizmos.DrawWireSphere(transform.position, possessRange);
    // }
}