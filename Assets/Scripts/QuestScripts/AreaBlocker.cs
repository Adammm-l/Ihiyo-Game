using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
//Adam
public class AreaBlocker : MonoBehaviour
{
    [SerializeField] private string requiredQuestTitle;
    [SerializeField] private bool mustBeCompleted = true;
    [SerializeField] private bool activateAfterQuestGiven = false;
    [SerializeField] private float messageDuration = 1.8f;
    [SerializeField] private GameObject messagePanel;
    [SerializeField] private TextMeshProUGUI messageText;

    [SerializeField] private Collider2D triggerCollider;
    [SerializeField] private Collider2D blockingCollider;

    private PlayerQuestManager questManager;
    private Coroutine hideMessageCoroutine;
    private bool hasBeenActivated = false;

    private void Start()
    {
        questManager = FindObjectOfType<PlayerQuestManager>();
        if (messagePanel != null)
            messagePanel.SetActive(false);

        if (triggerCollider == null || blockingCollider == null)
        {
            Collider2D[] colliders = GetComponents<Collider2D>();
            foreach (Collider2D col in colliders)
            {
                if (col.isTrigger)
                    triggerCollider = col;
                else
                    blockingCollider = col;
            }
        }

        if (activateAfterQuestGiven)
        {
            if (triggerCollider) triggerCollider.enabled = false;
            if (blockingCollider) blockingCollider.enabled = false;
        }
        else if (IsQuestCompleted())
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (!activateAfterQuestGiven && IsQuestCompleted())
        {
            Destroy(gameObject);
        }

        if (activateAfterQuestGiven && !hasBeenActivated && IsQuestActive())
        {
            if (triggerCollider) triggerCollider.enabled = true;
            if (blockingCollider) blockingCollider.enabled = true;
            hasBeenActivated = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ShowMessage();
        }
    }

    private bool IsQuestActive()
    {
        if (questManager == null)
            questManager = FindObjectOfType<PlayerQuestManager>();

        if (questManager == null)
            return false;

        List<GameQuests> activeQuests = questManager.GetActiveQuests();
        return activeQuests.Exists(q => q.questTitle == requiredQuestTitle);
    }

    private bool IsQuestCompleted()
    {
        if (questManager == null)
            questManager = FindObjectOfType<PlayerQuestManager>();

        if (questManager == null)
            return false;

        if (mustBeCompleted)
        {
            return PlayerPrefs.GetInt("Quest_" + requiredQuestTitle + "_Completed", 0) == 1;
        }
        else
        {
            return IsQuestActive();
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