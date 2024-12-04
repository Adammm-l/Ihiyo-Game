using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UIInventoryPage;
using InventoryModel;

namespace InventoryCTRL {
public class InventoryController : MonoBehaviour
{
    [Header("References")]
    public GameObject keybindHolder;
    KeybindManager keybindManager;
    KeyCode inventoryKey;

    [Header("Inventory")]
    [SerializeField] private UIInventory inventoryUI;

    [SerializeField] private InventorySO inventoryData;
    
    [SerializeField] private AudioClip dropSFX;

    [SerializeField] AudioSource audioSource;

    public List<InventoryItem> initialItems = new List<InventoryItem>();

    public void Start() {
        keybindManager = keybindHolder.GetComponent<KeybindManager>();
        PrepareUI();

        PrepareInventoryData();
    }

    public void Update() {
        inventoryKey = keybindManager.GetKeybind("Inventory");
        if (Input.GetKeyDown(inventoryKey)) { // Open and Close the Inventory Page

            if (inventoryUI.isActiveAndEnabled == false) {
                inventoryUI.Show();

                foreach(var item in inventoryData.GetCurrentInventoryState()) {

                    inventoryUI.UpdateData(item.Key, item.Value.item.ItemIMG, item.Value.num);
                }
            }

            else {
                inventoryUI.Hide();
                
            }
        }
    }

    public void PrepareUI() {

        inventoryUI.InitializeInvenUI(inventoryData.Size);
        this.inventoryUI.OnDescriptionRequested += HandleDescriptionRequested;
        this.inventoryUI.OnSwapItems += HandleSwapItems;
        this.inventoryUI.OnStartDragging += HandleDragging;
        this.inventoryUI.OnItemActionRequested += HandleItemActionRequest;
    }

    public void PrepareInventoryData() {

        inventoryData.Initialize();
        inventoryData.OnInventoryUpdated += UpdateInventoryUI;

        foreach(InventoryItem item in initialItems) {

            if (item.isEmpty) {

                continue;
            }

            inventoryData.AddItem(item);
        }
    }

    private void UpdateInventoryUI(Dictionary<int, InventoryItem> inventoryState) {

        inventoryUI.ResetAllItems();

        foreach(var item in inventoryState) {

            inventoryUI.UpdateData(item.Key, item.Value.item.ItemIMG, item.Value.num);
        }
    }

    private void HandleDescriptionRequested(int itemIndex) {
        InventoryItem inventoryItem = inventoryData.GetItemAt(itemIndex);

        if(inventoryItem.isEmpty) {

            inventoryUI.ResetSelect();
            return;
        }

        ItemSO item = inventoryItem.item;
        inventoryUI.UpdateDesc(itemIndex, item.ItemIMG, item.name, item.Description);
        
    }

    private void HandleSwapItems(int item1, int item2) {

        inventoryData.SwapItems(item1, item2); // Swap Items around and call to the original function

    }

    private void HandleDragging(int itemIndex) { // Call original function to handle dragging behavior

        InventoryItem invenItem = inventoryData.GetItemAt(itemIndex);

        if(invenItem.isEmpty) {

            return;
        }

        inventoryUI.CreateDrag(invenItem.item.ItemIMG, invenItem.num);

    }

    private void HandleItemActionRequest(int wield) {

        InventoryItem item = inventoryData.GetItemAt(wield);

        if (item.isEmpty) {
            return;
        }

        IItemAction itemAction = item.item as IItemAction;

        if (itemAction != null) { // Selected an interactble item

            itemAction.PerformAction(gameObject, null);

        }

        IDestroyableItem destroyableItem = item.item as IDestroyableItem;

        if (destroyableItem != null) { // Get rid of item upon use

            inventoryData.RemoveItem(wield,1);
        }

    }
}

}
