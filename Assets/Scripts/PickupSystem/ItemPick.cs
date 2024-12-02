using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventoryModel;

public class ItemPick : MonoBehaviour
{
    [field: SerializeField]

    public ItemSO InventoryItem { get; private set; }

    [field: SerializeField]

    public int Quantity { get; set; }  = 1;

    [SerializeField] private AudioSource audioSource;

    [SerializeField] private float duration = 9.8f; // Animation Duration

    private void Start() {

        GetComponent<SpriteRenderer>().sprite = InventoryItem.ItemIMG; // Set pickup img to be the inventory image
        StartCoroutine(AnimateItemPickUp()); // Destroy Item after animation ends

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
