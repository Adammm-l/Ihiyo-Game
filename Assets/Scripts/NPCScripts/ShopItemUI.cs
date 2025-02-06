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
        Debug.Log($"Setting item: {item.itemName}");
        Debug.Log($"TextMeshPro components: Name={itemNameText != null}, Price={itemPriceText != null}, Desc={itemDescriptionText != null}");
        Debug.Log($"Current text values - Name: '{itemNameText.text}', Price: '{itemPriceText.text}', Desc: '{itemDescriptionText.text}'");

        currentItem = item;
        merchant = merchantNPC;

        itemNameText.text = item.itemName;
        itemPriceText.text = $"Price: {item.itemPrice}";
        itemDescriptionText.text = item.itemDescription;

        Debug.Log($"After setting - Name: '{itemNameText.text}', Price: '{itemPriceText.text}', Desc: '{itemDescriptionText.text}'");
    }

}
