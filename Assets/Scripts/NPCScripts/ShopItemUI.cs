using UnityEngine;
using UnityEngine.UI;
using TMPro;
//adam
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
        //buyButton.onClick.AddListener(OnBuyClicked);
        CurrencyManager.Instance.OnCurrencyChanged += OnCurrencyChanged;
        Debug.Log($"ShopItemUI Start called for {gameObject.name}");
    }

    public void SetItemDetails(ShopItem item, MerchantTypeNPC merchantNPC)
    {
        currentItem = item;
        merchant = merchantNPC;
        itemNameText.text = item.itemName;
        itemPriceText.text = $"Buy: ${item.itemPrice}";
        itemDescriptionText.text = item.itemDescription;
        UpdateButtonState();
    }

    private void UpdateButtonState()
    { 
        bool canAfford = CurrencyManager.Instance.GetCurrentCurrency() >= currentItem.itemPrice;
        Image buttonImage = buyButton.GetComponent<Image>();
        Color buttonColor = buttonImage.color;
        buttonColor.a = canAfford ? 1f : 0.5f;
        buttonImage.color = buttonColor;
        buyButton.interactable = canAfford;
    }

    private void OnBuyClicked()
    {
        merchant.BuyItem(currentItem);
    }

    private void OnDestroy()
    {
        buyButton.onClick.RemoveListener(OnBuyClicked);
        CurrencyManager.Instance.OnCurrencyChanged -= OnCurrencyChanged;
    }

    private void OnCurrencyChanged(int newAmount)
    {
        UpdateButtonState();
    }
}