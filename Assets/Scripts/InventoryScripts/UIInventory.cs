using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;


namespace UIInventoryPage {
public class UIInventory : MonoBehaviour
{
    [SerializeField] private UIInventoryItem itemPrefab;

    [SerializeField] RectTransform contentPane;

    [SerializeField] private UIInvenDesc itemDesc;

    [SerializeField] MouseFollower mouseFollower;

    List<UIInventoryItem> uiItemList = new List<UIInventoryItem>(); // Create a list of items

    private int curDragItemIndex = -1;

    public event Action<int> OnDescriptionRequested, OnItemActionRequested, OnStartDragging; // Behavior for when we click on an Item

    public event Action<int,int> OnSwapItems; // For when we swap items in inventory

    private static bool invenExists; // All instances of this Player references the exact same variable

    private void Awake() {

        Hide();
        mouseFollower.Toggle(false); // Make the Mouse Cursor invisible at first since we don't wanna see it outside of menus
        itemDesc.ResetDescription();

    }

    void Start() {
        if(!invenExists) { // If the player doesn't exist, then mark them as Don't Destroy on Load, handling duplicates
            invenExists = true;
            DontDestroyOnLoad(transform.gameObject);
        }

        else { // Eliminate Duplicate Objects
            Destroy(gameObject);
        }
    }

    public void InitializeInvenUI(int intSize)
    {
        // Clear any existing slots
        foreach (Transform child in contentPane)
        {
            if (child.GetComponent<UIInventoryItem>() != null)
                Destroy(child.gameObject);
        }
        uiItemList.Clear();

        for (int i = 0; i < intSize; i++)
        {
            UIInventoryItem uIItem = Instantiate(itemPrefab, Vector3.zero, Quaternion.identity); // Creates the item object
            uIItem.transform.SetParent(contentPane);
            uiItemList.Add(uIItem); // Add item to list

            // Assign Methods to Clicks
            uIItem.OnItemClicked += HandleItemSelection;
            uIItem.OnItemBeginDrag += HandleBeginDrag;
            uIItem.OnItemDroppedOn += HandleSwap;
            uIItem.OnItemEndDrag += HandleEndDrag;
            uIItem.OnRightMouseBtnClick += HandleShowItemActions;
        }
    }

        // Show and Hide the Inventory Page
        public void Show() {

        gameObject.SetActive(true);
        ResetSelect();

    }

    public void ResetSelect() {

        itemDesc.ResetDescription();
        DeselectItems();
    }

    private void DeselectItems() {

        foreach(UIInventoryItem item in uiItemList) {

            item.Deselect();

        }
    }

    public void Hide() {

        gameObject.SetActive(false);
        ResetItemDrag();
    }
    

    private void HandleItemSelection(UIInventoryItem wield) {

        int index = uiItemList.IndexOf(wield);

        if (index == -1) {

            return;

        }

        OnDescriptionRequested?.Invoke(index);

    }

    private void HandleSwap(UIInventoryItem wield) {

        int index = uiItemList.IndexOf(wield);

        if(index == -1) { // Don't Swap with a Non-existant object

            return;
        }     
// Handle Item Identify logic for dragging and swapping items

        OnSwapItems?.Invoke(curDragItemIndex, index);
        HandleItemSelection(wield);
           
    }

    private void HandleBeginDrag(UIInventoryItem wield) {

        int index = uiItemList.IndexOf(wield);

        if(index == -1) { // Check if the object exists

            return;

        }

        curDragItemIndex = index;

        HandleItemSelection(wield); // Handle the current item we selected

        OnStartDragging?.Invoke(index);

    }

    private void HandleEndDrag(UIInventoryItem wield) {

        ResetItemDrag();
  
    }

    internal void UpdateDesc(int itemIndex, Sprite itemIMG, string name, string desc) {

        itemDesc.SetDescription(itemIMG, name, desc);
        DeselectItems();
        uiItemList[itemIndex].Select();

    }

    public void UpdateData(int itmIndex, Sprite itmIMG, int itmNum) {

        if (uiItemList.Count > itmIndex) { // If the item is in our list of items

            uiItemList[itmIndex].SetData(itmIMG, itmNum);

        }
    }

    private void HandleShowItemActions(UIInventoryItem wield) {

        int index = uiItemList.IndexOf(wield);

        if (index == -1) {

            return;

        }

        OnItemActionRequested?.Invoke(index);

    }

    private void ResetItemDrag() {

        mouseFollower.Toggle(false);

        curDragItemIndex = -1;
        
    }

    public void CreateDrag(Sprite sprite, int num) {

        mouseFollower.Toggle(true);

        mouseFollower.SetData(sprite, num);

    }

    internal void ResetAllItems() {

        foreach(var item in uiItemList) {

            item.ResetData();
            item.Deselect();

        }
    }

    
}


}