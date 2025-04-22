using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventoryModel;
// Pickup System done by Eri (Edwin)
public class ItemPick : MonoBehaviour
{
    [field: SerializeField]

    public ItemSO InventoryItem { get; private set; }

    [field: SerializeField]

    public int Quantity { get; set; }  = 1;

    [SerializeField] private AudioSource audioSource;

    [SerializeField] private float duration = 9.8f; // Animation Duration

    private void Start()
    {
        Debug.Log($"ItemPick: Started with item {InventoryItem.name}, quantity {Quantity}");

        // Find PickUpSystem and check inventory reference
        PickUpSystem pickupSystem = FindObjectOfType<PickUpSystem>();
        if (pickupSystem != null)
        {
            Debug.Log($"ItemPick: Found PickUpSystem component");

            // Using reflection to check if the inventory reference matches
            var field = pickupSystem.GetType().GetField("inventoryData",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public);

            if (field != null)
            {
                var pickupInventory = field.GetValue(pickupSystem) as InventorySO;
                Debug.Log($"ItemPick: PickUpSystem has inventoryData with ID: {pickupInventory?.GetInstanceID()}");
            }
            else
            {
                Debug.Log("ItemPick: Could not access inventoryData field");
            }
        }
        else
        {
            Debug.Log("ItemPick: No PickUpSystem found in the scene");
        }

        GetComponent<SpriteRenderer>().sprite = InventoryItem.ItemIMG;
        StartCoroutine(AnimateItemPickUp());
    }

    public void DestroyItem() {

        GetComponent<Collider2D>().enabled = false;
        StartCoroutine(AnimateItemPickUp());
    }

    private IEnumerator AnimateItemPickUp() {

        audioSource.Play(); // PLay sfx

        Vector3 startScale = transform.localScale;
        Vector3 endScale = Vector3.zero; // Kill Item
        float curTime = 0;

        while (curTime < duration) {

            curTime += Time.deltaTime; // Start making a countdown for item lifespan
            transform.localScale = Vector3.Lerp(startScale, endScale, curTime / duration);
            yield return null;

        }

        Destroy(gameObject); // Destory the item
    }
}
