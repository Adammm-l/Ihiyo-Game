using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//this script basically manages all of NPC interactions and quest giving
[System.Serializable]
public class DialogueResponse
{
    public string[] responses;
    public int[] nextSegmentIDs;
}

[System.Serializable]
public class DialogueSegment
{
    public List<string> lines;
    public List<bool> breakPoints; 
    public DialogueResponse responseOptions;
    public GameQuests triggeredQuest;
    public bool triggerQuestAfterSegment = false;
    public int nextSegmentID = -1; 
}


public class NPCInteraction : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private string npcName;
    [SerializeField] private List<DialogueSegment> dialogueSegments;
    [SerializeField] private List<string> questCompleteResponses;
    [SerializeField] private List<string> incompleteQuestResponses;

    [Header("Interactions")]
    public static bool IsInteracting = false;
    private bool isPlayerInRange = false;
    private int interactionCount = 0;
    private int dialogueSegmentIndex = 0;

    [Header("References")]
    public GameObject interactionText; //"E to interact" text
    public GameObject keybindHolder;
    private DialogueManager dialogueManager;
    KeybindManager keybindManager;
    KeyCode interactKey;

    void Start()
    {
        keybindManager = keybindHolder.GetComponent<KeybindManager>();

        TextMeshProUGUI interactionTextBox = interactionText.transform.GetComponent<TextMeshProUGUI>();
        interactKey = keybindManager.GetKeybind("Interact");

        string interactButton = interactKey.ToString();
        interactionTextBox.text = $"\"{interactButton}\" to interact";

        interactionText.SetActive(false);
        dialogueManager = FindObjectOfType<DialogueManager>();
    }

    void Update()
    {
        if (DialogueManager.IsMultipleChoiceActive) return;

        interactKey = keybindManager.GetKeybind("Interact");
        if (isPlayerInRange && Input.GetKeyDown(interactKey))
        {
            Interact();
        }
    }

    private void OnTriggerEnter2D(Collider2D other) //collider stuff for interaction
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
        Debug.Log($"[NPCInteraction] Interact called. dialogueSegmentIndex: {dialogueSegmentIndex}, interactionCount: {interactionCount}");

        PlayerControl player = FindObjectOfType<PlayerControl>();
        player.canMove = false; // Freeze player during interaction
        NPCMovement npcMovement = GetComponentInParent<NPCMovement>();
        npcMovement.PauseMovementInfinitely();
        NPCInteraction.IsInteracting = true;

        if (dialogueSegmentIndex >= dialogueSegments.Count)
        {
            Debug.Log("[NPCInteraction] All dialogue segments exhausted. Calling HandleDefaultInteraction.");
            HandleDefaultInteraction(player, npcMovement);
            return;
        }

        DialogueSegment currentSegment = dialogueSegments[dialogueSegmentIndex];
        Debug.Log($"[NPCInteraction] Processing segment {dialogueSegmentIndex}. interactionCount: {interactionCount}");

        if (interactionCount < currentSegment.lines.Count)  //never evaluates as false
        {
            HandleDialogue(currentSegment);
        }
        else
        {
            if (currentSegment.triggerQuestAfterSegment && currentSegment.triggeredQuest != null)
            {
                TriggerQuest(currentSegment.triggeredQuest);
            }

            if (currentSegment.nextSegmentID != -1)
            {
                Debug.Log($"NextSegmentID was -1");
                dialogueSegmentIndex = currentSegment.nextSegmentID;
                interactionCount = 0;

                if (dialogueSegmentIndex < dialogueSegments.Count)
                {
                    DialogueSegment nextSegment = dialogueSegments[dialogueSegmentIndex];
                    if (nextSegment.lines.Count > 0)
                    {
                        dialogueManager.ShowDialogue(npcName, nextSegment.lines[interactionCount]);
                        interactionCount++;
                    }
                }
            }
            else
            {
                MoveToNextSegment();
            }
        }
    }


    private void HandleDialogue(DialogueSegment currentSegment)
    {
        string currentLine = currentSegment.lines[interactionCount];
        dialogueManager.ShowDialogue(npcName, currentLine);

        if (currentSegment.responseOptions != null && interactionCount == currentSegment.lines.Count - 1)
        {
            dialogueManager.ShowResponses(currentSegment.responseOptions.responses, OnResponseSelected);
        }
        else
        {
            interactionCount++;
        }
    }
    private void HandleDefaultInteraction(PlayerControl player, NPCMovement npcMovement)
    {
        Debug.Log($"[NPCInteraction] HandleDefaultInteraction called for {npcName}.");

        if (dialogueSegments.Count > 0)
        {
            DialogueSegment lastSegment = dialogueSegments[dialogueSegments.Count - 1];
            GameQuests quest = lastSegment.triggeredQuest;

            if (quest != null && quest.IsEnabled)
            {
                Inventory playerInventory = FindObjectOfType<Inventory>();
                if (playerInventory != null)
                {
                    HandleQuestResponses(playerInventory, quest, npcMovement);
                    player.canMove = true;
                    IsInteracting = false;
                    return;
                }
            }
        }
        dialogueManager.ShowDialogue(npcName, "That's all I have to say!");
        npcMovement.PauseMovementWithTimer(5f); // Optional: Add delay before resuming movement
        player.canMove = true;
        IsInteracting = false;
    }




    private void HandleQuestResponses(Inventory playerInventory, GameQuests quest, NPCMovement npcMovement)
    {
        Debug.Log($"[HandleQuestResponses] Starting quest response handling. Quest: {quest.questTitle}, Required Item: {quest.requiredItem}, Required Amount: {quest.requiredAmount}");

        // Check if player has enough items to complete the quest
        int playerItemCount = playerInventory.GetItemCount(quest.requiredItem);
        Debug.Log($"[HandleQuestResponses] Player has {playerItemCount} of {quest.requiredItem}.");

        if (playerItemCount >= quest.requiredAmount)
        {
            // Remove items and complete the quest
            Debug.Log($"[HandleQuestResponses] Player has enough items to complete the quest. Completing quest...");
            playerInventory.RemoveItem(quest.requiredItem, quest.requiredAmount);

            PlayerQuestManager questManager = FindObjectOfType<PlayerQuestManager>();
            if (questManager != null)
            {
                questManager.CompleteQuest(quest);
                Debug.Log($"[HandleQuestResponses] Quest completed: {quest.questTitle}");
            }
            dialogueManager.ShowDialogue(npcName, questCompleteResponses[0]);
        }
        else
        {
            // Show incomplete quest response
            dialogueManager.ShowDialogue(npcName, incompleteQuestResponses[0]);
        }

        npcMovement.PauseMovementWithTimer(5f);
    }


    private void EndInteraction()
    {
        PlayerControl player = FindObjectOfType<PlayerControl>();
        player.canMove = true;

        NPCMovement npcMovement = GetComponentInParent<NPCMovement>();
        npcMovement.ResumeMovement();

        NPCInteraction.IsInteracting = false;
        dialogueManager.HideDialogue();
    }


    private void MoveToNextSegment()
    {
        Debug.Log($"[NPCInteraction] MoveToNextSegment called. Current index: {dialogueSegmentIndex}");

        if (dialogueSegmentIndex < dialogueSegments.Count - 1)
        {
            dialogueSegmentIndex++;
        }
        else
        {
            dialogueSegmentIndex = dialogueSegments.Count; // Cap the index
        }

        interactionCount = 0;
        EndInteraction();
    }





    private void OnResponseSelected(int responseIndex)
    {
        DialogueSegment currentSegment = dialogueSegments[dialogueSegmentIndex];
        DialogueResponse response = currentSegment.responseOptions;

        // Navigate directly to the next dialogue segment
        if (responseIndex < response.nextSegmentIDs.Length)
        {
            int nextSegmentID = response.nextSegmentIDs[responseIndex];
            if (nextSegmentID != -1)
            {
                dialogueSegmentIndex = nextSegmentID;
                interactionCount = 0; // Reset interaction count for the new segment

                // Automatically display the first line of the next segment
                DialogueSegment nextSegment = dialogueSegments[dialogueSegmentIndex];
                if (nextSegment.lines.Count > 0)
                {
                    dialogueManager.ShowDialogue(npcName, nextSegment.lines[interactionCount]);
                    interactionCount++; // Progress within the segment
                }
                return; // Exit after transitioning
            }
        }

        // If no follow-up segment, end interaction or move to the next segment
        MoveToNextSegment();
    }




    private void TriggerQuest(GameQuests quest)
    {
        PlayerQuestManager questManager = FindObjectOfType<PlayerQuestManager>();
        if (questManager != null)
        {
            quest.IsEnabled = true;
            questManager.AcceptQuest(quest);
            //Debug.Log($"Triggered quest: {quest.questTitle}");
            EnableQuestItems(quest.requiredItem);
        }
    }

    private void EnableQuestItems(string itemName)
    {
        Item[] items = Resources.FindObjectsOfTypeAll<Item>();

        foreach (Item item in items)
        {
            if (item.ItemName == itemName)
            {
                item.gameObject.SetActive(true);
                //Debug.Log($"Enabled item: {itemName}");
            }
        }
    }
}
