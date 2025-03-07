using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Quest Scripts Section done by Adam
public class Inventory : MonoBehaviour
{
    private Dictionary<string, int> items = new Dictionary<string, int>();
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void AddItem(string itemName)
    {
        if (items.ContainsKey(itemName))
        {
            items[itemName]++;
        }
        else
        {
            items[itemName] = 1;
        }

        Debug.Log($"Added {itemName}. Total: {items[itemName]}");

        // Update quest progress when an item is added
        PlayerQuestManager questManager = FindObjectOfType<PlayerQuestManager>();
        if (questManager != null)
        {
            questManager.UpdateQuestProgress(itemName);
        }
    }

    public int GetItemCount(string itemName)
    {
        return items.ContainsKey(itemName) ? items[itemName] : 0;
    }

    public void RemoveItem(string itemName, int amount)
    {
        if (items.ContainsKey(itemName))
        {
            items[itemName] -= amount;
            if (items[itemName] <= 0)
            {
                items.Remove(itemName); //Remove item completely if count reaches 0
            }
            Debug.Log($"Removed {amount} {itemName}(s). Remaining: {GetItemCount(itemName)}");
        }
    }
}