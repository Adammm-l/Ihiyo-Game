using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemUI : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemPriceText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    [SerializeField] private Button buyButton;

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

        itemIcon.sprite = item.itemIcon;
        itemNameText.text = item.itemName;
        itemPriceText.text = $"{item.itemPrice} Coin(s)";

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => merchant.BuyItem(currentItem));
    }
}
