using UnityEngine;

public class GamestateManagerProxy : MonoBehaviour
{
    public void Subscribe(IGameStateListener listener)
    {
        GameStateManager.Instance?.Subscribe(listener);
    }
    public void Unsubscribe(IGameStateListener listener)
    {
        GameStateManager.Instance?.Unsubscribe(listener);
    }
    public void RequestGameStateSwitchPrevious()
    {
        GameStateManager.Instance?.RequestGameStateSwitchPrevious();
    }
    public void RequestGameStateSwitch(IGameState newState)
    {
        GameStateManager.Instance?.RequestGameStateSwitch(newState);
    }

}
