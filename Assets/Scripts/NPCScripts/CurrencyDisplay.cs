using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CurrencyDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI currencyText;
    private CurrencyManager currencyManager;
    private bool isSubscribed = false;

    private void OnEnable()
    {
        SetupSubscription();
    }

    private void Start()
    {
        SetupSubscription();
    }

    private void SetupSubscription()
    {
        if (isSubscribed) return;

        currencyManager = FindObjectOfType<CurrencyManager>();
        if (currencyManager != null)
        {
            UpdateCurrencyDisplay(currencyManager.GetCurrentCurrency());
            currencyManager.OnCurrencyChanged += UpdateCurrencyDisplay;
            isSubscribed = true;
        }
    }

    private void OnDisable()
    {
        if (currencyManager != null && isSubscribed)
        {
            currencyManager.OnCurrencyChanged -= UpdateCurrencyDisplay;
            isSubscribed = false;
        }
    }

    private void OnDestroy()
    {
        if (currencyManager != null && isSubscribed)
        {
            currencyManager.OnCurrencyChanged -= UpdateCurrencyDisplay;
            isSubscribed = false;
        }
    }

    private void UpdateCurrencyDisplay(int amount)
    {
        if (currencyText != null)
        {
            currencyText.text = $"{amount}x";
        }
    }

    // Add this method to force an update
    public void ForceUpdate()
    {
        if (currencyManager != null)
        {
            UpdateCurrencyDisplay(currencyManager.GetCurrentCurrency());
        }
    }
}