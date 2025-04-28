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
    [SerializeField] private RectTransform thoughtsContainer;
    [SerializeField] private GameObject thoughtTextPrefab;
    [SerializeField] private TextMeshProUGUI titleText;


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
        keybindManager = KeybindManager.Instance;
        npcMovement = GetComponentInParent<NPCMovement>();
        thoughtBubbleUI.SetActive(false);
    }

    void Update()
    {
        if (!isPlayerInRange) return;

        if (playerControl == null)
        {
            playerControl = FindObjectOfType<PlayerControl>();
            if (playerControl != null)
            {
                playerForm = playerControl.GetComponent<SwitchPlayerForm>();
            }
        }
        interactKey = keybindManager.GetKeybind("Interact");

        if (isShowingThoughts && playerForm != null && !playerForm.isGhost)
        {
            HideThoughts();
            return;
        }

        if (playerForm != null && playerForm.isGhost)
        {
            if (Input.GetKeyDown(interactKey) && !isShowingThoughts)
            {
                ShowThoughts();
            }
            else if (Input.GetKeyDown(interactKey) && isShowingThoughts)
            {
                HideThoughts();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;

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
        }
    }

    private void ShowThoughts()
    {
        if (playerForm == null || !playerForm.isGhost) return;

        playerControl.canMove = false;
        if (npcMovement != null)
        {
            npcMovement.PauseMovementInfinitely();
        }

        if (titleText != null)
        {
            titleText.text = $"{npcName}'s Thoughts";
            titleText.characterSpacing = 1f;
            titleText.gameObject.SetActive(true);
        }

        if (thoughtBubbleUI != null)
        {
            thoughtBubbleUI.SetActive(true);
            isShowingThoughts = true;

            thoughtsRoutine = StartCoroutine(SpawnThoughtsRoutine());
        }
    }

    private void HideThoughts()
    {
        if (thoughtsRoutine != null)
        {
            StopCoroutine(thoughtsRoutine);
        }

        foreach (GameObject thought in activeThoughts)
        {
            Destroy(thought);
        }
        activeThoughts.Clear();

        if (thoughtBubbleUI != null)
        {
            thoughtBubbleUI.SetActive(false);
        }

        if (playerControl != null && !playerForm.isPossessing)
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
        if (thoughtFragments == null || thoughtFragments.Count == 0) return;

        string thought = thoughtFragments[Random.Range(0, thoughtFragments.Count)];

        GameObject thoughtObj = Instantiate(thoughtTextPrefab, thoughtsContainer);
        RectTransform rt = thoughtObj.GetComponent<RectTransform>();

        float padding = 75f;
        float verticalPadding = 40f;
        float randX = Random.Range(-thoughtsContainer.rect.width / 2 + padding, thoughtsContainer.rect.width / 2 - padding);
        float randY = Random.Range(-thoughtsContainer.rect.height / 2 + verticalPadding, thoughtsContainer.rect.height / 2 - verticalPadding);
        rt.anchoredPosition = new Vector2(randX, randY);
        TextMeshProUGUI textComponent = thoughtObj.GetComponent<TextMeshProUGUI>();
        textComponent.text = thought;
        textComponent.color = new Color(1f, 1f, 1f, 0);

        textComponent.enableWordWrapping = true;

        float maxWidth = thoughtsContainer.rect.width - (padding * 2);
        rt.sizeDelta = new Vector2(maxWidth * 0.6f, textComponent.preferredHeight);

        rt.rotation = Quaternion.Euler(0, 0, Random.Range(-10f, 10f));

        activeThoughts.Add(thoughtObj);
        StartCoroutine(AnimateThought(thoughtObj, textComponent));
    }

    private IEnumerator AnimateThought(GameObject thoughtObj, TextMeshProUGUI textComponent)
    {
        //Fade in
        float timer = 0f;
        while (timer < fadeInTime)
        {
            timer += Time.deltaTime;
            float alpha = timer / fadeInTime;
            textComponent.color = new Color(textComponent.color.r, textComponent.color.g, textComponent.color.b, alpha);
            yield return null;
        }

        //Stay visible
        yield return new WaitForSeconds(stayTime);

        //ade out
        timer = 0f;
        while (timer < fadeOutTime)
        {
            timer += Time.deltaTime;
            float alpha = 1 - (timer / fadeOutTime);
            textComponent.color = new Color(textComponent.color.r, textComponent.color.g, textComponent.color.b, alpha);
            yield return null;
        }
        activeThoughts.Remove(thoughtObj);
        Destroy(thoughtObj);
    }
}