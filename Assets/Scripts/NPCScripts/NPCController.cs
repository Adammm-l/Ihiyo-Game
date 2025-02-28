using UnityEngine;

public class NPCAnimationController : MonoBehaviour
{
    private Animator animator;
    private Vector3 previousPosition;
    private NPCMovement npcMovement;

    void Start()
    {
        animator = GetComponent<Animator>();
        previousPosition = transform.position;
        npcMovement = GetComponent<NPCMovement>();
    }

    void Update()
    {
        // Calculate movement direction based on position change
        Vector3 movement = transform.position - previousPosition;
        float moveX = movement.x;
        float moveY = movement.y;

        // Determine dominant direction
        bool horizontalDominant = Mathf.Abs(moveX) > Mathf.Abs(moveY);

        // Set normalized direction based on dominant axis
        float horizontalValue = horizontalDominant ? Mathf.Sign(moveX) : 0;
        float verticalValue = horizontalDominant ? 0 : Mathf.Sign(moveY);

        // Calculate speed
        float speed = movement.magnitude / Time.deltaTime;

        // Update animator parameters
        animator.SetFloat("Horizontal", horizontalValue);
        animator.SetFloat("Vertical", verticalValue);
        animator.SetFloat("Speed", speed);

        // Store current position for next frame
        previousPosition = transform.position;
    }
}