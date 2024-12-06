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
    [SerializeField] private ShopItemUI[] itemDisplays;

    // Start is called before the first frame update
    void Start()
    {
        shopUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
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
        for (int i = 0; i < itemDisplays.Length; i++)
        {
            if (i < shopItems.Count)
            {
                itemDisplays[i].SetItemDetails(shopItems[i], this);
                itemDisplays[i].gameObject.SetActive(true);
            }
            else
            {
                itemDisplays[i].gameObject.SetActive(false); //Hide unused slots
            }
        }
    }

    public void BuyItem(ShopItem item)
    {
    }
}
