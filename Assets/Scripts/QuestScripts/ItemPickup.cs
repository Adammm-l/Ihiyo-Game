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

    void Update()
    {
        interactKey = keybindManager.GetKeybind("Interact");
        if (isPlayerInRange && Input.GetKeyDown(interactKey))
        {
            // Add to quest inventory
            Inventory questInventory = FindObjectOfType<Inventory>();
            if (questInventory != null && item != null)
                questInventory.AddItem(item.ItemName);

            // Add to UI inventory by creating an ItemPick object
            if (inventoryItemSO != null)
            {
                // Create a temporary pickup that uses the existing PickUpSystem
                GameObject tempPickup = new GameObject("TempPickup");
                ItemPick itemPick = tempPickup.AddComponent<ItemPick>();

                // Use reflection to set private fields
                var itemField = typeof(ItemPick).GetField("InventoryItem",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.Public);
                itemField?.SetValue(itemPick, inventoryItemSO);

                var quantityField = typeof(ItemPick).GetField("Quantity",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.Public);
                quantityField?.SetValue(itemPick, 1);

                // Get the player's PickUpSystem to handle this item
                PickUpSystem pickupSystem = FindObjectOfType<PickUpSystem>();
                if (pickupSystem != null)
                {
                    // Trigger pickup via OnTriggerEnter2D
                    var collider = tempPickup.AddComponent<BoxCollider2D>();
                    pickupSystem.SendMessage("OnTriggerEnter2D", collider);
                    Debug.Log("Sent item to UI inventory system");
                }

                Destroy(tempPickup);
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