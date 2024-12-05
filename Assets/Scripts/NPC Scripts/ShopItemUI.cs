using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static UnityEditor.Progress;

public class ShopItemUI : MonoBehaviour
{
    [Header("Item Details (Text Fields)")]
    [SerializeField] private string itemName;
    [SerializeField] private string itemPrice;
    [SerializeField] private string itemDescription;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemPriceText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;

    private ShopItem currentItem;
    private MerchantTypeNPC merchant;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void SetItemDetails(ShopItem item, MerchantTypeNPC merchantNPC)
    {
        currentItem = item;
        merchant = merchantNPC;

        itemName = item.itemName;
        itemPrice = item.itemPrice.ToString();
        itemDescription = item.itemDescription;

        if (itemNameText != null) itemNameText.text = itemName;
        if (itemPriceText != null) itemPriceText.text = $"Price: {itemPrice}";
        if (itemDescriptionText != null) itemDescriptionText.text = itemDescription;

    }

}
