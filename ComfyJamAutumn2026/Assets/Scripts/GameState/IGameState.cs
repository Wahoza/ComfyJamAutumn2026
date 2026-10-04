using UnityEngine;

//[CreateAssetMenu(fileName = "IGameState", menuName = "Scriptable Objects/GameStates/IGameState")]

/// <summary>
/// Interface for Game States. All Game States are Scriptable Objects that inherit from this Object.
/// </summary>

public class IGameState : ScriptableObject
{

    /// <summary>
    /// If this is the Current Game State, Handle() is called in the Game State Manager Update().
    /// </summary>
    public virtual void Handle() { }



    /// <summary>
    /// Called when the Game State Manager switches to this Game State. Is called after OnStateDisable() from the previous state.
    /// </summary>
    public virtual void OnStateEnable() {
#if UNITY_EDITOR
        Debug.Log($"Switched to gameState {name}");
#endif
    }


    /// <summary>
    /// Called when the Game State Manager switches away from this Game State. Is called before OnStateEnable() from the new state.
    /// </summary>
    public virtual void OnStateDisable() {
#if UNITY_EDITOR
        Debug.Log($"Disabled {name}");
#endif
    }
}
