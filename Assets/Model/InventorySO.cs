using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu]

public class InventorySO : ScriptableObject
{

    [SerializeField] private List<InventoryItem> inventoryItems;

    [field: SerializeField] 

    public int Size { get; private set;} = 10;

    public void Initialize() { // Basically create a list of items and fill them out with emtpy values instead of null

        inventoryItems = new List<InventoryItem>();

        for (int i = 0; i < Size; i++) {

            inventoryItems.Add(InventoryItem.GetEmptyItem());
        }
    }

    // public void AddItem(ItemSO item, int num) {

    //     for (int i = 0; i < inventoryItems.Count; i++) {

    //         if(inventoryItems[i].isEmpty()) {

    //             inventoryItems[i] = new InventoryItem {

    //                 item = item,
    //                 num = num
    //             };
    //         }
    //     }
        
    // }

    public Dictionary<int, InventoryItem> GetCurrentInventoryState() {

        Dictionary<int, InventoryItem> returnVal = new Dictionary<int, InventoryItem>();

        for (int i = 0; i < inventoryItems.Count; i++) {

            if (inventoryItems[i].isEmpty) {
                continue;
            }
            returnVal[i] = inventoryItems[i];
        }

        return returnVal;
    }

}


[Serializable]

public struct InventoryItem { // Need this to change values like quantity, Using Struct over Class for security purposes

    public int num;
    public ItemSO item;

    public bool isEmpty => item == null;

    public InventoryItem ChangeNum (int newNum) { // Change the amount of items, 

        return new InventoryItem {

            item  = this.item,
            num = newNum

        };
    }

    public static InventoryItem GetEmptyItem() {

        return new InventoryItem 
        {

            item = null,
            num = 0

        };

    }
    
}

