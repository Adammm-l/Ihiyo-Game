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
            dialogueManager.ShowDialogue(npcName, dialogueLines[interactionCount]);
            if (interactionCount < triggeredQuests.Count && triggeredQuests[interactionCount] != null) //Check if the current dialogue triggers a quest
            {
                if (triggeredQuests[interactionCount].IsEnabled)                 //Trigger the quest only if it is enabled since i couldnt get null quests to work
                {
                    //Debug.Log($"Triggering quest: {triggeredQuests[interactionCount].questTitle}");
                    GiveQuestToPlayer(triggeredQuests[interactionCount]);
                }
                else
                {
                    //Debug.Log($"Quest skipped: {triggeredQuests[interactionCount].questTitle} (Disabled)");
                }
            }

            if (interactionCount < responseOptions.Count && responseOptions[interactionCount].responses.Length > 0)
            {
                dialogueManager.ShowResponses(responseOptions[interactionCount].responses, OnResponseSelected);
            }
            else
            {
                interactionCount++;
            }
        }
        else //End of dialogue, allow the player to move again
        {
            dialogueManager.ShowDialogue(npcName, "That's all I have to say!");
            player.canMove = true;
            IsInteracting = false;
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

            //Update quest log
            QuestLogManager questLogManager = FindObjectOfType<QuestLogManager>();
            if (questLogManager != null)
            {
                questLogManager.UpdateQuestLog();
            }
        }
    }
}
