using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventoryModel;

public class PickUpSystem : MonoBehaviour
{

    [SerializeField] private InventorySO inventoryData;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"PickUpSystem: Triggered by {collision.gameObject.name}");

        ItemPick item = collision.GetComponent<ItemPick>();

        if (item != null)
        {
            Debug.Log($"PickUpSystem: Found ItemPick component with item {item.InventoryItem.name}, qty: {item.Quantity}");
            Debug.Log($"PickUpSystem: Using inventoryData with ID: {inventoryData.GetInstanceID()}");

            int remind = inventoryData.AddItem(item.InventoryItem, item.Quantity);

            Debug.Log($"PickUpSystem: AddItem returned {remind} (0 means all items added successfully)");

            if (remind == 0)
            {
                Debug.Log($"PickUpSystem: All items added, destroying pickup");
                item.DestroyItem();
            }
            else
            {
                Debug.Log($"PickUpSystem: Not all items added, remaining: {remind}");
                item.Quantity = remind;
            }
        }
        else
        {
            Debug.Log($"PickUpSystem: No ItemPick component found on collision object");
        }
    }
}
