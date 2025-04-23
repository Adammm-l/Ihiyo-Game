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

    private static bool invenExists; // All instances of this Player references the exact same variable

        public void Update()
        {
            inventoryKey = keybindManager.GetKeybind("Inventory");
            if (Input.GetKeyDown(inventoryKey))
            {
                if (inventoryUI.isActiveAndEnabled == false)
                {
                    inventoryUI.Show();

                    foreach (var item in inventoryData.GetCurrentInventoryState())
                    {
                        inventoryUI.UpdateData(item.Key, item.Value.item.ItemIMG, item.Value.num);
                    }

                    // Add this line to ensure selection is reset after loading items
                    inventoryUI.ResetSelect();
                }
                else
                {
                    inventoryUI.Hide();
                } 
            }
        }

        void Start()
        {
            keybindManager = FindObjectOfType<KeybindManager>();

            if (keybindManager == null)
            {
                Debug.LogError("KeybindManager not found in the scene! Ensure it's added to the scene.");
            }

            // Debug initial inventory state
            Debug.Log("Initial inventory state:");
            for (int i = 0; i < inventoryData.Size; i++)
            {
                var item = inventoryData.GetItemAt(i);
                Debug.Log($"Slot {i}: {(item.isEmpty ? "Empty" : item.item.name)}");
            }

            // Make sure these methods are called during initialization
            PrepareInventoryData();
            PrepareUI();

            Debug.Log($"InventoryController initialized with data ID: {inventoryData.GetInstanceID()}");
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

    // Add this public method to remove items by name
    public void RemoveItemByName(string itemName, int amount)
    {
        if (inventoryData == null) return;
    
        // Loop through inventory slots to find the item
        for (int i = 0; i < inventoryData.Size; i++)
        {
            var item = inventoryData.GetItemAt(i);
            if (!item.isEmpty && item.item.name == itemName)
            {
                // Found the item, remove it
                inventoryData.RemoveItem(i, amount);
                Debug.Log($"Removed {amount}x {itemName} from UI inventory at slot {i}");
                break;
            }
        }
    }

    private void HandleDescriptionRequested(int itemIndex)
    {
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
