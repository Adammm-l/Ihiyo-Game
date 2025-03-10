using UnityEngine;

public class PossessedObjectMovement : MonoBehaviour
{
    public float objectMoveSpeed = 3f;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update() // add controls later
    {
        // get input for movement
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        // move object
        Vector2 movement = new Vector2(moveX, moveY) * objectMoveSpeed * Time.deltaTime;
        rb.MovePosition(rb.position + movement);
    }
}

// when i open this again
// add layer to object
// add script to object
// add rigibody to object (kinematic so it doesn't fall out of the world not sure how else to deal with that rn)
