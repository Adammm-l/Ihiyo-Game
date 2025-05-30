using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using InventoryCTRL;
using InventoryModel;
//adam
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
    // Add item reward fields
    public bool giveItemAfterSegment = false;
    public string itemToGive;
    public int itemAmount = 1;
}

public class NPCInteraction : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private string npcName;
    [SerializeField] private List<DialogueSegment> dialogueSegments;

    [Header("Interactions")]
    public static bool IsInteracting = false;
    private bool isPlayerInRange = false;
    private int interactionCount = 0;
    private int dialogueSegmentIndex = 0;
    private bool isShopOpen = false;

    [Header("Managers")]
    public GameObject interactionText; //"E to interact" text
    private DialogueManager dialogueManager;
    KeybindManager keybindManager;
    KeyCode interactKey;

    [Header("UI Elements")]
    [SerializeField] private GameObject questNotificationSprite;
    [SerializeField] private float notificationDuration = 3f;
    [SerializeField] private TextMeshProUGUI BoughtItemText;


    void Start()
    {
        keybindManager = KeybindManager.Instance;

        TextMeshProUGUI interactionTextBox = interactionText.transform.GetComponent<TextMeshProUGUI>();
        interactKey = keybindManager.GetKeybind("Interact");

        string interactButton = interactKey.ToString();
        interactionTextBox.text = $"\"{interactButton}\" to interact";

        interactionText.SetActive(false);
        questNotificationSprite.SetActive(false);
        dialogueManager = FindObjectOfType<DialogueManager>();
    }

    void Update()
    {
        if (DialogueManager.IsMultipleChoiceActive) return;

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

            NPCMovement npcMovement = GetComponentInParent<NPCMovement>();
            if (npcMovement != null)
            {
                npcMovement.ResumeMovement();
            }
        }
    }

    private void Interact()
    {
        PlayerControl player = FindObjectOfType<PlayerControl>();

        // Check if player is in ghost form
        SwitchPlayerForm playerForm = player.GetComponent<SwitchPlayerForm>();
        if (playerForm != null && playerForm.isGhost)
        {
            // Let GhostNPCInteraction handle it
            return;
        }

        NPCMovement npcMovement = GetComponentInParent<NPCMovement>();
        MerchantTypeNPC merchant = GetComponentInParent<MerchantTypeNPC>();
        player.canMove = false;
        npcMovement.PauseMovementInfinitely();
        NPCInteraction.IsInteracting = true;

        if (isShopOpen)
        {
            //Close the shop if it's already open
            if (merchant != null)
            {
                merchant.CloseShop();
                isShopOpen = false;
                NPCInteraction.IsInteracting = false;
                player.canMove = true;
            }
            return;
        }

        // Check for quests first - HIGHEST PRIORITY
        PlayerQuestManager questManager = FindObjectOfType<PlayerQuestManager>();
        if (questManager != null)
        {
            Inventory playerInventory = FindObjectOfType<Inventory>();
            if (playerInventory != null)
            {
                // First priority: Check if this NPC is a completion NPC for any active quest
                foreach (GameQuests quest in questManager.GetActiveQuests())
                {
                    if (quest.completionNPC == npcName)
                    {
                        HandleQuestResponses(playerInventory, quest, npcMovement);
                        player.canMove = true;
                        IsInteracting = false;
                        return;
                    }
                }

                // Second priority: Check if this NPC gave a quest to another NPC
                if (dialogueSegments.Count > 0)
                {
                    foreach (DialogueSegment segment in dialogueSegments)
                    {
                        if (segment.triggerQuestAfterSegment && segment.triggeredQuest != null)
                        {
                            GameQuests questGiven = segment.triggeredQuest;

                            foreach (GameQuests activeQuest in questManager.GetActiveQuests())
                            {
                                if (activeQuest.questTitle == questGiven.questTitle)
                                {
                                    if (!string.IsNullOrEmpty(activeQuest.completionNPC) && activeQuest.completionNPC != npcName)
                                    {
                                        string incompleteResponse = !string.IsNullOrEmpty(activeQuest.giverIncompleteResponse)
                                            ? activeQuest.giverIncompleteResponse
                                            : "Have you completed that task yet?";

                                        dialogueManager.ShowDialogue(npcName, incompleteResponse);
                                        npcMovement.PauseMovementInfinitely();
                                        player.canMove = true;
                                        IsInteracting = false;
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        // Continue with regular dialogue if no quest responses were triggered
        if (dialogueSegmentIndex >= dialogueSegments.Count) //defualt interaction if there are no more lines
        {
            if (merchant != null) //checking if the NPC can sell things
            {
                merchant.OpenShop();
                isShopOpen = true;
                NPCInteraction.IsInteracting = true;
                return;
            }
            HandleDefaultInteraction(player, npcMovement);
            return;
        }

        DialogueSegment currentSegment = dialogueSegments[dialogueSegmentIndex];

        if ((interactionCount < currentSegment.lines.Count) ||
            (currentSegment.triggerQuestAfterSegment && currentSegment.triggeredQuest != null) ||
            (currentSegment.giveItemAfterSegment && !string.IsNullOrEmpty(currentSegment.itemToGive))) //runs regularly 
        {
            HandleDialogue(currentSegment);
            if (currentSegment.triggerQuestAfterSegment && currentSegment.triggeredQuest != null)
            {
                TriggerQuest(currentSegment.triggeredQuest);
            }

            if (currentSegment.giveItemAfterSegment && !string.IsNullOrEmpty(currentSegment.itemToGive)) //gives the item to player if active
            {
                GiveItemToPlayer(currentSegment.itemToGive, currentSegment.itemAmount);
            }

            for (int i = 0; i < currentSegment.breakPoints.Count; i++)
            {
                if (currentSegment.breakPoints[i])
                {
                    MoveToNextSegment();
                    return;
                }
            }
        }
    }

    private void GiveItemToPlayer(string itemName, int amount)
    {
        Inventory questInventory = FindObjectOfType<Inventory>();
        for (int i = 0; i < amount; i++)
        {
            questInventory.AddItem(itemName);
        }

        ItemSO itemSO = Resources.Load<ItemSO>("Items/" + itemName);
        if (itemSO != null)
        {
            //Debug.Log($"Adding {itemName} to UI inventory");
            InventoryBridge.AddItem(itemSO, amount);
        }
        else
        {
            //Debug.Log($"Item {itemName} is non-physical, only added to quest inventory");
        }
    }

    private void HandleDialogue(DialogueSegment currentSegment) //regular dialogue handler
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

    private void HandleDefaultInteraction(PlayerControl player, NPCMovement npcMovement) //checks default interaction. If there is an active quest, it overrides it
    {
        PlayerQuestManager questManager = FindObjectOfType<PlayerQuestManager>();
        if (questManager != null)
        {
            Inventory playerInventory = FindObjectOfType<Inventory>();
            if (playerInventory != null)
            {
                // First priority: Check for quests this NPC should complete
                foreach (GameQuests quest in questManager.GetActiveQuests())
                {
                    if (quest.completionNPC == npcName)
                    {
                        HandleQuestResponses(playerInventory, quest, npcMovement);
                        player.canMove = true;
                        IsInteracting = false;
                        return;
                    }
                }

                // Second priority: Check for quests this NPC gave (even if another NPC completes them)
                if (dialogueSegments.Count > 0)
                {
                    foreach (DialogueSegment segment in dialogueSegments)
                    {
                        if (segment.triggerQuestAfterSegment && segment.triggeredQuest != null)
                        {
                            GameQuests questGiven = segment.triggeredQuest;

                            foreach (GameQuests activeQuest in questManager.GetActiveQuests())
                            {
                                if (activeQuest.questTitle == questGiven.questTitle)
                                {
                                    if (string.IsNullOrEmpty(activeQuest.completionNPC) || activeQuest.completionNPC == npcName)
                                    {
                                        HandleQuestResponses(playerInventory, activeQuest, npcMovement);
                                    }
                                    else
                                    {
                                        string incompleteResponse = !string.IsNullOrEmpty(activeQuest.giverIncompleteResponse)
                                            ? activeQuest.giverIncompleteResponse
                                            : "Have you completed that task yet?";

                                        dialogueManager.ShowDialogue(npcName, incompleteResponse);
                                    }
                                    player.canMove = true;
                                    IsInteracting = false;
                                    return;
                                }
                            }
                        }
                    }
                }
            }
        }

        dialogueManager.ShowDialogue(npcName, "**yawn**"); //nothing active
        npcMovement.PauseMovementWithTimer(5f); // For default interaction, NPC walks away after 5 seconds
        player.canMove = true;
        IsInteracting = false;
    }

    private void HandleQuestResponses(Inventory playerInventory, GameQuests quest, NPCMovement npcMovement)
    {
        bool questCompleted = false;

        if (quest.UsesMultipleItems)
        {
            // Check all required items
            questCompleted = true;
            foreach (QuestItemRequirement req in quest.requiredItems)
            {
                int playerItemCount = playerInventory.GetItemCount(req.itemName);
                Debug.Log($"[HandleQuestResponses] Player has {playerItemCount} of {req.itemName}. Needs {req.amount}");

                // Update current amount for quest tracking
                req.currentAmount = playerItemCount;

                if (playerItemCount < req.amount)
                {
                    questCompleted = false;
                    break;
                }
            }
        }
        else
        {
            // Legacy single item check
            int playerItemCount = playerInventory.GetItemCount(quest.requiredItem);
            quest.currentAmount = playerItemCount;
            Debug.Log($"[HandleQuestResponses] Player has {playerItemCount} of {quest.requiredItem}.");
            questCompleted = playerItemCount >= quest.requiredAmount;
        }

        bool isCompletionNPC = (quest.completionNPC == npcName);

        if (questCompleted)
        {
            // Only completion NPC can actually complete the quest
            if (isCompletionNPC || string.IsNullOrEmpty(quest.completionNPC))
            {
                // Remove all required items
                if (quest.UsesMultipleItems)
                {
                    foreach (QuestItemRequirement req in quest.requiredItems)
                    {
                        RemoveItemFromPlayer(req.itemName, req.amount);
                    }
                }
                else
                {
                    RemoveItemFromPlayer(quest.requiredItem, quest.requiredAmount);
                }

                PlayerQuestManager questManager = FindObjectOfType<PlayerQuestManager>();
                if (questManager != null)
                {
                    questManager.CompleteQuest(quest);
                    Debug.Log($"[HandleQuestResponses] Quest completed: {quest.questTitle}");
                }

                string completeResponse = !string.IsNullOrEmpty(quest.completionCompleteResponse)
                    ? quest.completionCompleteResponse
                    : "Thank you for completing this task!";

                dialogueManager.ShowDialogue(npcName, completeResponse);
            }
        }
        else
        {
            // Quest not completed - same as before
            string incompleteResponse;

            if (isCompletionNPC)
            {
                incompleteResponse = !string.IsNullOrEmpty(quest.completionIncompleteResponse)
                    ? quest.completionIncompleteResponse
                    : "I'm waiting for you to complete this task.";
            }
            else
            {
                incompleteResponse = !string.IsNullOrEmpty(quest.giverIncompleteResponse)
                    ? quest.giverIncompleteResponse
                    : "Have you completed that task yet?";
            }

            dialogueManager.ShowDialogue(npcName, incompleteResponse);
        }

        npcMovement.PauseMovementInfinitely();
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
        if (dialogueSegmentIndex < dialogueSegments.Count - 1)
        {
            dialogueSegmentIndex++;
        }
        else
        {
            dialogueSegmentIndex = dialogueSegments.Count;
        }

        interactionCount = 0;
        EndInteraction();
    }

    private void OnResponseSelected(int responseIndex)
    {
        DialogueSegment currentSegment = dialogueSegments[dialogueSegmentIndex];
        DialogueResponse response = currentSegment.responseOptions;

        if (responseIndex < response.nextSegmentIDs.Length) //moves to next segment: basically allows for branching dialogue
        {
            int nextLineSegmentID = response.nextSegmentIDs[responseIndex];
            if (nextLineSegmentID != -1)
            {
                dialogueSegmentIndex = nextLineSegmentID;
                interactionCount = 0;
                DialogueSegment nextSegment = dialogueSegments[dialogueSegmentIndex];
                if (nextSegment.lines.Count > 0)
                {
                    dialogueManager.ShowDialogue(npcName, nextSegment.lines[interactionCount]);
                    interactionCount++;
                }
                return;
            }
        }
        //If no follow-up segment, end interaction or move to the next segment
        MoveToNextSegment();
    }

    private void TriggerQuest(GameQuests quest)
    {
        PlayerQuestManager questManager = FindObjectOfType<PlayerQuestManager>();
        if (questManager != null)
        {
            quest.IsEnabled = true;

            if (string.IsNullOrEmpty(quest.completionNPC))
            {
                quest.completionNPC = npcName;
            }

            questManager.AcceptQuest(quest);
            ShowQuestNotification();

            // Enable all required items
            if (quest.UsesMultipleItems)
            {
                foreach (QuestItemRequirement req in quest.requiredItems)
                {
                    EnableQuestItems(req.itemName);
                }
            }
            else
            {
                EnableQuestItems(quest.requiredItem);
            }
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

    //===== Notification stuffs - DONT TOUCH ======
    public void ShowPurchaseNotification(string itemName, int amount)
    {
        BoughtItemText.text = $"Bought {amount}x {itemName}";
        BoughtItemText.gameObject.SetActive(true);
        StartCoroutine(HidePurchaseNotification());
    }

    private IEnumerator HidePurchaseNotification()
    {
        yield return new WaitForSeconds(notificationDuration);
        BoughtItemText.gameObject.SetActive(false);
    }

    private void ShowQuestNotification()
    {
        questNotificationSprite.SetActive(true);
        StartCoroutine(HideQuestNotificationAfterDelay());
    }

    private IEnumerator HideQuestNotificationAfterDelay()
    {
        yield return new WaitForSeconds(notificationDuration);

        questNotificationSprite.SetActive(false);
    }
    private void RemoveItemFromPlayer(string itemName, int amount)
    {
        // Remove from quest inventory
        Inventory questInventory = FindObjectOfType<Inventory>();
        if (questInventory != null)
            questInventory.RemoveItem(itemName, amount);

        // Remove from UI inventory
        InventoryController inventoryController = FindObjectOfType<InventoryController>();
        if (inventoryController != null)
        {
            inventoryController.RemoveItemByName(itemName, amount);
        }
    }
}