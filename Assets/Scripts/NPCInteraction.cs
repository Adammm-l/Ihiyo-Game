using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCInteraction : MonoBehaviour
{
    private bool isPlayerInRange = false;
    public GameObject interactionText; //the text that says "E" to interact
    private DialogueManager dialogueManager;
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
        dialogueManager.ShowDialogue("Hi, I am from Terraria");

    }
}
