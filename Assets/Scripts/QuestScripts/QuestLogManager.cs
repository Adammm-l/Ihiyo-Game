using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QuestLogManager : MonoBehaviour
{
    [Header("Text Settings")]
    [SerializeField] private TMP_FontAsset questFont;

    [Header("References")]
    [SerializeField] private GameObject questLogPanel;
    [SerializeField] private Transform questContent;
    [SerializeField] private GameObject questTextPrefab;
    KeyCode toggleKey;
    KeybindManager keybindManager;

    private bool isLogOpen = false;

    void Start()
    {
        keybindManager = KeybindManager.Instance;
    }

    private void Update()
    {
        toggleKey = keybindManager.GetKeybind("QuestLog");
        if (NPCInteraction.IsInteracting) return;

        if (Input.GetKeyDown(toggleKey))
        {
            ToggleQuestLog();
        }
    }

    public void ToggleQuestLog()
    {
        isLogOpen = UIManager.Instance.TogglePanel(questLogPanel);

        if (isLogOpen)
        {
            UpdateQuestLog();
        }
    }

    public void UpdateQuestLog()
    {
        foreach (Transform child in questContent)
        {
            if (child.gameObject != questTextPrefab)
            {
                Destroy(child.gameObject);
            }
        }

        PlayerQuestManager questManager = FindObjectOfType<PlayerQuestManager>();
        if (questManager == null) return;
        List<GameQuests> activeQuests = questManager.GetActiveQuests();

        foreach (GameQuests quest in activeQuests)
        {
            GameObject questText = Instantiate(questTextPrefab, questContent);
            TextMeshProUGUI textComponent = questText.GetComponent<TextMeshProUGUI>();

            if (questFont != null)
            {
                textComponent.font = questFont;
            }
            string progressText;

            if (quest.UsesMultipleItems)
            {
                string itemProgress = "";
                foreach (QuestItemRequirement req in quest.requiredItems)
                {
                    itemProgress += $"\n- {req.itemName}: {req.currentAmount}/{req.amount}";
                }
                progressText = $"{quest.questTitle}\n{quest.questDescription}{itemProgress}";
            }
            else
            {
                progressText = $"{quest.questTitle}\n{quest.questDescription}\nProgress: {quest.currentAmount}/{quest.requiredAmount}";
            }

            textComponent.text = progressText;
        }
    }

    public void OnQuestLogButtonClick()
    {
        if (!NPCInteraction.IsInteracting)
        {
            ToggleQuestLog();
        }
    }
}