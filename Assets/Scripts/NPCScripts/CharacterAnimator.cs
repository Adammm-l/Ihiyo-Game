/*
using UnityEngine;

public class CharacterAnimator : MonoBehaviour
{
    public Animator animator;

    void Start()
    {
        // Get the Animator component if not assigned
        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Get input (replace with your own input system if needed)
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        // Set animator parameters
        animator.SetFloat("Horizontal", horizontal);
        animator.SetFloat("Vertical", vertical);
        animator.SetFloat("Speed", Mathf.Abs(horizontal) + Mathf.Abs(vertical));

*/