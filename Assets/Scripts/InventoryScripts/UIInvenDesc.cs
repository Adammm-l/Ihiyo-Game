using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace UIInventoryPage {
public class UIInvenDesc : MonoBehaviour
{

    [SerializeField] private Image itemIMG;

    [SerializeField] private TMP_Text title;

    [SerializeField] private TMP_Text description;

    public void Awake() {

        ResetDescription(); // Clear out Description upon launch

    }

    public void ResetDescription() { // Reset Item Details

        this.itemIMG.gameObject.SetActive(false);
        this.title.text = "";
        this.description.text = "";

    }

    public void SetDescription(Sprite sprite, string itemName, string itemDesc) { // Construct Item Details

        this.itemIMG.gameObject.SetActive(true);
        this.itemIMG.sprite = sprite;
        this.title.text = itemName;
        this.description.text = itemDesc;

    }
}
}