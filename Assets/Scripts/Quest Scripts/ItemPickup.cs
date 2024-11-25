using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    private bool isPlayerInRange = false;
    private Item item;

    // Start is called before the first frame update
    void Start()
    {
        item = GetComponent<Item>();
    }

    // Update is called once per frame
    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E)) //pickup
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
