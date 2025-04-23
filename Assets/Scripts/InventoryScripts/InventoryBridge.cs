using UnityEngine;
using InventoryModel;
using InventoryCTRL;
using System.Reflection;

public class InventoryBridge : MonoBehaviour
{
    private static InventoryBridge instance;
    [SerializeField] private InventorySO inventoryDataSO;
    private static InventorySO staticInventoryData;

    private void Awake()
    {
        instance = this;

        // Connect to inventory systems
        if (inventoryDataSO == null)
        {
            // Try to find from PickUpSystem first
            PickUpSystem pickupSystem = FindObjectOfType<PickUpSystem>();
            if (pickupSystem != null)
            {
                FieldInfo field = pickupSystem.GetType().GetField("inventoryData",
                    BindingFlags.NonPublic | BindingFlags.Instance);

                if (field != null)
                {
                    inventoryDataSO = field.GetValue(pickupSystem) as InventorySO;
                    Debug.Log($"InventoryBridge connected to PickUpSystem's inventoryData ID: {inventoryDataSO?.GetInstanceID()}");
                }
            }

            // If still null, try from InventoryController
            if (inventoryDataSO == null)
            {
                InventoryController controller = FindObjectOfType<InventoryController>();
                if (controller != null)
                {
                    // Either manually assign or access through reflection if needed
                    Debug.LogWarning("Could not find inventoryData automatically. Please assign it manually.");
                }
            }
        }

        // Store for static access
        staticInventoryData = inventoryDataSO;
    }

    // Instance method
    public void AddItemToInventory(string itemName)
    {
        // Find item in resources
        ItemSO itemSO = Resources.Load<ItemSO>("Items/" + itemName);
        if (itemSO != null && inventoryDataSO != null)
        {
            // This uses the public method of InventorySO
            inventoryDataSO.AddItem(itemSO, 1);
            Debug.Log($"Added {itemName} to UI inventory via instance method");
        }
    }

    // Instance method with direct ItemSO reference
    public void AddItemToInventory(ItemSO itemSO, int quantity = 1)
    {
        if (itemSO != null && inventoryDataSO != null)
        {
            inventoryDataSO.AddItem(itemSO, quantity);
            Debug.Log($"Added {quantity}x {itemSO.name} to UI inventory via instance method");
        }
    }

    // Static method for easy access
    public static void AddItem(ItemSO itemSO, int quantity = 1)
    {
        if (instance == null)
        {
            // Create instance if needed
            GameObject go = new GameObject("InventoryBridge");
            instance = go.AddComponent<InventoryBridge>();
            Debug.Log("Created InventoryBridge instance");
        }

        instance.AddItemToInventory(itemSO, quantity);
    }

    // Static method by name
    public static void AddItem(string itemName, int quantity = 1)
    {
        if (instance == null)
        {
            // Create instance if needed
            GameObject go = new GameObject("InventoryBridge");
            instance = go.AddComponent<InventoryBridge>();
        }

        ItemSO itemSO = Resources.Load<ItemSO>("Items/" + itemName);
        if (itemSO != null)
        {
            instance.AddItemToInventory(itemSO, quantity);
        }
    }
}