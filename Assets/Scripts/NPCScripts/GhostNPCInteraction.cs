using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GhostNPCInteraction : MonoBehaviour
{
    [Header("NPC Thoughts")]
    [SerializeField] private string npcName;
    [SerializeField] private List<string> thoughtFragments;

    [Header("UI Elements")]
    [SerializeField] private GameObject thoughtBubbleUI;
    [SerializeField] private RectTransform spawnArea;
    [SerializeField] private GameObject thoughtTextPrefab;
    [SerializeField] private TextMeshProUGUI titleText;

    [Header("References")]
    [SerializeField] private GameObject interactionTextGhost;
    [SerializeField] private GameObject keybindHolder;

    [Header("Thought Animation Settings")]
    [SerializeField] private float minSpawnInterval = 0.5f;
    [SerializeField] private float maxSpawnInterval = 2.0f;
    [SerializeField] private float fadeInTime = 0.5f;
    [SerializeField] private float stayTime = 1.5f;
    [SerializeField] private float fadeOutTime = 0.7f;

    private bool isPlayerInRange = false;
    private bool isShowingThoughts = false;
    private KeybindManager keybindManager;
    private KeyCode interactKey;
    private PlayerControl playerControl;
    private SwitchPlayerForm playerForm;
    private NPCMovement npcMovement;
    private List<GameObject> activeThoughts = new List<GameObject>();
    private Coroutine thoughtsRoutine;

    void Start()
    {
        keybindManager = keybindHolder.GetComponent<KeybindManager>();
        npcMovement = GetComponentInParent<NPCMovement>();

        // Set up ghost interaction text
        if (interactionTextGhost != null)
        {
            TextMeshProUGUI interactionTextBox = interactionTextGhost.GetComponent<TextMeshProUGUI>();
            interactKey = keybindManager.GetKeybind("Interact");
            interactionTextBox.text = $"\"{interactKey}\" to read thoughts";
            interactionTextGhost.SetActive(false);
        }

        // Make sure thoughts UI is hidden at start
        if (thoughtBubbleUI != null)
        {
            thoughtBubbleUI.SetActive(false);
        }
    }

    void Update()
    {
        if (!isPlayerInRange) return;

        // Get player references if we don't have them
        if (playerControl == null)
        {
            playerControl = FindObjectOfType<PlayerControl>();
            if (playerControl != null)
            {
                playerForm = playerControl.GetComponent<SwitchPlayerForm>();
            }
        }

        // Update interact key in case keybinds changed
        interactKey = keybindManager.GetKeybind("Interact");

        // Only show ghost interaction text when player is in ghost form
        if (playerForm != null && playerForm.isGhost)
        {
            if (interactionTextGhost != null)
            {
                interactionTextGhost.SetActive(true);
            }

            // Handle ghost interaction
            if (Input.GetKeyDown(interactKey) && !isShowingThoughts)
            {
                ShowThoughts();
            }
            else if (Input.GetKeyDown(interactKey) && isShowingThoughts)
            {
                HideThoughts();
            }
        }
        else
        {
            if (interactionTextGhost != null)
            {
                interactionTextGhost.SetActive(false);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;

            // Get player references
            playerControl = other.GetComponent<PlayerControl>();
            if (playerControl != null)
            {
                playerForm = playerControl.GetComponent<SwitchPlayerForm>();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            HideThoughts();

            if (interactionTextGhost != null)
            {
                interactionTextGhost.SetActive(false);
            }
        }
    }

    private void ShowThoughts()
    {
        if (playerForm == null || !playerForm.isGhost) return;

        // Stop player movement and NPC movement
        playerControl.canMove = false;
        if (npcMovement != null)
        {
            npcMovement.PauseMovementInfinitely();
        }

        // Set the title text
        if (titleText != null)
        {
            titleText.text = $"{npcName}'s Thoughts";
            titleText.gameObject.SetActive(true);
        }

        // Show the thought UI
        if (thoughtBubbleUI != null)
        {
            thoughtBubbleUI.SetActive(true);
            isShowingThoughts = true;

            // Start spawning random thoughts
            thoughtsRoutine = StartCoroutine(SpawnThoughtsRoutine());
        }
    }

    private void HideThoughts()
    {
        // Stop spawning thoughts
        if (thoughtsRoutine != null)
        {
            StopCoroutine(thoughtsRoutine);
        }

        // Destroy all active thoughts
        foreach (GameObject thought in activeThoughts)
        {
            Destroy(thought);
        }
        activeThoughts.Clear();

        if (thoughtBubbleUI != null)
        {
            thoughtBubbleUI.SetActive(false);
        }

        // Resume player and NPC movement
        if (playerControl != null)
        {
            playerControl.canMove = true;
        }

        if (npcMovement != null)
        {
            npcMovement.ResumeMovement();
        }

        isShowingThoughts = false;
    }

    private IEnumerator SpawnThoughtsRoutine()
    {
        while (isShowingThoughts)
        {
            SpawnRandomThought();
            yield return new WaitForSeconds(Random.Range(minSpawnInterval, maxSpawnInterval));
        }
    }

    private void SpawnRandomThought()
    {
        // Pick a random thought fragment
        if (thoughtFragments == null || thoughtFragments.Count == 0) return;

        string thought = thoughtFragments[Random.Range(0, thoughtFragments.Count)];

        // Create text object at random position within container
        GameObject thoughtObj = Instantiate(thoughtTextPrefab, spawnArea);
        RectTransform rt = thoughtObj.GetComponent<RectTransform>();

        // Increased padding to keep text away from borders
        float padding = 75f;
        float verticalPadding = 40f;

        // Random position within container bounds
        float randX = Random.Range(-spawnArea.rect.width / 2 + padding, spawnArea.rect.width / 2 - padding);
        float randY = Random.Range(-spawnArea.rect.height / 2 + verticalPadding, spawnArea.rect.height / 2 - verticalPadding);
        rt.anchoredPosition = new Vector2(randX, randY);

        // Set the text
        TextMeshProUGUI textComponent = thoughtObj.GetComponent<TextMeshProUGUI>();
        textComponent.text = thought;
        textComponent.color = new Color(1f, 1f, 1f, 0);  // Start transparent

        // Add text constraints to prevent overflow
        textComponent.enableWordWrapping = true;

        // Set maximum width for the text based on panel size
        float maxWidth = spawnArea.rect.width - (padding * 2);
        rt.sizeDelta = new Vector2(maxWidth * 0.6f, textComponent.preferredHeight);

        // Add some rotation for variety
        rt.rotation = Quaternion.Euler(0, 0, Random.Range(-10f, 10f));

        // Manage the thought object
        activeThoughts.Add(thoughtObj);
        StartCoroutine(AnimateThought(thoughtObj, textComponent));
    }

    private IEnumerator AnimateThought(GameObject thoughtObj, TextMeshProUGUI textComponent)
    {
        // Fade in
        float timer = 0f;
        while (timer < fadeInTime)
        {
            timer += Time.deltaTime;
            float alpha = timer / fadeInTime;
            textComponent.color = new Color(textComponent.color.r, textComponent.color.g, textComponent.color.b, alpha);
            yield return null;
        }

        // Stay visible
        yield return new WaitForSeconds(stayTime);

        // Fade out
        timer = 0f;
        while (timer < fadeOutTime)
        {
            timer += Time.deltaTime;
            float alpha = 1 - (timer / fadeOutTime);
            textComponent.color = new Color(textComponent.color.r, textComponent.color.g, textComponent.color.b, alpha);
            yield return null;
        }

        // Remove and destroy
        activeThoughts.Remove(thoughtObj);
        Destroy(thoughtObj);
    }
}