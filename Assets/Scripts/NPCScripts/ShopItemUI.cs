using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static UnityEditor.Progress;

public class ShopItemUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemPriceText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    [SerializeField] private Button buyButton;

    private ShopItem currentItem;
    private MerchantTypeNPC merchant;

    private void Start()
    {
        if (buyButton != null)
        {
            buyButton.onClick.AddListener(OnBuyClicked);
        }
    }

    public void SetItemDetails(ShopItem item, MerchantTypeNPC merchantNPC)
    {
        currentItem = item;
        merchant = merchantNPC;

        if (itemNameText != null) itemNameText.text = item.itemName;
        if (itemPriceText != null) itemPriceText.text = $"${item.itemPrice}";
        if (itemDescriptionText != null) itemDescriptionText.text = item.itemDescription;
    }

    private void OnBuyClicked()
    {
        if (merchant != null)
        {
            merchant.BuyItem(currentItem);
        }
    }

    private void OnDestroy()
    {
        if (buyButton != null)
        {
            buyButton.onClick.RemoveListener(OnBuyClicked);
        }
    }
}
