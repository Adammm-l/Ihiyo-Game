using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventoryModel;
using InventoryCTRL;

public class ItemPickup : MonoBehaviour
{
    [Header("Item Info")]
    [SerializeField] private ItemSO inventoryItemSO; //Reference to the ItemSO
    private bool isPlayerInRange = false;
    private Item item;
    PuzzleCompletionChecker puzzleChecker;

    [Header("References")]
    KeybindManager keybindManager;
    KeyCode interactKey;

    void Start()
    {
        item = GetComponent<Item>();
        keybindManager = KeybindManager.Instance;

        if (inventoryItemSO != null && GetComponent<SpriteRenderer>() != null)
            GetComponent<SpriteRenderer>().sprite = inventoryItemSO.ItemIMG;

        if (item.name == "TimeManipStone")
        {
            puzzleChecker = FindObjectOfType<PuzzleCompletionChecker>();
            GetComponent<SpriteRenderer>().sprite = puzzleChecker.completionSprites[0];
        }
    }

    void Update()
    {
        interactKey = keybindManager.GetKeybind("Interact");
        if (isPlayerInRange && Input.GetKeyDown(interactKey))
        {
            if (item.name == "TimeManipStone")
            {
                puzzleChecker = FindObjectOfType<PuzzleCompletionChecker>();
                if (!puzzleChecker.isPuzzleFinished)
                {
                    return;
                }
            }
            Inventory questInventory = FindObjectOfType<Inventory>();
            if (questInventory != null && item != null)
            {
                questInventory.AddItem(item.ItemName);

                PlayerQuestManager questManager = FindObjectOfType<PlayerQuestManager>();
                if (questManager != null)
                    questManager.UpdateQuestProgress(item.ItemName);

                QuestLogManager questLogManager = FindObjectOfType<QuestLogManager>();
                if (questLogManager != null)
                    questLogManager.UpdateQuestLog();
            }
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