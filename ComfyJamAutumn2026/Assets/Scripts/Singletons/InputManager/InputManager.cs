using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour, IGameStateListener
{
    /// <summary>
    /// Manager of System Input actions
    /// </summary>
    public static InputManager Instance { get; private set; }

    /// <summary>
    /// Managed System Input actions, managed by Instance.
    /// </summary>
    public static InputSystem_Actions InputActions
    {
        get
        {
            return Instance?.m_InputActions;
        }
    }

    private InputSystem_Actions m_InputActions;
    private List<InputActionMap> _currentMaps = new();
    private Coroutine _switchMapsAfterTickRoutine;

    private Dictionary<IGameState, List<string>> _gamestateMaps = new();

    [SerializeField] private List<IGameState> _gameStates = new();
    [SerializeField] private List<string> _boundMaps = new();
    [SerializeField] private List<int> _allocatedMapSize = new();
    [SerializeField] private List<bool> _isFoldedOutList = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Initialize and Deserialize the gamestate bound maps
    /// </summary>
    void Initialize()
    {
        m_InputActions = new InputSystem_Actions();

        int ctr = 0;
        for (int stateIterator = 0; stateIterator < _gameStates.Count; stateIterator++) {


            if (_gameStates[stateIterator] && !_gamestateMaps.ContainsKey(_gameStates[stateIterator]))
            {
                string debugString = "";

                _gamestateMaps.Add(_gameStates[stateIterator], new List<string>());

                for (int allocatedIterator = 0; allocatedIterator < _allocatedMapSize[stateIterator]; allocatedIterator++)
                {
                    _gamestateMaps[_gameStates[stateIterator]].Add(_boundMaps[ctr + allocatedIterator]);
                    debugString += $" {_boundMaps[ctr + allocatedIterator]}";
                }

            }


            ctr += _allocatedMapSize[stateIterator];
        }
    }

    /// <summary>
    /// Bind self to gamestate manager
    /// </summary>
    private void Start()
    {
        GameStateManager.Instance?.Subscribe(this);

        NotifyStateChange(GameStateManager.Instance?.GetGameState(), null);
    }

    #region Switch Enabled Action Maps

    /// <summary>
    /// Switch to action maps. Final execution is buffered to next Update, to avoid issues with multiple gamestate changes per tick.
    /// </summary>
    /// <param name="names">Names Of the Maps</param>
    public void SwitchToActionMapFromName(List<string> names)
    {
        List<InputActionMap> maps = new List<InputActionMap>();
        foreach (string name in names)
        {
            var map = m_InputActions.asset.FindActionMap(name);

            if (map != null)
                maps.Add(map);
        }

        if (_currentMaps != null)
        {
            foreach (var map in _currentMaps)
            {
                if (map != null)
                {
                    map.Disable();
                }
            }
        }

        _currentMaps.Clear();

        if (_switchMapsAfterTickRoutine != null)
        {
            StopCoroutine(_switchMapsAfterTickRoutine);
        }
        _switchMapsAfterTickRoutine = StartCoroutine(SwitchMapAfterGameTick(maps));
    }

    private IEnumerator SwitchMapAfterGameTick(List<InputActionMap> maps)
    {
        yield return null;

        if (_currentMaps != null)
        {
            foreach (var map in _currentMaps)
            {
                if (map != null)
                {
                    map.Disable();
                }
            }
        }

        foreach (var map in maps)
        {
            if (map != null)
            {
                map.Enable();
            }
            else
            {
                Debug.LogWarning("Could not find requested Map");
            }
        }

        _currentMaps = maps;
        _switchMapsAfterTickRoutine = null;
    }

    public void NotifyStateChange(IGameState newState, IGameState oldState)
    {
        if (_gamestateMaps.ContainsKey(newState))
        {
            SwitchToActionMapFromName(_gamestateMaps[newState]);
        }
        else 
        {
            SwitchToActionMapFromName(new List<string>());
        }
    }
    #endregion
}
