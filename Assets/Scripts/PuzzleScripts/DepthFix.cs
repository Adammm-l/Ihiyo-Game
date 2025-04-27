using UnityEngine;

public class SimpleDepthSorter : MonoBehaviour
{
    [Header("Settings")]
    public LayerMask possessableLayer;
    public LayerMask npcLayer;
    
    public float possessableOffset = 0.5f;
    public float npcOffset = 0.2f;
    public int baseNumber = 1;
    
    Transform playerTransform;
    SpriteRenderer playerRenderer;
    Collider2D[] sortedObjects = new Collider2D[50];
    Collider2D[] possessableObjects = new Collider2D[50];
    Collider2D[] npcObjects = new Collider2D[50];

    void Awake()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        playerRenderer = playerTransform.GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        int possessableCount = 0;
        int npcCount = 0;

        // collect possessable objects
        possessableCount = Physics2D.OverlapCircleNonAlloc(
            playerTransform.position,
            100f,
            possessableObjects,
            possessableLayer
        );

        // collect NPC objects
        npcCount = Physics2D.OverlapCircleNonAlloc(
            playerTransform.position,
            100f,
            npcObjects,
            npcLayer
        );

        int validNpcCount = 0;
        for (int i = 0; i < npcCount; i++)
        {
            if (npcObjects[i] != null)
            {
                NPCAnimationController npcController = npcObjects[i].GetComponent<NPCAnimationController>();
                if (npcController == null)
                {
                    continue; // skip NPC objects without controller
                }
            }
            npcObjects[validNpcCount] = npcObjects[i];
            validNpcCount++;
        }

        var playerCollider = playerTransform.GetComponent<Collider2D>();
        sortedObjects[validNpcCount + possessableCount] = playerCollider;
        int count = possessableCount + validNpcCount + 1;
        
        System.Array.Copy(possessableObjects, sortedObjects, possessableCount);
        System.Array.Copy(npcObjects, 0, sortedObjects, possessableCount, validNpcCount);

        // sorting objects by y-position
        System.Array.Sort(sortedObjects, (a, b) =>
        {
            if (a == null && b == null) return 0;
            if (a == null) return 1;
            if (b == null) return -1;

            float aY = a.transform.position.y;
            float bY = b.transform.position.y;

            if (((1 << a.gameObject.layer) & possessableLayer) != 0)
            {
                aY += possessableOffset; // apply offset for possessables
            }
            else if (((1 << a.gameObject.layer) & npcLayer) != 0)
            {
                aY -= npcOffset; // apply offset for NPCs
            }

            if (((1 << b.gameObject.layer) & possessableLayer) != 0)
            {
                bY += possessableOffset; // apply offset for possessables
            }
            else if (((1 << b.gameObject.layer) & npcLayer) != 0)
            {
                bY -= npcOffset; // apply offset for NPCs
            }

            return bY.CompareTo(aY);
        });

        // set sorting order
        for (int i = 0; i < count; i++)
        {
            if (sortedObjects[i] != null)
            {
                var renderer = sortedObjects[i].GetComponent<SpriteRenderer>();
                if (renderer != null) 
                {
                    int sortingOrder = i + baseNumber;
                    renderer.sortingOrder = sortingOrder;
            
                    if (renderer.transform.childCount > 0) // sorting order for objects with visible children
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
