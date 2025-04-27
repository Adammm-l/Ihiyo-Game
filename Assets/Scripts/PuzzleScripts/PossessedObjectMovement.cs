using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PossessedObjectMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 1f;
    public bool CanBePossessed { get; private set; } = true;

    Rigidbody2D rb;
    SwitchPlayerForm playerForm;
    float distanceCheckTimer = 0f;
    const float DISTANCE_CHECK_INTERVAL = 0.2f;

    public void Initialize(SwitchPlayerForm player)
    {
        if (!CanBePossessed)
        {
            return;
        }

        playerForm = player;
        rb = GetComponent<Rigidbody2D>();
        enabled = true;
        
        rb.gravityScale = 0;
        rb.drag = 2f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation; // prevent tipping over
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        UnfreezePosition();
    }

    void Update()
    {
        // movement input
        Vector2 input = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;
        
        rb.velocity = input * moveSpeed;

        // check distance periodically (better performance than every frame)
        distanceCheckTimer += Time.deltaTime;
        if (distanceCheckTimer > DISTANCE_CHECK_INTERVAL)
        {
            distanceCheckTimer = 0f;
            CheckDistanceToPlayer();
        }
    }

    void CheckDistanceToPlayer()
    {
        if (playerForm == null) return;
        
        float distance = Vector2.Distance(transform.position, playerForm.transform.position);
        if (distance > playerForm.maxPossessionDistance)
        {
            // stop object and release possession
            rb.velocity = Vector2.zero;
            playerForm.ReleaseObject();
        }
    }

    void OnDisable()
    {
        // reset physics after possession ends
        if (rb != null)
        {
            rb.gravityScale = 0;
            rb.velocity = Vector2.zero;
            FreezePosition();
        }
    }

    public void DisablePossession()
    {
        CanBePossessed = false;
        enabled = false;
    }

    void FreezePosition()
    {
        rb.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezePositionY | RigidbodyConstraints2D.FreezeRotation;
    }

    void UnfreezePosition()
    {
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }
}