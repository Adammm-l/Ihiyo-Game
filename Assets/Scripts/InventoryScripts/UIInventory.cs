using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIInventory : MonoBehaviour
{
    [SerializeField] private UIInventoryItem itemPrefab;

    [SerializeField] RectTransform contentPane;

    List<UIInventoryItem> uiItemList = new List<UIInventoryItem>(); // Create a list of items

    public void InitializeInvenUI(int intSize) {

        for (int i = 0; i < intSize; i++) {
// Creates a number of items in a list

            UIInventoryItem uIItem = Instantiate (itemPrefab, Vector3.zero, Quaternion.identity); // Basically creates the item object
            uIItem.transform.SetParent(contentPane);
            uiItemList.Add(uIItem); // Add item to list

// Assign Methods to Clicks

            //uIItem.OnItemClicked += HandleItemSelection;
            //uIItem.OnItemBeginDrag += HandleBeginDrag;
            //uIItem.OnItemDroppedOn += HandleSwap;
            //uIItem.OnItemEndDrag += HandleEndDrag;
            //uIItem.OnRightMouseBtnClick += HandleShowItemActions;

        }
    }

// Show and Hide the Inventory Page
    public void Show() {

        gameObject.SetActive(true);

    }

    public void Hide() {

        gameObject.SetActive(false);
    }
    
}
