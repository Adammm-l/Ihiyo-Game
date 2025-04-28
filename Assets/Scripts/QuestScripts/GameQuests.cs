using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class QuestItemRequirement
{
    public string itemName;
    public int amount = 1;
    public int currentAmount = 0;
}

[Serializable]
public class GameQuests
{
    [SerializeField] public bool isEnabled = false;
    public bool IsEnabled
    {
        get => isEnabled;
        set => isEnabled = value;
    }
    public string questTitle;
    public string questDescription;
    public bool isCompleted;

    // Legacy single item support
    public string requiredItem;
    public int requiredAmount;
    public int currentAmount;

    // New multiple items support
    public List<QuestItemRequirement> requiredItems = new List<QuestItemRequirement>();

    public string completionNPC;
    public string giverIncompleteResponse;
    public string completionCompleteResponse;
    public string completionIncompleteResponse;

    // Helper property to check if using new system
    public bool UsesMultipleItems => requiredItems != null && requiredItems.Count > 0;

    public bool IsQuestCompleted()
    {
        if (UsesMultipleItems)
        {
            foreach (QuestItemRequirement req in requiredItems)
            {
                if (req.currentAmount < req.amount)
                {
                    return false;
                }
            }
            return true;
        }
        else
        {
            return currentAmount >= requiredAmount;
        }
    }
}