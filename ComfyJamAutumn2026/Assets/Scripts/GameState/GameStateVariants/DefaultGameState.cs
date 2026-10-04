using UnityEngine;

[CreateAssetMenu(fileName = "DefaultGameState", menuName = "Scriptable Objects/GameStates/DefaultGameState")]
public class DefaultGameState : IGameState
{
    public override void OnStateEnable()
    {
        base.OnStateEnable();
    }

    public override void OnStateDisable()
    {
        base.OnStateDisable();
    }

    public override void Handle()
    {
        base.Handle();
    }
}
