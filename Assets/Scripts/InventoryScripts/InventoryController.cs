using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    [SerializeField] private UIInventory inventoryUI;
    public int invenSize = 10;

    public void Start() {
        inventoryUI.InitializeInvenUI(invenSize); // Test Inventory Size and Initialize it
    }

    public void Update() {

        if (Input.GetKeyDown(KeyCode.I)) { // Open and Close the Inventory Page

            if (inventoryUI.isActiveAndEnabled == false) {
                inventoryUI.Show();
            }

            else {
                inventoryUI.Hide();
            }
        }
    }
}
