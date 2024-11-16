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

    [SerializeField] private List<GameQuests> npcQuests; //List of quests that NPC's can give


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
        //Debug.Log("Interacted with NPC!");
        PlayerControl player = FindObjectOfType<PlayerControl>(); 
        player.canMove = false; //disable movement during interactions

        if (interactionCount < dialogueLines.Count)
        {
            //initial dialogue
            dialogueManager.ShowDialogue(npcName, dialogueLines[interactionCount]);
            //Show response options if available
            if (interactionCount < responseOptions.Count && responseOptions[interactionCount].responses.Length > 0)
            {
                dialogueManager.ShowResponses(responseOptions[interactionCount].responses, OnResponseSelected);
            }
            else
            {
                interactionCount++;
            }
        }
        else
        {
            dialogueManager.ShowDialogue(npcName, "STOP TALKING TO ME!!");
            player.canMove = true;
        }

        //quest giving
        if (npcQuests.Count > 0)
        {
            //Show the quest details
            GameQuests currentQuest = npcQuests[0];
            dialogueManager.ShowDialogue(npcName, $"Quest: {currentQuest.questTitle}\n{currentQuest.questDescription}");
            GiveQuestToPlayer(currentQuest);
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
            player.canMove = true; // Re-enable movement
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
            npcQuests.Remove(quest);
        }
    }
}
