using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

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
        // Clear existing displays
        foreach (var display in itemDisplays)
        {
            if (display != null) Destroy(display.gameObject);
        }
        itemDisplays.Clear();

        // Create new displays
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
}
