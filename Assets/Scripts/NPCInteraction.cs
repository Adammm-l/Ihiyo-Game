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
        player.canMove = false; //freezes player when interacting
        IsInteracting = true;

        if (interactionCount < dialogueLines.Count)
        {
            GameQuests quest = interactionCount < triggeredQuests.Count ? triggeredQuests[interactionCount] : null;

            if (quest != null && quest.IsEnabled && quest.currentAmount > 0) //path if the quest is already activated
            {
                Debug.Log($"NPC detected active quest: {quest.questTitle}");
                Inventory playerInventory = FindObjectOfType<Inventory>();
                if (playerInventory != null)
                {
                    int playerItemCount = playerInventory.GetItemCount(quest.requiredItem); //part of debug log
                    Debug.Log($"Player has {playerItemCount} {quest.requiredItem}(s). Quest requires {quest.requiredAmount}.");

                    if (playerInventory.GetItemCount(quest.requiredItem) >= quest.requiredAmount)
                    {
                        playerInventory.RemoveItem(quest.requiredItem, quest.requiredAmount);
                        PlayerQuestManager questManager = FindObjectOfType<PlayerQuestManager>();
                        if (questManager != null)
                        {
                            questManager.CompleteQuest(quest);
                        }
                        Debug.Log("Quest completed!");
                        dialogueManager.ShowDialogue(npcName, questCompleteResponses[interactionCount]); //response for complete
                    }
                    else
                    {
                        dialogueManager.ShowDialogue(npcName, incompleteQuestResponses[interactionCount]); //response for incomplete quest
                    }
                }
            }
            else
            {
                dialogueManager.ShowDialogue(npcName, dialogueLines[interactionCount]);
                if (interactionCount < triggeredQuests.Count && triggeredQuests[interactionCount] != null) //Check if the current dialogue triggers a quest
                {
                    if (quest != null && quest.IsEnabled && quest.currentAmount == 0)
                    {
                        GiveQuestToPlayer(quest);
                    }
                    if (interactionCount < responseOptions.Count && responseOptions[interactionCount].responses.Length > 0) //Handle multiple-choice responses if available
                    {
                        dialogueManager.ShowResponses(responseOptions[interactionCount].responses, OnResponseSelected);
                    }
                    else
                    {
                        interactionCount++;
                    }
                }
            }
        }
        else //End of dialogue, allow the player to move again
        {
            dialogueManager.ShowDialogue(npcName, "That's all I have to say!");
            player.canMove = true;
            IsInteracting = false;
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
    void ResetDialogue() //not really necessary yet, will probably be useful later
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
