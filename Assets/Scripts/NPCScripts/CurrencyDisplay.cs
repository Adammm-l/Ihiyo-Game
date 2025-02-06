using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
//adam 
public class CurrencyDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI currencyText;
    private CurrencyManager currencyManager;

    private void Start()
    {
        currencyManager = FindObjectOfType<CurrencyManager>();
        UpdateCurrencyDisplay(currencyManager.GetCurrentCurrency());
        currencyManager.OnCurrencyChanged += UpdateCurrencyDisplay;
    }

    private void OnDestroy()
    {
        currencyManager.OnCurrencyChanged -= UpdateCurrencyDisplay;
    }

    private void UpdateCurrencyDisplay(int amount)
    {
        currencyText.text = $"{amount}x";
    }
}
