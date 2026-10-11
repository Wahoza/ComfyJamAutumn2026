using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : StateBasedEnabledBehaviour
{
    [SerializeField] Rigidbody2D rb;
    [SerializeField] float speed;
    Vector3 movement;

    protected override void Start()
    {
        base.Start();
        InputManager.InputActions.Player.Move.performed += PlayerMove;
        InputManager.InputActions.Player.Move.canceled += PlayerMove;
    }
    private void PlayerMove(InputAction.CallbackContext context)
    {
        movement=context.ReadValue<Vector2>(); //this should be normalized trough input asset i think?
    }
    private void FixedUpdate()
    {
        rb.MovePosition(transform.position + movement * speed * Time.fixedDeltaTime);
    }


}
