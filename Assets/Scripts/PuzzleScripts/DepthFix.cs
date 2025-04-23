using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class DepthSorting : MonoBehaviour
{
    private Renderer objectRenderer;
    private Transform playerTransform;
    private int defaultSortingOrder;
    
    [Header("Settings")]
    public float yOffset = 0.5f; // Adjust based on your sprite pivot points
    public int behindSortOrder = 0;
    public int frontSortOrder = 1;

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        defaultSortingOrder = objectRenderer.sortingOrder;
    }

    void Update()
    {
        // Compare positions with y-offset to determine if player is in front or behind
        float playerYWithOffset = playerTransform.position.y - yOffset;
        float objectY = transform.position.y;

        if (playerYWithOffset > objectY)
        {
            // Player is behind the object
            objectRenderer.sortingOrder = frontSortOrder;
        }
        else
        {
            // Player is in front of the object
            objectRenderer.sortingOrder = behindSortOrder;
        }
    }
}