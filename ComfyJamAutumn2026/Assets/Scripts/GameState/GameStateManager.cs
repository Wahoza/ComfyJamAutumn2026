using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;


/// <summary>
/// Singleton that manages the Game State, the switching between different Game States, and the Notification of IGameStateListener objects
/// </summary>
public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance {get; private set;}
    [SerializeField] IGameState initialGameState;

    private Stack<IGameState> _previousStates;
    private IGameState _currentState;

    private List<IGameStateListener> _listeners;

    #region SignletonHandling
    private void Awake()
    {
        Initailize();
    }

    private void Initailize()
    {
        if (Instance == null)
        {
            Instance = this;
            _previousStates = new();

            DontDestroyOnLoad(Instance);

            RequestGameStateSwitch(initialGameState);
        }
    }
    private void OnApplicationQuit()
    {
        if(Instance != null)
            Destroy(Instance);
    }
    #endregion

    #region GameStateHandling
    public IGameState GetGameState()
    {
        return _currentState;
    }

    
    /// <summary>
    /// Request a switch into a new Game State from the Game State Manager.
    /// </summary>
    /// <param name="newState">Gamestate that is to be switched to</param>
    public void RequestGameStateSwitch(IGameState newState)
    {

        if (_previousStates == null)
            _previousStates = new();

        _previousStates.Push(_currentState);
        
        var prevState = _currentState;

        _currentState = newState;

        prevState?.OnStateDisable();
        _currentState?.OnStateEnable();

        Notify(newState, prevState);
    }



    /// <summary>
    /// Request to enter previous Game State, current Game State is discarded. If there was no previous Game State, switches to null.
    /// </summary>
    public void RequestGameStateSwitchPrevious()
    {
        if(_previousStates != null)
        {
            var prevState = _currentState;

            if(_previousStates.Count > 0)
                _currentState = _previousStates.Pop();
            else
                _currentState = null;

            prevState?.OnStateDisable();
            _currentState?.OnStateEnable();

            Notify(_currentState, prevState);
        }
    }

    private void Update()
    {
        _currentState?.Handle();
    }

    #endregion




    #region ListenerHandling
    public void Subscribe(IGameStateListener listener)
    {
        if (_listeners == null)
            _listeners = new List<IGameStateListener>();

        if(!_listeners.Contains(listener))
            _listeners.Add(listener);
    }

    public void Unsubscribe(IGameStateListener listener) 
    {
        if(_listeners != null && _listeners.Contains(listener))
            _listeners.Remove(listener);
    }


    /// <summary>
    /// Notifies all subscribed Listeners of the new Game State.
    /// </summary>

    private void Notify(IGameState newState, IGameState prevState)
    {
        if(_listeners == null)
            return;

        foreach(IGameStateListener listener in _listeners)
        {
            listener.NotifyStateChange(newState, prevState);
        }
    }
    #endregion
}
