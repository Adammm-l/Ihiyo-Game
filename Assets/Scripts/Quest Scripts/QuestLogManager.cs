using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QuestLogManager : MonoBehaviour
{
    [SerializeField] private GameObject questLogPanel;
    [SerializeField] private Transform questContent;
    [SerializeField] private GameObject questTextPrefab;
    private bool isLogOpen = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void ToggleQuestLog()
    {
        isLogOpen = !isLogOpen;
        questLogPanel.SetActive(isLogOpen);

        if (isLogOpen)
        {
            UpdateQuestLog();
        }
    }

    public void UpdateQuestLog()
    {
        //Clear old quest items
        foreach (Transform child in questContent)
        {
            Destroy(child.gameObject);
        }

        //Get active quests from the PlayerQuestManager
        PlayerQuestManager questManager = FindObjectOfType<PlayerQuestManager>();
        List<GameQuests> activeQuests = questManager.GetActiveQuests();

        foreach (GameQuests quest in activeQuests)
        {
            //Instantiate a new quest text prefab
            GameObject questText = Instantiate(questTextPrefab, questContent);
            TextMeshProUGUI textComponent = questText.GetComponent<TextMeshProUGUI>();

            //Set le quest details
            textComponent.text = $"{quest.questTitle}\n{quest.questDescription}\nProgress: {quest.currentAmount}/{quest.requiredAmount}";
        }
    }
}