using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DialogueResponse
{
    public string[] responses;
}

[System.Serializable]
public class DialogueSegment
{
    public List<string> lines; // Dialogue lines for this segment
    public List<bool> breakPoints; // Breakpoints for each line
    public DialogueResponse responseOptions; // Response options for the segment
    public GameQuests triggeredQuest; // Quest triggered by this segment
}

public class NPCInteraction : MonoBehaviour
{
    [SerializeField] private string npcName;
    [SerializeField] private List<DialogueSegment> dialogueSegments; // Dialogue split into segments
    [SerializeField] private List<string> questCompleteResponses; // Responses for completed quests
    [SerializeField] private List<string> incompleteQuestResponses; // Responses for incomplete quests

    public static bool IsInteracting = false;
    private bool isPlayerInRange = false;
    public GameObject interactionText; // "E to interact" text
    private DialogueManager dialogueManager;

    private int interactionCount = 0;
    private int dialogueSegmentIndex = 0;

    void Start()
    {
        interactionText.SetActive(false);
        dialogueManager = FindObjectOfType<DialogueManager>();
    }

    void Update()
    {
        if (DialogueManager.IsMultipleChoiceActive) return;

        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            interactionText.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            interactionText.SetActive(false);

            dialogueManager.HideDialogue();
        }
    }

    private void Interact()
    {
        PlayerControl player = FindObjectOfType<PlayerControl>();
        player.canMove = false; // Freeze player movement during interaction
        IsInteracting = true;

        // Check if there are remaining dialogue segments
        if (dialogueSegmentIndex < dialogueSegments.Count)
        {
            DialogueSegment currentSegment = dialogueSegments[dialogueSegmentIndex];

            // Process the current line in the segment
            if (interactionCount < currentSegment.lines.Count)
            {
                string currentLine = currentSegment.lines[interactionCount];
                dialogueManager.ShowDialogue(npcName, currentLine);

                // Trigger responses if available
                if (currentSegment.responseOptions != null &&
                    interactionCount == currentSegment.lines.Count - 1)
                {
                    dialogueManager.ShowResponses(currentSegment.responseOptions.responses, OnResponseSelected);
                }
                else
                {
                    interactionCount++;
                }

                // Check for a breakpoint
                if (interactionCount <= currentSegment.breakPoints.Count && currentSegment.breakPoints[interactionCount - 1])
                {
                    EndInteraction();
                    return;
                }
            }
            else
            {
                // End the current segment
                if (currentSegment.triggeredQuest != null)
                {
                    TriggerQuest(currentSegment.triggeredQuest);
                }

                MoveToNextSegment();
            }
        }
        else
        {
            // All dialogue segments completed
            dialogueManager.ShowDialogue(npcName, "That's all I have to say!");
            EndInteraction();
        }
    }

    private void MoveToNextSegment()
    {
        dialogueSegmentIndex++;
        interactionCount = 0;

        if (dialogueSegmentIndex >= dialogueSegments.Count)
        {
            Debug.Log("All dialogue segments completed.");
        }

        EndInteraction();
    }

    private void EndInteraction()
    {
        PlayerControl player = FindObjectOfType<PlayerControl>();
        player.canMove = true;
        IsInteracting = false;
    }

    private void OnResponseSelected(int responseIndex)
    {
        PlayerControl player = FindObjectOfType<PlayerControl>();

        // Handle quest trigger based on response
        DialogueSegment currentSegment = dialogueSegments[dialogueSegmentIndex];
        if (currentSegment.triggeredQuest != null && responseIndex == 0) // Example: trigger quest on "Yes" response
        {
            TriggerQuest(currentSegment.triggeredQuest);
        }

        MoveToNextSegment();
    }

    private void TriggerQuest(GameQuests quest)
    {
        PlayerQuestManager questManager = FindObjectOfType<PlayerQuestManager>();
        if (questManager != null)
        {
            questManager.AcceptQuest(quest);
            Debug.Log($"Triggered quest: {quest.questTitle}");

            // Enable quest-related items
            EnableQuestItems(quest.requiredItem);
        }
    }

    private void EnableQuestItems(string itemName)
    {
        // Find all items in the scene
        Item[] items = Resources.FindObjectsOfTypeAll<Item>();

        foreach (Item item in items)
        {
            // Activate items that match the quest's required item name
            if (item.ItemName == itemName)
            {
                item.gameObject.SetActive(true);
                Debug.Log($"Enabled item: {itemName}");
            }
        }
    }
}
