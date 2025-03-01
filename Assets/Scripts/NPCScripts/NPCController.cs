using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Animates the NPCs and calls the animations based on what direction theyre moving in
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
        Vector3 movement = transform.position - previousPosition;
        float moveX = movement.x;
        float moveY = movement.y;
        bool horizontalDominant = Mathf.Abs(moveX) > Mathf.Abs(moveY);
        float horizontalValue = horizontalDominant ? Mathf.Sign(moveX) : 0;
        float verticalValue = horizontalDominant ? 0 : Mathf.Sign(moveY);
        float speed = movement.magnitude / Time.deltaTime;

        animator.SetFloat("Horizontal", horizontalValue);
        animator.SetFloat("Vertical", verticalValue);
        animator.SetFloat("Speed", speed);

        previousPosition = transform.position;
    }
}