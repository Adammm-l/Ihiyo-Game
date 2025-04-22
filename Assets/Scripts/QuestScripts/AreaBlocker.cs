using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AreaBlocker : MonoBehaviour
{
    [SerializeField] private string requiredQuestTitle;
    [SerializeField] private bool mustBeCompleted = true;
    [SerializeField] private float messageDuration = 3f;
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private TextMeshProUGUI messageText;

    private PlayerQuestManager questManager;
    private Coroutine hideMessageCoroutine;

    private void Start()
    {
        questManager = FindObjectOfType<PlayerQuestManager>();
        if (messagePanel != null)
            messagePanel.SetActive(false);

        // Check if quest is already completed - if so, destroy this blocker
        if (IsQuestCompleted())
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Update()
    {
        // Continuously check if quest gets completed during gameplay
        if (IsQuestCompleted())
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ShowMessage();
        }
    }

    private bool IsQuestCompleted()
    {
        if (questManager == null)
            questManager = FindObjectOfType<PlayerQuestManager>();

        if (questManager == null)
            return false;

        if (mustBeCompleted)
        {
            // Check if quest is completed via PlayerPrefs
            return PlayerPrefs.GetInt("Quest_" + requiredQuestTitle + "_Completed", 0) == 1;
        }
        else
        {
            // If we're checking for quest being active
            List<GameQuests> activeQuests = questManager.GetActiveQuests();
            return activeQuests.Exists(q => q.questTitle == requiredQuestTitle);
        }
    }

    private void ShowMessage()
    {
        if (messagePanel != null && messageText != null)
        {
            if (hideMessageCoroutine != null)
                StopCoroutine(hideMessageCoroutine);

            messagePanel.SetActive(true);

            hideMessageCoroutine = StartCoroutine(HideMessageAfterDelay());
        }
    }

    private IEnumerator HideMessageAfterDelay()
    {
        yield return new WaitForSeconds(messageDuration);
        if (messagePanel != null)
            messagePanel.SetActive(false);
        hideMessageCoroutine = null;
    }
}