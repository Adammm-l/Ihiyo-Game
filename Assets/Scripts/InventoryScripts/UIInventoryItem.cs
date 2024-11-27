using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

public class UIInventoryItem : MonoBehaviour
{
    // Set up general settings
    [SerializeField] private Image itemIMG;
    
    [SerializeField] private TMP_Text quantTXT;

    [SerializeField] private Image borderIMG;

    public event Action<UIInventoryItem> OnItemCLicked, OnItemDroppedOn, OnItemBeginDrag, OnItemEndDrag, OnRightMouseBtnClick; // Actions to take when certains actions happen to item

    private bool empty = true; 

    public void Awake() {

        ResetData();
        Deselect();
    }

    public void ResetData() { // Behavior for resetting data

        this.itemIMG.gameObject.SetActive(false); // Hide Item quantity and Item image to make it look empty
        this.empty = true;

    }

    public void Deselect() {

        this.borderIMG.enabled = false; // Hide Cursor

    }

    public void SetData(Sprite sprite, int quant) { // Creating an Item and its Params

        this.itemIMG.gameObject.SetActive(true); // Enable Image
        this.itemIMG.sprite = sprite; // Set to passed Sprite
        this.quantTXT.text = quant + ""; // Turn this into a String
        this.empty = false;

    }

    public void Select() {

        this.borderIMG.enabled = true; // Show Cursor

    }

    public void OnBeginDrag() {

        if (empty) { // Check if item is empty so that we don't drag

            return;

        }

        OnItemBeginDrag?.Invoke(this); // Otherwise, check if null, then call OnItemBeginDrag

    }

    public void OnDrop() {

        OnItemDroppedOn?.Invoke(this); // Same Logic as earlier

    }

    public void OnEndDrag() {

        OnItemEndDrag?.Invoke(this);

    }

    public void OnPointerClick(BaseEventData data) { // Branching Path where we determine which behavior to run depending on which Mouse Click is used

        if (empty) { // Don't Click on Item when Inventory Slot is empty

            return;

        }

        PointerEventData pData = (PointerEventData)data; // Convert Input to Mouse Input

        if (pData.button == PointerEventData.InputButton.Right) { // If the input is Right Click

            OnRightMouseBtnClick?.Invoke(this);

        }

        else { // Otherwise, run behavior for left click

            OnItemCLicked?.Invoke(this);


        }
    }


}
