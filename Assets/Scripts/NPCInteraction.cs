using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueResponse
{
    public string[] responses;
}

public class NPCInteraction : MonoBehaviour
{
    [SerializeField] private string npcName;
    [SerializeField] private List<string> dialogueLines; //initial dialogues
    [SerializeField] private List<DialogueResponse> responseOptions; //array of responses
    [SerializeField] private List<string> npcResponses; //NPC reply to each response

    [SerializeField] private List<GameQuests> triggeredQuests; //Quests triggered by specific dialogue lines
    [SerializeField] private List<string> questCompleteResponses; //Responses for just-completed quests
    [SerializeField] private List<string> incompleteQuestResponses; //Responses for incomplete quests

    public static bool IsInteracting = false;  //when interacting with an NPC
    private bool isPlayerInRange = false;
    public GameObject interactionText; //the text that says "E" to interact
    private DialogueManager dialogueManager;

    //this is for different dialogues depending how many times the player has interacted with the NPC
    private int interactionCount = 0;

    void Start()
    {
        interactionText.SetActive(false);
        dialogueManager = FindObjectOfType<DialogueManager>();
    }

    // Update is called once per frame
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
        player.canMove = false; // Freeze player during interaction
        IsInteracting = true;

        // Debug: Interaction triggered
        Debug.Log($"Interact called. Interaction Count: {interactionCount}, Dialogue Lines Count: {dialogueLines.Count}");

        // Check if there's an active quest to handle
        GameQuests quest = interactionCount < triggeredQuests.Count ? triggeredQuests[interactionCount] : null;

        // Handle quest progression and completion
        if (quest != null && quest.IsEnabled)
        {
            Inventory playerInventory = FindObjectOfType<Inventory>();
            if (playerInventory != null)
            {
                Debug.Log($"Player Inventory detected. {quest.requiredItem} count: {playerInventory.GetItemCount(quest.requiredItem)}");

                if (playerInventory.GetItemCount(quest.requiredItem) >= quest.requiredAmount)
                {
                    // Complete the quest
                    Debug.Log("Completing quest...");
                    playerInventory.RemoveItem(quest.requiredItem, quest.requiredAmount);
                    PlayerQuestManager questManager = FindObjectOfType<PlayerQuestManager>();
                    if (questManager != null)
                    {
                        questManager.CompleteQuest(quest);
                    }

                    // Use custom dialogue for quest completion
                    dialogueManager.ShowDialogue(npcName, questCompleteResponses[Mathf.Min(interactionCount, questCompleteResponses.Count - 1)]);

                    // Increment interaction count after quest completion
                    interactionCount++;
                    return; // Exit after handling quest completion
                }
                else
                {
                    // Quest incomplete dialogue
                    int remaining = quest.requiredAmount - playerInventory.GetItemCount(quest.requiredItem);
                    Debug.Log($"Quest incomplete. Remaining: {remaining} {quest.requiredItem}(s)");
                    dialogueManager.ShowDialogue(npcName, $"You still need {remaining} more {quest.requiredItem}(s)");

                    // Increment interaction count even if the quest is incomplete
                    interactionCount++;
                    return; // Exit after handling quest status
                }
            }
        }

        // Handle regular dialogue if no active quest or quest logic is completed
        if (interactionCount < dialogueLines.Count)
        {
            Debug.Log($"Displaying regular dialogue: {dialogueLines[interactionCount]}");
            dialogueManager.ShowDialogue(npcName, dialogueLines[interactionCount]);

            // Trigger a new quest if applicable
            if (quest != null && quest.IsEnabled && quest.currentAmount == 0)
            {
                Debug.Log($"Triggering quest: {quest.questTitle}");
                GiveQuestToPlayer(quest);
            }

            // Handle response options if available
            if (interactionCount < responseOptions.Count && responseOptions[interactionCount].responses.Length > 0)
            {
                Debug.Log($"Displaying response options for dialogue line {interactionCount}");
                dialogueManager.ShowResponses(responseOptions[interactionCount].responses, OnResponseSelected);
            }
            else
            {
                Debug.Log("No response options. Incrementing interaction count.");
                interactionCount++;
            }
        }
        else
        {
            // Default dialogue when all lines are exhausted
            Debug.Log("Default dialogue triggered. Checking for quest...");
            if (quest != null && quest.IsEnabled)
            {
                Debug.Log($"Quest still active: {quest.questTitle}");
                dialogueManager.ShowDialogue(npcName, "You still need to complete my task!");
            }
            else
            {
                dialogueManager.ShowDialogue(npcName, "That's all I have to say!");
                player.canMove = true;
                IsInteracting = false;
            }
        }
    }


    private void CompleteQuest(GameQuests quest)
    {
        PlayerQuestManager playerQuestManager = FindObjectOfType<PlayerQuestManager>();
        if (playerQuestManager != null)
        {
            playerQuestManager.RemoveQuest(quest); // Remove quest from quest log

            QuestLogManager questLogManager = FindObjectOfType<QuestLogManager>();
            if (questLogManager != null)
            {
                questLogManager.UpdateQuestLog(); // Update quest log UI
            }

            Debug.Log($"Quest completed: {quest.questTitle}");
        }
    }


    private void OnResponseSelected(int responseIndex)
    {
        //Show NPC's response to the selected option
        PlayerControl player = FindObjectOfType<PlayerControl>();
        dialogueManager.ShowDialogue(npcName, npcResponses[responseIndex]);

        interactionCount++;

        if (interactionCount >= dialogueLines.Count)
        {
            player.canMove = true; //Re-enable movement
            IsInteracting = false;
        }
    }
    void ResetDialogue() //not useful yet
    {
        interactionCount = 0;
    }

    private void GiveQuestToPlayer(GameQuests quest)
    {
        PlayerQuestManager playerQuestManager = FindObjectOfType<PlayerQuestManager>();
        if (playerQuestManager != null)
        {
            playerQuestManager.AcceptQuest(quest);
            EnableQuestItems(quest.requiredItem);

            //Update quest log
            QuestLogManager questLogManager = FindObjectOfType<QuestLogManager>();
            if (questLogManager != null)
            {
                questLogManager.UpdateQuestLog();
            }
        }
    }

    private void EnableQuestItems(string itemName)
    {
        Item[] items = Resources.FindObjectsOfTypeAll<Item>();

        foreach (Item item in items)
        {
            if (item.ItemName == itemName) //uses the name of the item, set from Item.cs to determine what items will appear
            {
                item.gameObject.SetActive(true);
                Debug.Log($"Enabled item: {itemName}");
            }
        }
    }
}
