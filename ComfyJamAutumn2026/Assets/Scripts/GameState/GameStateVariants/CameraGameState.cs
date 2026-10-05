using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "CameraGameState", menuName = "Scriptable Objects/GameStates/CameraGameState")]
public class CameraGameState : IGameState
{
    public override void OnStateEnable()
    {
        base.OnStateEnable();

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }

    public override void OnStateDisable()
    {
        base.OnStateDisable();

        //Cursor.lockState = CursorLockMode.Locked;
    }

    public override void Handle()
    {
        base.Handle();
    }
}
