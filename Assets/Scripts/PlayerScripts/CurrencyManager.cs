using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    [SerializeField] private int startingCurrency = 0;

    private int currentCurrency;
    private const string CURRENCY_SAVE_KEY = "PlayerCurrency";

    public event Action<int> OnCurrencyChanged;

    private void Awake()
    {
        LoadCurrency();
    }

    private void LoadCurrency()
    {
        currentCurrency = PlayerPrefs.GetInt(CURRENCY_SAVE_KEY, startingCurrency);
    }

    public void AddCurrency(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("Attempted to add negative currency. Use SpendCurrency instead.");
            return;
        }

        currentCurrency += amount;
        SaveCurrency();
        OnCurrencyChanged?.Invoke(currentCurrency);
    }

    public bool SpendCurrency(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("Attempted to spend negative currency. Use AddCurrency instead.");
            return false;
        }

        if (currentCurrency >= amount)
        {
            currentCurrency -= amount;
            SaveCurrency();
            OnCurrencyChanged?.Invoke(currentCurrency);
            return true;
        }

        return false;
    }

    public int GetCurrentCurrency()
    {
        return currentCurrency;
    }

    private void SaveCurrency()
    {
        PlayerPrefs.SetInt(CURRENCY_SAVE_KEY, currentCurrency);
        PlayerPrefs.Save();
    }
}