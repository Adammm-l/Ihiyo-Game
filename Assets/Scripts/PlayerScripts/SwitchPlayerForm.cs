using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class SwitchPlayerForm : MonoBehaviour
{
    public bool isGhost = false;
    public bool isPossessing = false;
    public bool isInsideGhostPassable = false;

    [Header("Ghost Properties")]
    public float ghostSpeed = 3f;
    public Color ghostColor;
    public Color possessableColor;
    public LayerMask ghostPossessableLayer;
    public LayerMask ghostPassableLayer;
    public float possessRange = 3f;

    [Header("Physical Properties")]
    public float physicalSpeed = 2f;
    Renderer playerRenderer;

    [Header("References")]
    public CinemachineVirtualCamera cmVirtualCamera;
    Rigidbody2D rb;
    BoxCollider2D playerCollider;
    PlayerControl playerMovement;
    GameObject ghostIndicator;
    Transform ghostStateIcon;
    Transform humanStateIcon;

    [Header("Keybinds")]
    KeybindManager keybindManager;
    KeyCode switchForm;
    KeyCode possessKey = KeyCode.P;

    [Header("Possession Settings")]
    public float maxPossessionDistance = 5f;

    public GameObject possessedObject;
    Renderer[] possessableRenderers;
    Color[] originalColors;
    
    // passable objects
    List<Renderer> allPassableRenderers = new List<Renderer>();
    List<Color> allPassableOriginalColors = new List<Color>();
    
    // possessable objects
    Dictionary<Renderer, Color> allPossessableRenderers = new Dictionary<Renderer, Color>();
    List<Renderer> currentPossessableInRange = new List<Renderer>();

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerRenderer = GetComponent<Renderer>();
        playerCollider = GetComponent<BoxCollider2D>();

        if (playerCollider == null)
        {
            Debug.LogError("No BoxCollider2D found on player object!");
            enabled = false;
            return;
        }

        keybindManager = KeybindManager.Instance;
        playerMovement = GetComponent<PlayerControl>();

        
        FindAllPassableObjects();
        FindAllPossessableObjects();
    }

    void FindAllPassableObjects()
    {
        allPassableRenderers.Clear();
        allPassableOriginalColors.Clear();
        
        Renderer[] allRenderers = FindObjectsOfType<Renderer>();
        
        foreach (Renderer renderer in allRenderers)
        {
            if ((ghostPassableLayer.value & (1 << renderer.gameObject.layer)) != 0)
            {
                allPassableRenderers.Add(renderer);
                allPassableOriginalColors.Add(renderer.material.color);
            }
        }
    }

    void FindAllPossessableObjects()
    {
        Collider2D[] allPossessableColliders = Physics2D.OverlapCircleAll(transform.position, 1000f, ghostPossessableLayer);
        foreach (Collider2D collider in allPossessableColliders)
        {
            Renderer r = collider.GetComponent<Renderer>();
            if (r != null && !allPossessableRenderers.ContainsKey(r))
            {
                allPossessableRenderers.Add(r, r.material.color);
            }
        }
    }

    void Update()
    {
        switchForm = keybindManager.GetKeybind("SwitchForm");

        if (Input.GetKeyDown(switchForm))
        {
            TryToggleForm();
        }

        if (isGhost && !isPossessing)
        {
            UpdatePossessableObjects();
            HighlightPossessableObjects();
            ResetOutOfRangePossessableObjects();

            if (Input.GetKeyDown(possessKey))
            {
                TryPossessObject();
            }
        }
        else if (isPossessing && Input.GetKeyDown(possessKey))
        {
            ReleaseObject();
        }

        if (isGhost && playerCollider != null)
        {
            CheckGhostPassableCollision();
        }

        if (isPossessing)
        {
            // auto-release distance check
            if (Vector2.Distance(transform.position, possessedObject.transform.position) > maxPossessionDistance)
            {
                ReleaseObject();
                Debug.Log("Released object: Too far away");
            }
        }
    }

    void ResetOutOfRangePossessableObjects()
    {
        foreach (KeyValuePair<Renderer, Color> kvp in allPossessableRenderers)
        {
            if (!currentPossessableInRange.Contains(kvp.Key))
            {
                if (kvp.Key != null)
                {
                    kvp.Key.material.color = kvp.Value;
                }
            }
        }
    }

    void CheckGhostPassableCollision()
    {
        Collider2D[] overlappingColliders = Physics2D.OverlapBoxAll(
            playerCollider.bounds.center,
            playerCollider.bounds.size,
            0f,
            ghostPassableLayer
        );

        isInsideGhostPassable = overlappingColliders.Length > 0;
    }

    void TryToggleForm()
    {
        if (isGhost && isInsideGhostPassable)
        {
            Debug.Log("Cannot return to physical form while inside a ghost-passable object!");
            return;
        }

        ToggleForm();
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
        return isGhost ? ghostSpeed : physicalSpeed;
    }

    void EnterGhostForm()
    {
        Debug.Log("Swapping to Ghost Form");
        playerRenderer.material.color = ghostColor;

        string passableLayer = GetLayerNameFromLayerMask(ghostPassableLayer);
        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer(passableLayer), true);
        
        HighlightAllPassableObjects();
        UpdatePossessableObjects();

        ghostIndicator = GameObject.Find("GhostIndicatorCanvas/GhostIndicatorHUD");
        ghostStateIcon = ghostIndicator.transform.GetChild(0);
        humanStateIcon = ghostIndicator.transform.GetChild(1);

        ghostStateIcon.gameObject.SetActive(true);
        humanStateIcon.gameObject.SetActive(false);
    }

    void EnterPhysicalForm()
    {
        Debug.Log("Returning to Physical Form");
        playerRenderer.material.color = Color.white;

        string passableLayer = GetLayerNameFromLayerMask(ghostPassableLayer);
        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer(passableLayer), false);
        
        ResetAllPassableObjectColors();
        ResetAllPossessableObjectColors();
        ReleaseObject();

        ghostIndicator = GameObject.Find("GhostIndicatorCanvas/GhostIndicatorHUD");
        ghostStateIcon = ghostIndicator.transform.GetChild(0);
        humanStateIcon = ghostIndicator.transform.GetChild(1);
        
        ghostStateIcon.gameObject.SetActive(false);
        humanStateIcon.gameObject.SetActive(true);
    }

    string GetLayerNameFromLayerMask(LayerMask layerMask)
    {
        int layerNumber = (int)Mathf.Log(layerMask.value, 2);
        return LayerMask.LayerToName(layerNumber);
    }

    void HighlightAllPassableObjects()
    {
        for (int i = 0; i < allPassableRenderers.Count; i++)
        {
            if (allPassableRenderers[i] != null)
            {
                allPassableRenderers[i].material.color = Color.Lerp(
                    allPassableOriginalColors[i], 
                    ghostColor, 
                    0.7f
                );
            }
        }
    }

    void ResetAllPassableObjectColors()
    {
        for (int i = 0; i < allPassableRenderers.Count; i++)
        {
            if (allPassableRenderers[i] != null)
            {
                allPassableRenderers[i].material.color = allPassableOriginalColors[i];
            }
        }
    }

    void UpdatePossessableObjects()
    {
        Collider2D[] possessableColliders = Physics2D.OverlapCircleAll(transform.position, possessRange, ghostPossessableLayer);
        currentPossessableInRange.Clear();
        
        // initialize empty arrays if no objects found
        possessableRenderers = new Renderer[possessableColliders.Length];
        originalColors = new Color[possessableColliders.Length];

        if (possessableColliders.Length > 0)
        {
            // find closest object
            GameObject closestObject = null;
            float closestDistance = Mathf.Infinity;
            
            foreach (Collider2D collider in possessableColliders)
            {
                float distance = Vector2.Distance(transform.position, collider.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestObject = collider.gameObject;
                }
            }

            // only process the closest object
            if (closestObject != null)
            {
                Renderer r = closestObject.GetComponent<Renderer>();
                if (r != null)
                {
                    possessableRenderers[0] = r;
                    originalColors[0] = allPossessableRenderers.ContainsKey(r) ? allPossessableRenderers[r] : r.material.color;
                    currentPossessableInRange.Add(r);
                    
                    if (!allPossessableRenderers.ContainsKey(r))
                    {
                        allPossessableRenderers.Add(r, originalColors[0]);
                    }
                }
            }
        }
    }

    void HighlightPossessableObjects()
    {
        // check if array exists and has at least one element
        if (possessableRenderers != null && possessableRenderers.Length > 0 && possessableRenderers[0] != null)
        {
            // only highlight the closest object
            possessableRenderers[0].material.color = Color.Lerp(
                originalColors[0], 
                possessableColor, 
                0.7f
            );
        }
    }

    void TryPossessObject()
    {
        Collider2D[] possessableColliders = Physics2D.OverlapCircleAll(transform.position, possessRange, ghostPossessableLayer);
        if (possessableColliders.Length > 0)
        {
            // Find the closest object to possess
            GameObject closestObject = null;
            float closestDistance = Mathf.Infinity;
            
            foreach (Collider2D collider in possessableColliders)
            {
                float distance = Vector2.Distance(transform.position, collider.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestObject = collider.gameObject;
                }
            }

            possessedObject = closestObject;
            playerMovement.canMove = false;
            isPossessing = true;

            // set up possession
            var possessedMovement = possessedObject.GetComponent<PossessedObjectMovement>();
            if (possessedMovement == null)
            {
                possessedMovement = possessedObject.AddComponent<PossessedObjectMovement>();
            }
            possessedMovement.Initialize(this);

            // camera and player setup
            if (cmVirtualCamera != null)
            {
                cmVirtualCamera.Follow = possessedObject.transform;
            }
            
            // ignore collision between player and object
            Physics2D.IgnoreCollision(playerCollider, possessedObject.GetComponent<Collider2D>(), true);
        }
    }

    void ResetAllPossessableObjectColors()
    {
        foreach (KeyValuePair<Renderer, Color> kvp in allPossessableRenderers)
        {
            if (kvp.Key != null)
            {
                kvp.Key.material.color = kvp.Value;
            }
        }
    }

    public void ReleaseObject()
    {
        if (possessedObject != null)
        {
            var possessedMovement = possessedObject.GetComponent<PossessedObjectMovement>();
            if (possessedMovement != null)
            {
                possessedMovement.enabled = false;
            }

            // restore collisions
            Physics2D.IgnoreCollision(playerCollider, possessedObject.GetComponent<Collider2D>(), false);

            // Restore player control
            if (cmVirtualCamera != null) 
            {
                cmVirtualCamera.Follow = transform;
            }

            playerMovement.enabled = true;
            possessedObject = null;
            isPossessing = false;
            playerMovement.canMove = true;
        }
    }
}