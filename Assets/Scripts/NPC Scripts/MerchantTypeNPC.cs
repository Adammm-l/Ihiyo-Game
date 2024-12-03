using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class ShopItem //Items in the shop and their names
{
    public string itemName;
    public Sprite itemIcon;
    public int itemPrice;
}
public class MerchantTypeNPC : MonoBehaviour
{
    [Header("Shop Items")]
    [SerializeField] private List<ShopItem> shopItems = new List<ShopItem>();

    [Header("UI References")]
    [SerializeField] private GameObject shopUI;
    [SerializeField] private Transform itemDisplayParent;
    [SerializeField] private GameObject itemPrefab;

    //private PlayerInventory playerInventory;

    // Start is called before the first frame update
    void Start()
    {
        shopUI.SetActive(false);
        //playerInventory = FindObjectOfType<PlayerInventory>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            OpenShop();
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
        ClearShopUI();
    }

    private void PopulateShopUI()
    {
        ClearShopUI();

        foreach (var shopItem in shopItems)
        {
            GameObject itemInstance = Instantiate(itemPrefab, itemDisplayParent);
            ShopItemUI itemUI = itemInstance.GetComponent<ShopItemUI>();
        }
    }

    private void ClearShopUI()
    {
        foreach (Transform child in itemDisplayParent)
        {
            Destroy(child.gameObject);
        }
    }

    public void BuyItem(ShopItem item)
    {
        //if (playerInventory == null || item == null) return;

        //if (playerInventory.Currency >= item.itemPrice)
        {
            //playerInventory.Currency -= item.itemPrice;
            //playerInventory.AddItem(item.itemName);
            //Debug.Log($"Bought {item.itemName} for {item.itemPrice} currency!");
        }
        //else
        {
            //Debug.Log("Not enough currency!");
        }
    }

}
