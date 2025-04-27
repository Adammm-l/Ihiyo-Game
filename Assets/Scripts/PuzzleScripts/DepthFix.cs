using UnityEngine;

public class SimpleDepthSorter : MonoBehaviour
{
    [Header("Settings")]
    public LayerMask possessableLayer;
    public float yOffset = 0.5f; // Adjust based on sprite pivot points
    public int baseNumber = 1;
    
    Transform playerTransform;
    SpriteRenderer playerRenderer;
    Collider2D[] sortedObjects = new Collider2D[50]; // Buffer for objects to sort

    void Awake()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        playerRenderer = playerTransform.GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        int count = Physics2D.OverlapCircleNonAlloc(
            playerTransform.position,
            100f,
            sortedObjects,
            possessableLayer
        );

        var playerCollider = playerTransform.GetComponent<Collider2D>();
        sortedObjects[count] = playerCollider;
        count++;

        System.Array.Sort(sortedObjects, (a, b) =>
        {
            if (a == null && b == null) 
            {
                return 0;
            }
            if (a == null) 
            {
                return 1;
            }
            if (b == null)
            {
                return -1;
            }
            
            float aY = a.transform.position.y - yOffset;
            float bY = b.transform.position.y - yOffset;
            
            if (a.transform == playerTransform) 
            {
                aY -= yOffset;
            }
            if (b.transform == playerTransform)
            {
                bY -= yOffset;
            }
            return bY.CompareTo(aY);
        });

        for (int i = 0; i < count; i++)
        {
            if (sortedObjects[i] != null)
            {
                var renderer = sortedObjects[i].GetComponent<SpriteRenderer>();
                if (renderer != null) 
                {
                    int sortingOrder = i + baseNumber;
                    renderer.sortingOrder = sortingOrder;
            
                    if (renderer.transform.childCount > 0)
                    {
                        Transform child = renderer.transform.GetChild(0);
                        var childRenderer = child.GetComponent<SpriteRenderer>();
                        if (childRenderer != null)
                        {
                            childRenderer.sortingOrder = sortingOrder + 1;
                        }
                    }
                }
            }
        }
    }
}