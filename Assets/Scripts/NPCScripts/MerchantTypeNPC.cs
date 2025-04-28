using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using InventoryModel;
//adam
[System.Serializable]
public class ShopItem //Items in the shop and their names
{
    public string itemName;
    public int itemPrice;
    public string itemDescription;
}
public class MerchantTypeNPC : MonoBehaviour
{
    [Header("Shop Items")]
    [SerializeField] private List<ShopItem> shopItems = new List<ShopItem>();

    [Header("UI References")]
    [SerializeField] private GameObject shopUI;
    [SerializeField] private ShopItemUI itemPrefab;
    [SerializeField] private Transform itemContainer;

    private List<ShopItemUI> itemDisplays = new List<ShopItemUI>();

    void Start()
    {
        shopUI.SetActive(false);
        CreateItemDisplays();
    }

    private void CreateItemDisplays()
    {
        //Clear existing displays
        foreach (var display in itemDisplays)
        {
            if (display != null) Destroy(display.gameObject);
        }
        itemDisplays.Clear();

        //Create new displays
        foreach (var item in shopItems)
        {
            ShopItemUI newDisplay = Instantiate(itemPrefab, itemContainer);
            itemDisplays.Add(newDisplay);
        }
    }

    public void OpenShop()
    {
        if (shopUI == null) return;
        shopUI.SetActive(true);
        PopulateShopUI();

        // Update currency display in both ways
        CurrencyManager.Instance.UpdateCurrencyDisplay();

        // Also directly update any CurrencyDisplay components in the shop UI
        CurrencyDisplay[] displays = shopUI.GetComponentsInChildren<CurrencyDisplay>(true);
        foreach (var display in displays)
        {
            display.ForceUpdate();
        }
    }

    public void CloseShop()
    {
        if (shopUI == null) return;
        shopUI.SetActive(false);
    }

    private void PopulateShopUI()
    {
        for (int i = 0; i < shopItems.Count; i++)
        {
            if (i < itemDisplays.Count)
            {
                itemDisplays[i].SetItemDetails(shopItems[i], this);
                itemDisplays[i].gameObject.SetActive(true);
            }
        }
    }

    public void BuyItem(ShopItem item)
    {
        if (CurrencyManager.Instance.SpendCurrency(item.itemPrice))
        {
            Debug.Log($"Successfully bought {item.itemName} for {item.itemPrice}");

            // Add item to quest inventory
            Inventory questInventory = FindObjectOfType<Inventory>();
            if (questInventory != null)
            {
                questInventory.AddItem(item.itemName);
            }

            // Add item to UI inventory
            ItemSO itemSO = Resources.Load<ItemSO>("Items/" + item.itemName);
            if (itemSO != null)
            {
                InventoryBridge.AddItem(itemSO, 1);
            }

            // Show notification
            NPCInteraction npcInteraction = GetComponent<NPCInteraction>();
            npcInteraction.ShowPurchaseNotification(item.itemName, 1);
        }
    }
}