using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : StateBasedEnabledBehaviour
{
    private IInteractable currentTarget;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out IInteractable i))
            currentTarget = i;
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent(out IInteractable i) && i == currentTarget)
            currentTarget = null;
    }
    private void Update()
    {
        if (currentTarget != null && InputManager.InputActions.Player.Interact.WasPressedThisFrame()) {
            currentTarget.Interact();
        }
    }

}
