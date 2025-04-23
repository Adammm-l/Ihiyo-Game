using System.Collections;
using UnityEngine;
using InventoryModel;

public class ItemPick : MonoBehaviour
{
    [field: SerializeField]
    public ItemSO InventoryItem { get; private set; }

    [field: SerializeField]
    public int Quantity { get; set; } = 1;

    private void Start()
    {
        GetComponent<SpriteRenderer>().sprite = InventoryItem.ItemIMG;
        // Removed animation call
    }

    public void DestroyItem()
    {
        GetComponent<Collider2D>().enabled = false;
        Destroy(gameObject); // Immediately destroy without animation
    }

    // Removed AnimateItemPickUp coroutine completely
}