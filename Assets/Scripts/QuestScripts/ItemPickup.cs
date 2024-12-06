using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Item Info")]
    private bool isPlayerInRange = false;
    private Item item;

    [Header("References")]
    public GameObject keybindHolder;
    KeybindManager keybindManager;
    KeyCode interactKey;

    // Start is called before the first frame update
    void Start()
    {
        item = GetComponent<Item>();
        keybindManager = keybindHolder.GetComponent<KeybindManager>();
    }

    // Update is called once per frame
    void Update()
    {
        interactKey = keybindManager.GetKeybind("Interact");
        if (isPlayerInRange && Input.GetKeyDown(interactKey)) //pickup
        {
            Inventory playerInventory = FindObjectOfType<Inventory>();
            if (playerInventory != null && item != null)
            {
                playerInventory.AddItem(item.ItemName);
                UpdateQuestProgress(item.ItemName);
                Debug.Log($"Picked up: {item.ItemName}");
                Destroy(gameObject); //Remove the item from the scene
            }
        }
    }
    private void UpdateQuestProgress(string itemName)
    {
        PlayerQuestManager questManager = FindObjectOfType<PlayerQuestManager>();
        if (questManager != null)
        {
            questManager.UpdateQuestProgress(itemName);
        }

        QuestLogManager questLogManager = FindObjectOfType<QuestLogManager>();
        if (questLogManager != null)
        {
            questLogManager.UpdateQuestLog();
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }
}
