using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventoryModel;
using InventoryCTRL;

public class ItemPickup : MonoBehaviour
{
    [Header("Item Info")]
    [SerializeField] private ItemSO inventoryItemSO; // Reference to the ItemSO
    private bool isPlayerInRange = false;
    private Item item;

    [Header("References")]
    public GameObject keybindHolder;
    KeybindManager keybindManager;
    KeyCode interactKey;

    void Start()
    {
        item = GetComponent<Item>();
        keybindManager = keybindHolder.GetComponent<KeybindManager>();

        // Set sprite to match inventory item
        if (inventoryItemSO != null && GetComponent<SpriteRenderer>() != null)
            GetComponent<SpriteRenderer>().sprite = inventoryItemSO.ItemIMG;
    }

    private bool hasBeenPickedUp = false;

    void Update()
    {
        interactKey = keybindManager.GetKeybind("Interact");
        if (isPlayerInRange && Input.GetKeyDown(interactKey) && !hasBeenPickedUp)
        {
            hasBeenPickedUp = true; // Prevent multiple pickups

            // Add to quest inventory
            Inventory playerInventory = FindObjectOfType<Inventory>();
            if (playerInventory != null && item != null)
            {
                playerInventory.AddItem(item.ItemName);

                // Update quests
                PlayerQuestManager questManager = FindObjectOfType<PlayerQuestManager>();
                if (questManager != null)
                    questManager.UpdateQuestProgress(item.ItemName);

                QuestLogManager questLogManager = FindObjectOfType<QuestLogManager>();
                if (questLogManager != null)
                    questLogManager.UpdateQuestLog();
            }

            // Let ItemPick component handle UI inventory
            // We just need to destroy the object
            Debug.Log($"Picked up: {item.ItemName}");
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            isPlayerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            isPlayerInRange = false;
    }
}