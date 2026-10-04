using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Determines druing which Game States the Game Object itself is Active.
/// </summary>
public class StateBasedActiveComponent : MonoBehaviour, IGameStateListener
{
    [SerializeField] private List<IGameState> _enabledGameStates;

    public void NotifyStateChange(IGameState newState, IGameState oldState)
    {
        bool isActive = _enabledGameStates.Contains(newState);

        gameObject.SetActive(isActive);
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
