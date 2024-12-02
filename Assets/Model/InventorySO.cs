using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

namespace InventoryModel {

[CreateAssetMenu]

public class InventorySO : ScriptableObject
{

    [SerializeField] private List<InventoryItem> inventoryItems;

    [field: SerializeField] 

    public int Size { get; private set;} = 10;

    public event Action<Dictionary<int, InventoryItem>> OnInventoryUpdated;

    public void Initialize() { // Basically create a list of items and fill them out with emtpy values instead of null

        inventoryItems = new List<InventoryItem>();

        for (int i = 0; i < Size; i++) {

            inventoryItems.Add(InventoryItem.GetEmptyItem());
        }
    }


    public int AddItem(ItemSO item, int num) {

        if (item.IsStack == false) {

            for (int i = 0; i < inventoryItems.Count; i++) {

                while (num > 0 && IsInvenFull() == false) {

                    //num -= AddFirstFreeItem(item, 1, itemState);
                    num -= AddFirstFreeItem(item,1);
                }

                InformChange(); // Inform system about any changes made
                //return num; // Always return quantity, even if we doin't have space
            }
        }

        num = AddStackableItem(item,num);

        InformChange();

        return num;
    }

    private int AddFirstFreeItem(ItemSO item, int num) {

        InventoryItem newItem = new InventoryItem {

            item = item,
            num = num,
            //itemState = new List<ItemParameter>(itemState == null ? item.DefaultParemetersList: itemState)
        };

        for (int i = 0; i < inventoryItems.Count; i++) {

            if (inventoryItems[i].isEmpty) {

                inventoryItems[i] = newItem;
                return num;
            }
        }

        return 0;
    }

    private bool IsInvenFull() {

       return inventoryItems.Where(item => item.isEmpty).Any() == false;

    }

    private int AddStackableItem(ItemSO item, int num) {

        for (int i = 0; i < inventoryItems.Count; i++) {

            if (inventoryItems[i].isEmpty) {

                continue;
            }

            if(inventoryItems[i].item.ID == item.ID) {

                int amtTake = inventoryItems[i].item.MaxStackSize - inventoryItems[i].num;

                if (num > amtTake) {

                    inventoryItems[i] = inventoryItems[i].ChangeNum(inventoryItems[i].item.MaxStackSize);

                    num -= amtTake;
                }

                else {

                    inventoryItems[i] = inventoryItems[i].ChangeNum(inventoryItems[i].num + num);

                    InformChange();

                    return 0;
                }
            }
        }

        while (num > 0 && IsInvenFull() ==  false) {

            int newNum = Mathf.Clamp(num, 0, item.MaxStackSize);

            num -= newNum;
            AddFirstFreeItem(item, newNum);
        }

        return num;
    }

    public void RemoveItem(int itemIndex, int num) {

        if (inventoryItems.Count > itemIndex) {

            if (inventoryItems[itemIndex].isEmpty) {

                return;

            }

            int remind = inventoryItems[itemIndex].num - num;

            if (remind <= 0) {

                inventoryItems[itemIndex] = InventoryItem.GetEmptyItem();
            }

            else {

                inventoryItems[itemIndex] = inventoryItems[itemIndex].ChangeNum(remind);
            }

            InformChange();
        }
    }

    public void AddItem(InventoryItem item) {

        AddItem(item.item, item.num);
    }

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

    public InventoryItem GetItemAt(int index) {

        return inventoryItems[index];
    }

    public void SwapItems(int item1, int item2) { // Swap the items around without changing their data

        InventoryItem itemint1 = inventoryItems[item1];
        inventoryItems[item1] = inventoryItems[item2];
        inventoryItems[item2] = itemint1;

        InformChange();
    }

    private void InformChange() {

        OnInventoryUpdated?.Invoke(GetCurrentInventoryState());
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

}