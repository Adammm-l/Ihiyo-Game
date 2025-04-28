using UnityEngine;
using InventoryModel;
using InventoryCTRL;
using System.Reflection;

public class InventoryBridge : MonoBehaviour
{
    private static InventoryBridge instance;
    public InventorySO inventoryDataSO;
    private static InventorySO staticInventoryData;

    private void Awake()
    {
        instance = this;

        if (inventoryDataSO == null)
        {
            PickUpSystem pickupSystem = FindObjectOfType<PickUpSystem>();
            if (pickupSystem != null)
            {
                FieldInfo field = pickupSystem.GetType().GetField("inventoryData", BindingFlags.NonPublic | BindingFlags.Instance);

                if (field != null)
                {
                    inventoryDataSO = field.GetValue(pickupSystem) as InventorySO;
                }
            }
            if (inventoryDataSO == null)
            {
                InventoryController controller = FindObjectOfType<InventoryController>();
            }
        }
        staticInventoryData = inventoryDataSO;
    }

    public void AddItemToInventory(string itemName)
    {
        ItemSO itemSO = Resources.Load<ItemSO>("Items/" + itemName);
        if (itemSO != null && inventoryDataSO != null)
        {
            inventoryDataSO.AddItem(itemSO, 1);
        }
    }

    public void AddItemToInventory(ItemSO itemSO, int quantity = 1)
    {
        if (itemSO != null && inventoryDataSO != null)
        {
            inventoryDataSO.AddItem(itemSO, quantity);
        }
    }

    public static void AddItem(ItemSO itemSO, int quantity = 1)
    {
        if (instance == null)
        {
            GameObject go = new GameObject("InventoryBridge");
            instance = go.AddComponent<InventoryBridge>();
            Debug.Log("Created InventoryBridge instance");
        }

        instance.AddItemToInventory(itemSO, quantity);
    }

    public static void AddItem(string itemName, int quantity = 1)
    {
        if (instance == null)
        {
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