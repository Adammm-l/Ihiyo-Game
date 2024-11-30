using System.Collections.Generic;
using UnityEngine;
//this script basically manages all of NPC interactions and quest giving
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

    void Update() //every time E is pressed
    {
        if (DialogueManager.IsMultipleChoiceActive) return;

        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
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

    private void Interact() //main interaction method. Manages what happens when you interact, its order, quest responses, multiple choice responses, etc.
    {
        PlayerControl player = FindObjectOfType<PlayerControl>();
        player.canMove = false;
        NPCMovement npcMovement = GetComponentInParent<NPCMovement>();
        npcMovement.PauseMovementInfinitely();
        IsInteracting = true;

        if (dialogueSegmentIndex >= dialogueSegments.Count) //nothing for the NPC to say, default case essentially + quest checking (also when NPC has no more lines)
        {
            DialogueSegment lastSegment = dialogueSegments[dialogueSegments.Count - 1];
            GameQuests quest = lastSegment.triggeredQuest;

            if (quest != null && quest.IsEnabled)
            {
                Inventory playerInventory = FindObjectOfType<Inventory>();
                if (playerInventory != null)
                {
                    if (playerInventory.GetItemCount(quest.requiredItem) >= quest.requiredAmount)
                    {
                        //quest has been completed get rid of quest components
                        playerInventory.RemoveItem(quest.requiredItem, quest.requiredAmount);
                        PlayerQuestManager questManager = FindObjectOfType<PlayerQuestManager>();
                        if (questManager != null)
                        {
                            questManager.CompleteQuest(quest);
                        }
                        //quest completion response
                        dialogueManager.ShowDialogue(npcName, questCompleteResponses[dialogueSegmentIndex - 1]);
                        npcMovement.PauseMovementWithTimer(5f);
                    }
                    else
                    {
                        //incomplete quest response
                        dialogueManager.ShowDialogue(npcName, incompleteQuestResponses[dialogueSegmentIndex - 1]);
                        npcMovement.PauseMovementWithTimer(5f);
                    }
                    player.canMove = true;
                    IsInteracting = false;
                    return;
                }
            }
            //if no active quest or nothing else to say, show the default line
            dialogueManager.ShowDialogue(npcName, "That's all I have to say!");
            npcMovement.PauseMovementWithTimer(5f);
            player.canMove = true;
            IsInteracting = false;
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
            MoveToNextSegment(); //move on
        }
    }

    private void EndInteraction()
    {
        PlayerControl player = FindObjectOfType<PlayerControl>();
        player.canMove = true;
        NPCMovement npcMovement = GetComponentInParent<NPCMovement>();
        npcMovement.ResumeMovement();

        IsInteracting = false;

        dialogueManager.HideDialogue();
    }

    private void MoveToNextSegment()
    {
        dialogueSegmentIndex++;
        interactionCount = 0;
        /*
        if (dialogueSegmentIndex >= dialogueSegments.Count)
        {
            Debug.Log("All dialogue segments completed.");
        }
        */
        EndInteraction();
    }

    private void OnResponseSelected(int responseIndex)
    {
        DialogueSegment currentSegment = dialogueSegments[dialogueSegmentIndex];
        PlayerControl player = FindObjectOfType<PlayerControl>();

        if (responseIndex < currentSegment.npcResponsesToPlayer.Length) //NPC response to player choice
        {
            string npcResponse = currentSegment.npcResponsesToPlayer[responseIndex];
            dialogueManager.ShowDialogue(npcName, npcResponse);

            interactionCount = currentSegment.lines.Count;
            IsInteracting = true;
        }

        if (currentSegment.triggeredQuest != null && responseIndex == 0) //Trigger quests if applicable
        {
            TriggerQuest(currentSegment.triggeredQuest);
        }
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
                Debug.Log($"Enabled item: {itemName}");
            }
        }
    }
}
