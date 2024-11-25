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
        Debug.Log($"Quest accepted: {quest.questTitle}");
    }

    public void CompleteQuest(GameQuests quest)
    {
        if (activeQuests.Contains(quest))
        {
            quest.isCompleted = true;
            activeQuests.Remove(quest); // Remove completed quest
        }
    }
    public List<GameQuests> GetActiveQuests()
    {
        return activeQuests;
    }

    public void UpdateQuestProgress(string itemName)
    {
        foreach (GameQuests quest in activeQuests)
        {
            if (quest.requiredItem == itemName && !quest.isCompleted)
            {
                quest.currentAmount++;
                if (quest.currentAmount >= quest.requiredAmount)
                {
                    CompleteQuest(quest);
                }
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
