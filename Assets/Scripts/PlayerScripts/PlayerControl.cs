using System.Collections;
using UnityEngine;
// Eri (Edwin)
public class PlayerControl : MonoBehaviour
{
    [Header("Attributes")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] public bool canMove = true; //added by Adam

    [Header("References")]
    public GameObject keybindHolder;
    private Rigidbody2D rb;
    private Vector2 moveDir;
    private Animator animator;
    private LayerMask solidObjectsLayer; //Layer for Object Collision
    KeybindManager keybindManager;
    SwitchPlayerForm switchPlayerForm;

    [Header("Keybinds")]
    KeyCode leftKey;
    KeyCode rightKey;
    KeyCode upKey;
    KeyCode downKey;
    
    AudioManager audioManager; // Var needed so that we can incoporate SFX for our player 

  


    private void Awake() {

        audioManager = GameObject.FindGameObjectWithTag("Audio").GetComponent<AudioManager>();

    }

    void Start() {
        rb = GetComponent<Rigidbody2D>(); //Accesses the RigidBody Component that are both attached to the same object
        animator = GetComponent<Animator>(); //Access character animation
        keybindManager = keybindHolder.GetComponent<KeybindManager>();
        switchPlayerForm = GetComponent<SwitchPlayerForm>();
    }
    // Update is called once per frame
    private void Update()
    {
        if (!canMove)
        {
            moveDir = Vector2.zero; //stops all movement
            return;
        }
        // moveDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")); //Reads User Input

        leftKey = keybindManager.GetKeybind("MoveLeft");
        rightKey = keybindManager.GetKeybind("MoveRight");
        upKey = keybindManager.GetKeybind("MoveUp");
        downKey = keybindManager.GetKeybind("MoveDown");


        // modified original input system to function with set keybinds
        float horizontalInput = 0f;
        float verticalInput = 0f;

        // horizontal inputs
        if (Input.GetKey(leftKey))
        {
            horizontalInput -= 1f;
        }
        if (Input.GetKey(rightKey))
        {
            horizontalInput += 1f;
        }

        // vertical inputs
        if (Input.GetKey(downKey)) 
        {
            verticalInput -= 1f;
        }
        if (Input.GetKey(upKey)) 
        {
            verticalInput += 1f;
        }

        // combine inputs into direction vector like before
        moveDir = new Vector2(horizontalInput, verticalInput).normalized;

    }

    void FixedUpdate() {

        bool walker = moveDir.magnitude > 0;
        animator.SetBool("isWalking", true); //Connects the Bool val

        if(!walker) { //Checked if we stopped moving
            animator.SetBool("isWalking", false);
            animator.SetFloat("LastInputX", moveDir.x);
            animator.SetFloat("LastInputX", moveDir.y);
            
        }

        moveSpeed = switchPlayerForm.GetPlayerSpeed();
        rb.velocity = moveDir.normalized * moveSpeed; //Physics Based Movement

        animator.SetFloat("InputX", moveDir.x);  
        animator.SetFloat("InputY", moveDir.y);
    }
}
