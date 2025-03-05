using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
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

    [Header("Quest Settings")]

    [Header("Managers")]
    public GameObject interactionText; //"E to interact" text
    public GameObject keybindHolder;
    private DialogueManager dialogueManager;
    KeybindManager keybindManager;
    KeyCode interactKey;

    [Header("UI Elements")]
    [SerializeField] private GameObject questNotificationSprite;
    [SerializeField] private float notificationDuration = 3f;
    [SerializeField] private TextMeshProUGUI BoughtItemText;


    void Start()
    {
        keybindManager = keybindHolder.GetComponent<KeybindManager>();

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
        //Debug.Log($"Interact called. dialogueSegmentIndex: {dialogueSegmentIndex}, interactionCount: {interactionCount}");
        PlayerControl player = FindObjectOfType<PlayerControl>();
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
        Inventory playerInventory = FindObjectOfType<Inventory>();
        PlayerQuestManager questManager = FindObjectOfType<PlayerQuestManager>();

        for (int i = 0; i < amount; i++)
        {
            playerInventory.AddItem(itemName);

            // Update quest progress when item is given
            if (questManager != null)
            {
                questManager.UpdateQuestProgress(itemName);
            }
        }
        //Debug.Log($"Gave player {amount}x {itemName}");
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
        // First check if this NPC has given any active quests
        PlayerQuestManager questManager = FindObjectOfType<PlayerQuestManager>();
        if (questManager != null)
        {
            foreach (GameQuests quest in questManager.GetActiveQuests())
            {
                // Check if the quest was given by this NPC (referenced in dialogueSegments)
                bool questGivenByThisNPC = false;
                foreach (DialogueSegment segment in dialogueSegments)
                {
                    if (segment.triggeredQuest != null && segment.triggeredQuest.questTitle == quest.questTitle)
                    {
                        questGivenByThisNPC = true;
                        break;
                    }
                }

                if (questGivenByThisNPC)
                {
                    // This NPC gave the quest but it's incomplete
                    string response = !string.IsNullOrEmpty(quest.incompleteResponse)
                        ? quest.incompleteResponse
                        : "You haven't completed my task yet.";

                    dialogueManager.ShowDialogue(npcName, response);
                    npcMovement.PauseMovementWithTimer(5f);
                    player.canMove = true;
                    IsInteracting = false;
                    return;
                }
            }
        }

        // Then check if this NPC can complete quests from other NPCs
        CheckForCompletableQuests(player, npcMovement);
    }

    private void CheckForCompletableQuests(PlayerControl player, NPCMovement npcMovement)
    {
        // Get all active quests
        PlayerQuestManager questManager = FindObjectOfType<PlayerQuestManager>();
        Inventory playerInventory = FindObjectOfType<Inventory>();

        if (questManager == null || playerInventory == null)
        {
            // No quest manager
            dialogueManager.ShowDialogue(npcName, "That's all I have to say!");
            npcMovement.PauseMovementWithTimer(5f);
            player.canMove = true;
            IsInteracting = false;
            return;
        }

        // Check all active quests
        foreach (GameQuests quest in questManager.GetActiveQuests())
        {
            // Check if this NPC can complete this quest
            if (quest.completionNPC == npcName)
            {
                int playerItemCount = playerInventory.GetItemCount(quest.requiredItem);

                if (playerItemCount >= quest.requiredAmount)
                {
                    // Player has the required items for this quest
                    playerInventory.RemoveItem(quest.requiredItem, quest.requiredAmount);
                    questManager.CompleteQuest(quest);
                    Debug.Log($"[CheckForCompletableQuests] Quest completed: {quest.questTitle}");

                    // Use quest-specific complete response or default
                    string response = !string.IsNullOrEmpty(quest.completeResponse)
                        ? quest.completeResponse
                        : "Thank you for bringing this to me!";

                    dialogueManager.ShowDialogue(npcName, response);

                    npcMovement.PauseMovementWithTimer(5f);
                    player.canMove = true;
                    IsInteracting = false;
                    return;
                }
                else
                {
                    // Player doesn't have the required items
                    string response = !string.IsNullOrEmpty(quest.incompleteResponse)
                        ? quest.incompleteResponse
                        : "You don't have what I need yet.";

                    dialogueManager.ShowDialogue(npcName, response);

                    npcMovement.PauseMovementWithTimer(5f);
                    player.canMove = true;
                    IsInteracting = false;
                    return;
                }
            }
        }

        // No matching quests found
        dialogueManager.ShowDialogue(npcName, "That's all I have to say!");
        npcMovement.PauseMovementWithTimer(5f);
        player.canMove = true;
        IsInteracting = false;
    }

    private void HandleQuestResponses(Inventory playerInventory, GameQuests quest, NPCMovement npcMovement)
    {
        //check if quest is fulfilled
        int playerItemCount = playerInventory.GetItemCount(quest.requiredItem);
        Debug.Log($"[HandleQuestResponses] Player has {playerItemCount} of {quest.requiredItem}.");

        if (playerItemCount >= quest.requiredAmount) //quest is done
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

            string response = !string.IsNullOrEmpty(quest.completeResponse)
                ? quest.completeResponse
                : "Thank you for bringing this to me!";

            dialogueManager.ShowDialogue(npcName, response);
        }
        else //quest is not done
        {
            string response = !string.IsNullOrEmpty(quest.incompleteResponse)
                ? quest.incompleteResponse
                : "You don't have what I need yet.";

            dialogueManager.ShowDialogue(npcName, response);
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
            questManager.AcceptQuest(quest);
            ShowQuestNotification();
            Debug.Log($"Triggered quest: {quest.questTitle}");
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
}