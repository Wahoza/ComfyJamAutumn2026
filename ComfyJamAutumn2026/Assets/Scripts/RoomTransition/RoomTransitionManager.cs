using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton that moves the player between rooms: covers the screen, loads the target scene,
/// places the player at the target door and reveals the new room.
/// </summary>
public class RoomTransitionManager : MonoBehaviour
{
    /// <summary>
    /// Put a prefab with this name in a Resources folder to customize the manager and its RoomTransitionScreen.
    /// </summary>
    private const string ResourcesPrefabName = "RoomTransitionManager";

    private static RoomTransitionManager m_instance;

    /// <summary>
    /// Instance of RoomTransitionManager. If none exists, one is created from the Resources prefab, or with default settings.
    /// </summary>
    public static RoomTransitionManager Instance
    {
        get
        {
            if (m_instance == null)
            {
                RoomTransitionManager prefab = Resources.Load<RoomTransitionManager>(ResourcesPrefabName);
                m_instance = prefab != null
                    ? Instantiate(prefab)
                    : new GameObject().AddComponent<RoomTransitionManager>();
                m_instance.name = "RoomTransitionManager";
            }
            return m_instance;
        }
    }

    /// <summary>
    /// Instance of RoomTransitionManager. If none exists, returns null.
    /// </summary>
    public static RoomTransitionManager NullableInstance => m_instance;

    [Tooltip("Game State used while a room is loading. Leave empty to use a state with no input maps or behaviours bound to it, which freezes the player.")]
    [SerializeField] IGameState _transitionGameState;
    [Tooltip("Minimum time the screen stays covered, so it doesn't flicker when a room loads instantly.")]
    [SerializeField] float _minimumLoadingTime = 0.4f;

    private RoomTransitionScreen _screen;
    private IGameState _activeTransitionState;
    private GameStateManager _lockedGameStateManager;

    public bool IsTransitioning { get; private set; }

    #region SingletonHandling
    private void Awake()
    {
        if (m_instance != null && m_instance != this)
        {
            Destroy(gameObject);
            return;
        }
        m_instance = this;

        // DontDestroyOnLoad only works on root objects
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        if (!TryGetComponent(out _screen))
            _screen = gameObject.AddComponent<RoomTransitionScreen>();
    }

    private void OnDestroy()
    {
        if (m_instance == this)
            m_instance = null;
    }
    #endregion

    #region TransitionHandling
    /// <summary>
    /// Leave the current room through a door, arriving at the door it leads to.
    /// </summary>
    public void TransitionTo(RoomDoor fromDoor)
    {
        TransitionTo(fromDoor.TargetScenePath, fromDoor.TargetDoorId, fromDoor.Side);
    }

    /// <summary>
    /// Load a room scene and place the player at one of its doors.
    /// </summary>
    /// <param name="scene">Scene name or path, must be in the build scene list</param>
    /// <param name="targetDoorId">Id of the RoomDoor in the new scene the player arrives at</param>
    /// <param name="exitSide">Side the player is leaving through, decides the wipe direction</param>
    public void TransitionTo(string scene, string targetDoorId, DoorSide exitSide)
    {
        if (IsTransitioning)
            return;

        if (string.IsNullOrEmpty(scene))
        {
            Debug.LogError("[RoomTransition] Door has no target scene assigned.");
            return;
        }

        if (SceneUtility.GetBuildIndexByScenePath(scene) < 0 && !Application.CanStreamedLevelBeLoaded(scene))
        {
            Debug.LogError($"[RoomTransition] Scene '{scene}' is not in the build scene list (File > Build Profiles > Scene List).");
            return;
        }

        StartCoroutine(TransitionRoutine(scene, targetDoorId, exitSide));
    }

    private IEnumerator TransitionRoutine(string scene, string targetDoorId, DoorSide exitSide)
    {
        IsTransitioning = true;
        LockGameState();

        yield return _screen.Cover(exitSide);
        float loadingStartTime = Time.unscaledTime;

        AsyncOperation loading = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Single);
        while (loading != null && !loading.isDone)
            yield return null;

        PlacePlayerAtDoor(scene, targetDoorId);

        // Give the new room a frame to run Start() and let the cameras settle on the player before it is shown
        yield return null;
        while (Time.unscaledTime - loadingStartTime < _minimumLoadingTime)
            yield return null;

        yield return _screen.Reveal(exitSide);

        UnlockGameState();
        IsTransitioning = false;
    }

    private void PlacePlayerAtDoor(string scene, string targetDoorId)
    {
        RoomDoor door = RoomDoor.Find(targetDoorId);
        if (door == null)
        {
            Debug.LogError($"[RoomTransition] No RoomDoor with id '{targetDoorId}' in '{scene}'. Doors found: {RoomDoor.ListDoorIds()}.");
            return;
        }

        PlayerMovement player = FindAnyObjectByType<PlayerMovement>();
        if (player == null)
        {
            Debug.LogError($"[RoomTransition] No Player (PlayerMovement) found in '{scene}'.");
            return;
        }

        try
        {
            door.ReceivePlayer(player);
        }
        catch (Exception exception)
        {
            // Never leave the player stuck behind the loading screen
            Debug.LogException(exception);
        }

        // Cinemachine would otherwise damp from the room's starting camera position over to the player
        for (int i = 0; i < CinemachineCore.VirtualCameraCount; i++)
            CinemachineCore.GetVirtualCamera(i).PreviousStateIsValid = false;
    }
    #endregion

    #region GameStateHandling
    /// <summary>
    /// Switch into the transition Game State, which disables input and player behaviours while the room changes.
    /// </summary>
    private void LockGameState()
    {
        _lockedGameStateManager = GameStateManager.Instance;
        if (_lockedGameStateManager == null)
            return;

        if (_activeTransitionState == null)
        {
            _activeTransitionState = _transitionGameState;
            if (_activeTransitionState == null)
            {
                _activeTransitionState = ScriptableObject.CreateInstance<DefaultGameState>();
                _activeTransitionState.name = "RoomTransitionGameState";
            }
        }

        _lockedGameStateManager.RequestGameStateSwitch(_activeTransitionState);
    }

    private void UnlockGameState()
    {
        // Only return to the previous state if the same manager survived the load and nothing else switched state meanwhile
        if (_lockedGameStateManager != null
            && _lockedGameStateManager == GameStateManager.Instance
            && _lockedGameStateManager.GetGameState() == _activeTransitionState)
        {
            _lockedGameStateManager.RequestGameStateSwitchPrevious();
        }

        _lockedGameStateManager = null;
    }
    #endregion
}
