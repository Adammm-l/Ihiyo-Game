using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventoryCTRL;
using InventoryModel;

// This modified CurrencyManager uses the inventory system to track coins
public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    [SerializeField] private string currencyItemName = "Coin"; // The item to use as currency

    // Event that other classes can subscribe to
    public event System.Action<int> OnCurrencyChanged;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    // Get current coin count from inventory
    public int GetCurrentCurrency()
    {
        Inventory questInventory = FindObjectOfType<Inventory>();
        if (questInventory != null)
        {
            return questInventory.GetItemCount(currencyItemName);
        }
        return 0;
    }

    // Spend currency by removing coins from inventory
    public bool SpendCurrency(int amount)
    {
        if (GetCurrentCurrency() >= amount)
        {
            // Remove from quest inventory
            Inventory questInventory = FindObjectOfType<Inventory>();
            if (questInventory != null)
                questInventory.RemoveItem(currencyItemName, amount);

            // Remove from UI inventory
            InventoryController inventoryController = FindObjectOfType<InventoryController>();
            if (inventoryController != null)
            {
                inventoryController.RemoveItemByName(currencyItemName, amount);
            }

            // Notify subscribers
            NotifyCurrencyChanged();
            return true;
        }

        return false;
    }

    // Add currency by adding coins to inventory
    public void AddCurrency(int amount)
    {
        Inventory questInventory = FindObjectOfType<Inventory>();
        if (questInventory != null)
        {
            for (int i = 0; i < amount; i++)
            {
                questInventory.AddItem(currencyItemName);
            }
        }

        ItemSO itemSO = Resources.Load<ItemSO>("Items/" + currencyItemName);
        if (itemSO != null)
        {
            InventoryBridge.AddItem(itemSO, amount);
        }

        // Notify subscribers
        NotifyCurrencyChanged();
    }

    // Public method to force a currency update notification
    public void UpdateCurrencyDisplay()
    {
        NotifyCurrencyChanged();
    }

    // Private method to trigger the event
    private void NotifyCurrencyChanged()
    {
        OnCurrencyChanged?.Invoke(GetCurrentCurrency());
    }
}