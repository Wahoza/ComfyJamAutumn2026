using UnityEngine;

/// <summary>
/// Game State Change Listener interface that can Subscribe to and Unsubscribe from the GameStateManager, and is notified of Game State changes.
/// </summary>
public interface IGameStateListener
{
    void Subscribe() {

        if(GameStateManager.Instance == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning("Could not find Game State Manager");
#endif  
            return;
        }
        GameStateManager.Instance.Subscribe(this);
    }


    void Unsubscribe()
    {
        if (GameStateManager.Instance != null)
            GameStateManager.Instance.Unsubscribe(this);
    }


    /// <summary>
    /// Function that is called by GameStateManager when the Game State is changed.
    /// </summary>
    void NotifyStateChange(IGameState newState, IGameState oldState);
}
