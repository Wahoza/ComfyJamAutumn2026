using System;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : StateBasedEnabledBehaviour
{
    [SerializeField] Rigidbody2D rb;
    [SerializeField] float speed;
    Vector3 movement;

    [SerializeField] Animator controller;
    [SerializeField] Transform spriteHolder;
    [SerializeField] float animatorDirectionThreshold = 0.4f;

    private string upParam = "WalkUp";
    private string downParam = "WalkDown";
    private string horizParam = "WalkHorizontal";

    private string currentParam = null;

    protected override void Start()
    {
        base.Start();

        InputManager.InputActions.Player.Move.performed += PlayerMove;
        InputManager.InputActions.Player.Move.canceled += PlayerMove;
    }
    private void PlayerMove(InputAction.CallbackContext context)
    {
        movement=context.ReadValue<Vector2>().normalized; //this should be normalized trough input asset i think?

        if(currentParam != null )
            controller.SetBool(currentParam, false);

        currentParam = movement.y > animatorDirectionThreshold ? upParam : movement.y < -animatorDirectionThreshold ? downParam : Mathf.Abs(movement.x) > 0 ? horizParam : null;

        if (currentParam != null)
            controller.SetBool(currentParam, true);

        if(movement.x != 0)
            transform.localEulerAngles = new Vector3(0, movement.x < 0 ? 0 : 180, 0);
    }
    private void FixedUpdate()
    {
        rb.MovePosition(transform.position + movement * speed * Time.fixedDeltaTime);
    }

    private void OnDestroy()
    {
        base.OnDestroy();

        InputManager.InputActions.Player.Move.performed -= PlayerMove;
        InputManager.InputActions.Player.Move.canceled -= PlayerMove;
    }

}
