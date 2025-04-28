using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerQuestManager : MonoBehaviour
{
    private List<GameQuests> activeQuests = new List<GameQuests>();

    public void AcceptQuest(GameQuests quest)
    {
        activeQuests.Add(quest);

        if (quest.UsesMultipleItems)
        {
            string itemDetails = "";
            foreach (QuestItemRequirement req in quest.requiredItems)
            {
                itemDetails += $"{req.itemName}({req.currentAmount}/{req.amount}), ";
            }
            Debug.Log($"[PlayerQuestManager] Quest accepted: {quest.questTitle}, Required items: {itemDetails}");
        }
        else
        {
            Debug.Log($"[PlayerQuestManager] Quest accepted: {quest.questTitle}, Required: {quest.requiredItem}, Amount: {quest.requiredAmount}, Current: {quest.currentAmount}");
        }

        QuestLogManager questLogManager = FindObjectOfType<QuestLogManager>();
        if (questLogManager != null)
        {
            questLogManager.UpdateQuestLog();
        }
    }

    public void CompleteQuest(GameQuests quest)
    {
        Debug.Log($"[PlayerQuestManager] Attempting to complete quest: {quest.questTitle}");
        if (activeQuests.Contains(quest))
        {
            quest.isCompleted = true;
            quest.isEnabled = false;
            activeQuests.Remove(quest);

            PlayerPrefs.SetInt("Quest_" + quest.questTitle + "_Completed", 1);
            PlayerPrefs.Save();

            Debug.Log($"[PlayerQuestManager] Quest completed and removed: {quest.questTitle}");

            QuestLogManager questLogManager = FindObjectOfType<QuestLogManager>();
            if (questLogManager != null)
            {
                questLogManager.UpdateQuestLog();
            }
        }
        else
        {
            Debug.LogError($"[PlayerQuestManager] Tried to complete quest that's not active: {quest.questTitle}");
        }
    }

    public List<GameQuests> GetActiveQuests()
    {
        Debug.Log($"[PlayerQuestManager] Getting active quests, count: {activeQuests.Count}");
        return activeQuests;
    }

    public void UpdateQuestProgress(string itemName)
    {
        Debug.Log($"[PlayerQuestManager] Updating progress for item: {itemName}");
        bool updated = false;

        foreach (GameQuests quest in activeQuests)
        {
            if (quest.isCompleted) continue;

            bool questUpdated = false;

            // Check for multi-item quests
            if (quest.UsesMultipleItems)
            {
                foreach (QuestItemRequirement req in quest.requiredItems)
                {
                    if (req.itemName == itemName && req.currentAmount < req.amount)
                    {
                        req.currentAmount++;
                        questUpdated = true;
                        updated = true;
                        Debug.Log($"[PlayerQuestManager] Updated progress for {quest.questTitle} - {itemName}: {req.currentAmount}/{req.amount}");
                        break;
                    }
                }
            }
            // Check for legacy single-item quests
            else if (quest.requiredItem == itemName && quest.currentAmount < quest.requiredAmount)
            {
                quest.currentAmount++;
                questUpdated = true;
                updated = true;
                Debug.Log($"[PlayerQuestManager] Updated progress for {quest.questTitle}: {quest.currentAmount}/{quest.requiredAmount}");
            }

            if (questUpdated)
            {
                // Check if quest is now complete - could auto-complete here if desired
            }
        }

        if (!updated)
        {
            Debug.Log($"[PlayerQuestManager] No quests found needing {itemName} or already at max progress");
        }

        QuestLogManager questLogManager = FindObjectOfType<QuestLogManager>();
        if (questLogManager != null)
        {
            questLogManager.UpdateQuestLog();
        }
    }

    public void RemoveQuest(GameQuests quest)
    {
        if (activeQuests.Contains(quest))
        {
            activeQuests.Remove(quest);
            Debug.Log($"[PlayerQuestManager] Removed quest: {quest.questTitle}");

            QuestLogManager questLogManager = FindObjectOfType<QuestLogManager>();
            if (questLogManager != null)
            {
                questLogManager.UpdateQuestLog();
            }
        }
    }
}