using UnityEngine;

[CreateAssetMenu(fileName = "JournalGameState", menuName = "Scriptable Objects/GameStates/JournalGameState")]

public class JournalGameState : IGameState
{
    public override void OnStateEnable()
    {
        base.OnStateEnable();

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
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
