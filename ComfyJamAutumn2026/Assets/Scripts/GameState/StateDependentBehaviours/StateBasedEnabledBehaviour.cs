using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Monobehaviour, that is enabled or disabled based on Game State.
/// </summary>
public class StateBasedEnabledBehaviour : MonoBehaviour, IGameStateListener
{
    [SerializeField] private List<IGameState> _enabledGameStates;

    public void NotifyStateChange(IGameState newState, IGameState oldState)
    {
        enabled = _enabledGameStates.Contains(newState);
    }



    protected virtual void Start()
    {
        (this as IGameStateListener).Subscribe();
    }

    void OnDestroy()
    {
        (this as IGameStateListener).Unsubscribe();
    }
}
