using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{

    public GameObject dialoguePanel;     //dialogue panel
    public TextMeshProUGUI npcNameText;   //npcName box
    public TextMeshProUGUI dialogueText;   //npc dialogue text box
    public GameObject responsePanel;      //response panel
    public Button[] responseButtons;    //Buttons for player responses
    public static bool IsMultipleChoiceActive = false; //to fix a bug where quests are duplicated when E is pressed on multiple choice prompts


    private System.Action<int> onResponseSelected; //Callback for the selected response

    // Start is called before the first frame update
    void Start()
    {
        dialoguePanel.SetActive(false);  
        responsePanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowDialogue(string npcName, string message)
    {
        npcNameText.text = npcName;
        dialogueText.text = message;
        dialoguePanel.SetActive(true);
        responsePanel.SetActive(false);
    }

    public void ShowResponses(string[] responses, System.Action<int> callback)
    {
        IsMultipleChoiceActive = true;
        responsePanel.SetActive(true);
        dialoguePanel.SetActive(true);
        onResponseSelected = callback;

        // Enable and set text for each response button
        for (int i = 0; i < responseButtons.Length; i++)
        {
            if (i < responses.Length)
            {
                responseButtons[i].gameObject.SetActive(true);
                responseButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = responses[i];

                int responseIndex = i; //the index for the buttton
                //code for action when clicking on a response box
                responseButtons[i].onClick.RemoveAllListeners();
                responseButtons[i].onClick.AddListener(() => SelectResponse(responseIndex));
            }
            else
            {
                responseButtons[i].gameObject.SetActive(false); //Hide unused buttons
            }
        }
    }

    private void SelectResponse(int responseIndex)
    {
        responsePanel.SetActive(false);
        onResponseSelected?.Invoke(responseIndex);
        IsMultipleChoiceActive = false;
    }

    public void HideDialogue()
    {
        dialoguePanel.SetActive(false);
        responsePanel.SetActive(false);
    }
}
