using UnityEngine;
using UnityEngine.Events;
using TMPro;
using InventoryCTRL;

public class PillarInteraction : MonoBehaviour
{
    [Header("Interaction Requirements")]
    [SerializeField] private string requiredItemName;
    [SerializeField] private int requiredAmount = 1;
    [SerializeField] private GameObject interactionText;

    [Header("Messages")]
    [SerializeField] private string successMessage = "The pillar accepts your offering...";
    [SerializeField] private string failureMessage = "This pillar requires a specific offering...";

    [Header("Events")]
    public UnityEvent onObjectInteracted;

    private ObjectSnap objectSnap;
    private KeybindManager keybindManager;
    private KeyCode interactKey;
    private bool isPlayerInRange = false;
    private bool hasBeenActivated = false;
    private DialogueManager dialogueManager;

    void Start() //***COMMENTS FOR TERRENCE WHEN HE FIXES COLLIDER POSSESSION*** (i didn't even need them lmao)
    {
        objectSnap = GetComponent<ObjectSnap>();
        keybindManager = KeybindManager.Instance;
        dialogueManager = FindObjectOfType<DialogueManager>();

        TextMeshProUGUI interactionTextBox = interactionText.GetComponent<TextMeshProUGUI>();
        interactKey = keybindManager.GetKeybind("Interact");
        string interactButton = interactKey.ToString();
        interactionTextBox.text = $"\"{interactButton}\" to interact";
        interactionText.SetActive(false);
    }

    void Update()
    {
        if (DialogueManager.IsMultipleChoiceActive) return;

        // Only allow interaction if pillar is snapped, player in range, and not yet activated
        if (isPlayerInRange && objectSnap != null && objectSnap.hasSnappedObject && !hasBeenActivated)
        {
            if (Input.GetKeyDown(interactKey))
            {
                Interact();
            }
        }
    }

    // This function will be called by the InteractionCollider's trigger events
    public void SetPlayerInRange(bool inRange)
    {
        isPlayerInRange = inRange;

        // Only show interaction text if conditions are met
        if (inRange && objectSnap.hasSnappedObject && !hasBeenActivated)
        {
            interactionText.SetActive(true);
        }
        else
        {
            interactionText.SetActive(false);
            if (!inRange && dialogueManager != null)
            {
                dialogueManager.HideDialogue();
            }
        }
    }

    private void Interact()
    {
        PlayerControl player = FindObjectOfType<PlayerControl>();

        // Check if player is in ghost form
        SwitchPlayerForm playerForm = player.GetComponent<SwitchPlayerForm>();
        if (playerForm != null && playerForm.isGhost)
        {
            return;
        }

        player.canMove = false;
        NPCInteraction.IsInteracting = true;

        // Check if player has the required item
        Inventory playerInventory = FindObjectOfType<Inventory>();
        if (playerInventory != null)
        {
            int playerItemCount = playerInventory.GetItemCount(requiredItemName);

            if (playerItemCount >= requiredAmount)
            {
                // Success - remove item and trigger event
                RemoveItemFromPlayer(requiredItemName, requiredAmount);

                if (dialogueManager != null)
                {
                    dialogueManager.ShowDialogue("Pillar", successMessage);
                }

                hasBeenActivated = true;
                interactionText.SetActive(false);
                onObjectInteracted.Invoke();
            }
            else
            {
                // Failure - show message
                dialogueManager.ShowDialogue("Pillar", failureMessage);

            }
        }
        player.canMove = true;
        NPCInteraction.IsInteracting = false;
    }

    private void RemoveItemFromPlayer(string itemName, int amount)
    {
        Inventory questInventory = FindObjectOfType<Inventory>();
        questInventory.RemoveItem(itemName, amount);

        InventoryController inventoryController = FindObjectOfType<InventoryController>();
        inventoryController.RemoveItemByName(itemName, amount);

    }
}