using UnityEngine;

public class GhostAbilityUnlocker : MonoBehaviour
{
    [SerializeField] private int dialogueSegmentToUnlock = 0;

    private NPCInteraction npcInteraction;
    private bool hasCheckedThisSegment = false;

    void Start()
    {
        npcInteraction = GetComponent<NPCInteraction>();

        if (npcInteraction == null)
        {
            Debug.LogError("GhostAbilityUnlocker requires an NPCInteraction component!");
            enabled = false;
        }
    }

    void Update()
    {
        if (!NPCInteraction.IsInteracting)
        {
            hasCheckedThisSegment = false;
            return;
        }

        if (hasCheckedThisSegment) return;

        System.Reflection.FieldInfo field = typeof(NPCInteraction).GetField(
            "dialogueSegmentIndex",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance
        );

        if (field != null)
        {
            int currentSegment = (int)field.GetValue(npcInteraction);
            if (currentSegment == dialogueSegmentToUnlock)
            {
                UnlockGhostAbility();
                hasCheckedThisSegment = true;
            }
        }
    }

    void UnlockGhostAbility()
    {
        PlayerControl player = FindObjectOfType<PlayerControl>();
        if (player == null) return;

        SwitchPlayerForm playerForm = player.GetComponent<SwitchPlayerForm>();
        if (playerForm == null) return;

        playerForm.canTransform = true;

        SaveController saveManager = FindObjectOfType<SaveController>();
        if (saveManager != null)
        {
            int activeSlot = saveManager.GetActiveSlot();
            SaveData saveData = saveManager.GetSaveData(activeSlot);
            saveData.canTransform = true;
        }
    }
}