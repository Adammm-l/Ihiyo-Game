using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventoryModel;

public class PickUpSystem : MonoBehaviour
{

    [SerializeField] private InventorySO inventoryData;
 
    private void OnTriggerEnter2D(Collider2D collision) {

        ItemPick item = collision.GetComponent<ItemPick>();

        if (item != null) {

            int remind = inventoryData.AddItem(item.InventoryItem, item.Quantity);

            if (remind == 0) {

                item.DestroyItem(); // End the Item

            }

            else {

                item.Quantity = remind;

            }
        }
    }
}
