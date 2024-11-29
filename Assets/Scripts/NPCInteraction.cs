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
    public List<string> lines;
    public List<bool> breakPoints;
    public DialogueResponse responseOptions;
    public string[] npcResponsesToPlayer;
    public GameQuests triggeredQuest;
}

public class NPCInteraction : MonoBehaviour
{
    [SerializeField] private string npcName;
    [SerializeField] private List<DialogueSegment> dialogueSegments;
    [SerializeField] private List<string> questCompleteResponses;
    [SerializeField] private List<string> incompleteQuestResponses;

    public static bool IsInteracting = false;
    private bool isPlayerInRange = false;
    public GameObject interactionText; //"E to interact" text
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
        if (dialogueSegmentIndex >= dialogueSegments.Count) //nothing for the NPC to say, default case essentially 
        {
            dialogueManager.ShowDialogue(npcName, "That's all I have to say!");
            player.canMove = true; //player movement immediately
            return;
        }
        
        //Otherwise, proceed with interaction
        player.canMove = false;
        IsInteracting = true;
        DialogueSegment currentSegment = dialogueSegments[dialogueSegmentIndex];

        if (interactionCount < currentSegment.lines.Count)
        {
            string currentLine = currentSegment.lines[interactionCount];
            dialogueManager.ShowDialogue(npcName, currentLine);

            if (currentSegment.responseOptions != null && interactionCount == currentSegment.lines.Count - 1) //trigger responses if available
            {
                dialogueManager.ShowResponses(currentSegment.responseOptions.responses, OnResponseSelected);
            }
            else
            {
                interactionCount++;
            }
            if (interactionCount <= currentSegment.breakPoints.Count && currentSegment.breakPoints[interactionCount - 1]) //check for a breakpoint
            {
                EndInteraction();
                return;
            }
        }
        else //end current segment
        {
            if (currentSegment.triggeredQuest != null)
            {
                TriggerQuest(currentSegment.triggeredQuest);
            }
            MoveToNextSegment(); //move on
        }
    }

    private void EndInteraction()
    {
        PlayerControl player = FindObjectOfType<PlayerControl>();
        player.canMove = true;
        IsInteracting = false;

        // Hide dialogue if interaction ends completely
        dialogueManager.HideDialogue();
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

    private void OnResponseSelected(int responseIndex)
    {
        DialogueSegment currentSegment = dialogueSegments[dialogueSegmentIndex];
        PlayerControl player = FindObjectOfType<PlayerControl>();

        // Display the NPC's response to the player's choice
        if (responseIndex < currentSegment.npcResponsesToPlayer.Length)
        {
            string npcResponse = currentSegment.npcResponsesToPlayer[responseIndex];
            dialogueManager.ShowDialogue(npcName, npcResponse);

            // Store the NPC response for later to prevent skipping directly to end
            interactionCount = currentSegment.lines.Count; // Move interactionCount to the end of current lines
            IsInteracting = true; // Stay in interaction state until player presses E again
        }

        // Trigger quest if applicable
        if (currentSegment.triggeredQuest != null && responseIndex == 0) // Example: trigger quest on "Yes" response
        {
            TriggerQuest(currentSegment.triggeredQuest);
        }

        // Wait for player to press E to end the interaction after the response
    }



    private void TriggerQuest(GameQuests quest)
    {
        PlayerQuestManager questManager = FindObjectOfType<PlayerQuestManager>();
        if (questManager != null)
        {
            quest.IsEnabled = true;
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
