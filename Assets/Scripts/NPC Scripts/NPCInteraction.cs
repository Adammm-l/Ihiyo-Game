using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

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

    [Header("Managers")]
    public GameObject interactionText; //"E to interact" text
    public GameObject keybindHolder;
    private DialogueManager dialogueManager;
    KeybindManager keybindManager;
    KeyCode interactKey;

    [Header("UI Elements")]
    [SerializeField] private GameObject questNotificationSprite;
    [SerializeField] private float notificationDuration = 3f;


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
        player.canMove = false;
        NPCMovement npcMovement = GetComponentInParent<NPCMovement>();
        npcMovement.PauseMovementInfinitely();
        NPCInteraction.IsInteracting = true;

        if (dialogueSegmentIndex >= dialogueSegments.Count) //defualt interaction if there are no more lines
        {
            HandleDefaultInteraction(player, npcMovement);
            return;
        }

        DialogueSegment currentSegment = dialogueSegments[dialogueSegmentIndex];

        if ((interactionCount < currentSegment.lines.Count) || (currentSegment.triggerQuestAfterSegment && currentSegment.triggeredQuest != null)) //runs regularly 
        {
            HandleDialogue(currentSegment);
            if (currentSegment.triggerQuestAfterSegment && currentSegment.triggeredQuest != null)
            {
                TriggerQuest(currentSegment.triggeredQuest);
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
        if (dialogueSegments.Count > 0) 
        {
            DialogueSegment lastSegment = dialogueSegments[dialogueSegments.Count - 1];
            GameQuests quest = lastSegment.triggeredQuest;

            if (quest != null && quest.IsEnabled) //there is indeed active quest
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
        dialogueManager.ShowDialogue(npcName, "That's all I have to say!"); //nothing active
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
            dialogueManager.ShowDialogue(npcName, questCompleteResponses[0]);
        }
        else //quest is not done
        {
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