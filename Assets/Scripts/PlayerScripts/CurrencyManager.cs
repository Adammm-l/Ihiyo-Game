using UnityEngine;
using System;

public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    [SerializeField] private int startingCurrency = 0;
    private int currentCurrency;
    private const string CURRENCY_SAVE_KEY = "PlayerCurrency";

    public event Action<int> OnCurrencyChanged;

    private void Start()
    {
        ResetCurrency();  //for debugging; get rid of it later
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        LoadCurrency();
    }

    private void LoadCurrency()
    {
        currentCurrency = PlayerPrefs.GetInt(CURRENCY_SAVE_KEY, startingCurrency);
        Debug.Log($"Loaded currency: {currentCurrency}");
    }

    public void AddCurrency(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("Attempted to add negative currency");
            return;
        }

        currentCurrency += amount;
        SaveCurrency();
        OnCurrencyChanged?.Invoke(currentCurrency);
        Debug.Log($"Added {amount} currency. New total: {currentCurrency}");
    }

    public bool SpendCurrency(int amount)
    {
        Debug.Log($"Attempting to spend {amount} currency. Current total: {currentCurrency}");
        if (amount < 0)
        {
            Debug.LogWarning("Attempted to spend negative currency");
            return false;
        }

        if (currentCurrency >= amount)
        {
            currentCurrency -= amount;
            SaveCurrency();
            OnCurrencyChanged?.Invoke(currentCurrency);
            Debug.Log($"Successfully spent {amount} currency. New total: {currentCurrency}");
            return true;
        }

        Debug.Log("Not enough currency to spend");
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

    public void ResetCurrency()
    {
        currentCurrency = startingCurrency;
        SaveCurrency();
        OnCurrencyChanged?.Invoke(currentCurrency);
    }


}