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
    KeybindManager keybindManager;
    KeyCode interactKey;

    void Start()
    {
        item = GetComponent<Item>();
        keybindManager = KeybindManager.Instance;

        // Set sprite to match inventory item
        if (inventoryItemSO != null && GetComponent<SpriteRenderer>() != null)
            GetComponent<SpriteRenderer>().sprite = inventoryItemSO.ItemIMG;
    }

    private bool hasBeenPickedUp = false;

    void Update()
    {
        interactKey = keybindManager.GetKeybind("Interact");
        if (isPlayerInRange && Input.GetKeyDown(interactKey))
        {
            // Add to quest inventory
            Inventory questInventory = FindObjectOfType<Inventory>();
            if (questInventory != null && item != null)
            {
                questInventory.AddItem(item.ItemName);

                // Update quest progress
                PlayerQuestManager questManager = FindObjectOfType<PlayerQuestManager>();
                if (questManager != null)
                    questManager.UpdateQuestProgress(item.ItemName);

                QuestLogManager questLogManager = FindObjectOfType<QuestLogManager>();
                if (questLogManager != null)
                    questLogManager.UpdateQuestLog();
            }

            // Add to UI inventory through bridge
            if (inventoryItemSO != null)
            {
                InventoryBridge.AddItem(inventoryItemSO, 1);
            }

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