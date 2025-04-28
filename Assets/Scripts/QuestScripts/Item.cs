using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{

    [SerializeField] private string itemName; //name of a collectable object/item
    [SerializeField] private GameObject interactionText; //"E To Pick Up" text
    public string ItemName => itemName;

    // Start is called before the first frame update
    void Start()
    {
        if (interactionText != null)
        {
            interactionText.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && interactionText != null)
        {
            if (itemName == "Time Stone")
            {
                PuzzleCompletionChecker puzzleChecker = FindObjectOfType<PuzzleCompletionChecker>();
                if (!puzzleChecker.isPuzzleFinished)
                {
                    return;
                }
            }
            interactionText.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && interactionText != null)
        {
            interactionText.SetActive(false);
        }
    }
}
