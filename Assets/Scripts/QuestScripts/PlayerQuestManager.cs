using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerQuestManager : MonoBehaviour //manages the player's active quests
{

    private List<GameQuests> activeQuests = new List<GameQuests>();

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void AcceptQuest(GameQuests quest)
    {
        activeQuests.Add(quest);
        Debug.Log($"[PlayerQuestManager] Quest accepted: {quest.questTitle}, Required: {quest.requiredItem}, Amount: {quest.requiredAmount}, Current: {quest.currentAmount}");

        QuestLogManager questLogManager = FindObjectOfType<QuestLogManager>(); //refresh
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
            activeQuests.Remove(quest); // Remove completed quest
            Debug.Log($"[PlayerQuestManager] Quest completed and removed: {quest.questTitle}");

            QuestLogManager questLogManager = FindObjectOfType<QuestLogManager>(); //refresh
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
        foreach (var quest in activeQuests)
        {
            Debug.Log($"[PlayerQuestManager] Active quest: {quest.questTitle}, Required: {quest.requiredItem}, Current: {quest.currentAmount}/{quest.requiredAmount}");
        }
        return activeQuests;
    }

    public void UpdateQuestProgress(string itemName)
    {
        Debug.Log($"[PlayerQuestManager] Updating progress for item: {itemName}");
        bool updated = false;

        foreach (GameQuests quest in activeQuests)
        {
            Debug.Log($"[PlayerQuestManager] Checking quest: {quest.questTitle}, Required: {quest.requiredItem}, Current: {quest.currentAmount}/{quest.requiredAmount}");

            if (quest.requiredItem == itemName && !quest.isCompleted)
            {
                if (quest.currentAmount < quest.requiredAmount)
                {
                    quest.currentAmount++;
                    updated = true;
                    Debug.Log($"[PlayerQuestManager] Updated progress for {quest.questTitle}: {quest.currentAmount}/{quest.requiredAmount}");
                }
                else
                {
                    Debug.Log($"[PlayerQuestManager] Progress already at max for {quest.questTitle}: {quest.currentAmount}/{quest.requiredAmount}");
                }
            }
        }

        if (!updated)
        {
            Debug.Log($"[PlayerQuestManager] No quests found needing {itemName} or already at max progress");
        }

        QuestLogManager questLogManager = FindObjectOfType<QuestLogManager>(); //refresh
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

            QuestLogManager questLogManager = FindObjectOfType<QuestLogManager>(); //refresh
            if (questLogManager != null)
            {
                questLogManager.UpdateQuestLog();
            }
        }
    }

    /*private void OnTriggerEnter2D(Collider2D other)   //reactivate later
    {
        if (other.CompareTag("Item")) 
        {
            string itemName = other.gameObject.name;
            FindObjectOfType<PlayerQuestManager>().UpdateQuestProgress(itemName);
            Destroy(other.gameObject);
        }
    }
    */
}