using System.Collections;
using UnityEngine;

public class PlayerControl : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] public bool canMove = true; //added by Adam
    private Rigidbody2D rb;
    private Vector2 moveDir;
    private Animator animator;
    private LayerMask solidObjectsLayer; //Layer for Object Collision

    void Start() {
        rb = GetComponent<Rigidbody2D>(); //Accesses the RigidBody Component that are both attached to the same object
        animator = GetComponent<Animator>(); //Access character animation
    }
    // Update is called once per frame
    private void Update()
    {
        if (!canMove)
        {
            moveDir = Vector2.zero; //stops all movement
            return;
        }
        moveDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")); //Reads User Input
    }

    void FixedUpdate() {

        bool walker = moveDir.magnitude > 0;
        animator.SetBool("isWalking", true); //Connects the Bool val

        if(!walker) { //Checked if we stopped moving
            animator.SetBool("isWalking", false);
            animator.SetFloat("LastInputX", moveDir.x);
            animator.SetFloat("LastInputX", moveDir.y);
        }


        rb.velocity = moveDir.normalized * moveSpeed; //Physics Based Movement
        animator.SetFloat("InputX", moveDir.x);  
        animator.SetFloat("InputY", moveDir.y);
    }
}
