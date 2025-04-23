using UnityEngine;
using UnityEngine.Events;

public class ObjectSnap : MonoBehaviour
{
    [Header("Snap Settings")]
    public Vector2 snapPosition;
    public float snapRadius = 0.5f;
    public LayerMask objectLayer;
    public bool disableAfterSnap = true;
    public string solidObjectsLayer = "SolidObjects";

    [Header("Events")]
    public UnityEvent onObjectSnapped;

    public bool hasSnappedObject = false;
    SwitchPlayerForm playerForm;
    [HideInInspector] public GameObject snappedObject;
    public GameObject objectToSnap;

    void Start()
    {
        playerForm = FindObjectOfType<SwitchPlayerForm>();
    }

    void Update()
    {
        if (hasSnappedObject)
        {
            return;
        }

        Collider2D[] colliders = Physics2D.OverlapCircleAll(snapPosition, snapRadius, objectLayer);
        
        foreach (Collider2D col in colliders)
        {
            if (col.gameObject == objectToSnap && 
                col.GetComponent<PossessedObjectMovement>() != null)
            {
                SnapObject(col.gameObject);
                break;
            }
        }
    }

    public void SnapObject(GameObject obj)
    {
        snappedObject = obj;
        hasSnappedObject = true;

        // change layer to prevent re-possession
        int solidLayer = LayerMask.NameToLayer(solidObjectsLayer);
        if (solidLayer != -1)
        {
            obj.layer = solidLayer;
        }

        // snap position and freeze
        obj.transform.position = snapPosition;
        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }

        // force release object if possessed
        if (playerForm != null && playerForm.isPossessing && playerForm.possessedObject == obj)
        {
            playerForm.ReleaseObject();
        }

        // disable possession component
        PossessedObjectMovement possessable = obj.GetComponent<PossessedObjectMovement>();
        if (possessable != null)
        {
            possessable.enabled = false;
        }

        onObjectSnapped.Invoke();
        if (disableAfterSnap)
        {
            this.enabled = false;
        }
    }

    void OnDrawGizmosSelected() // so i can see it in the editor
    {
        Gizmos.color = hasSnappedObject ? Color.green : Color.blue;
        Gizmos.DrawWireSphere(snapPosition, snapRadius);
    }
}