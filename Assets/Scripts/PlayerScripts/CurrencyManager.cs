using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventoryCTRL;
using InventoryModel;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    [SerializeField] private string currencyItemName = "Coin";

    public event System.Action<int> OnCurrencyChanged;

    private void Awake()
    {
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

    public int GetCurrentCurrency()
    {
        Inventory questInventory = FindObjectOfType<Inventory>();
        if (questInventory != null)
        {
            return questInventory.GetItemCount(currencyItemName);
        }
        return 0;
    }

    public bool SpendCurrency(int amount)
    {
        if (GetCurrentCurrency() >= amount)
        {
            Inventory questInventory = FindObjectOfType<Inventory>();
            if (questInventory != null)
                questInventory.RemoveItem(currencyItemName, amount);

            InventoryController inventoryController = FindObjectOfType<InventoryController>();
            inventoryController.RemoveItemByName(currencyItemName, amount);

            NotifyCurrencyChanged();
            return true;
        }

        return false;
    }

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
        InventoryBridge.AddItem(itemSO, amount);
        NotifyCurrencyChanged();
    }

    public void UpdateCurrencyDisplay()
    {
        NotifyCurrencyChanged();
    }

    private void NotifyCurrencyChanged()
    {
        OnCurrencyChanged?.Invoke(GetCurrentCurrency());
    }
}