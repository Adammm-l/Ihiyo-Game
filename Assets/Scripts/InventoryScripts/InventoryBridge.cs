using UnityEngine;
using InventoryModel;
using InventoryCTRL;

public class InventoryBridge : MonoBehaviour
{
    [SerializeField] private InventorySO inventoryDataSO;

    private void Awake()
    {
        // Find the reference if not assigned
        if (inventoryDataSO == null)
        {
            InventoryController controller = FindObjectOfType<InventoryController>();
            if (controller != null)
            {
            }
        }
    }

    public void AddItemToInventory(string itemName)
    {
        // Find item in resources
        ItemSO itemSO = Resources.Load<ItemSO>("Items/" + itemName);
        if (itemSO != null && inventoryDataSO != null)
        {
            // This uses the public method of InventorySO
            inventoryDataSO.AddItem(itemSO, 1);
        }
    }
}