using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Trigger on the edge of a room. When the player walks into it, the RoomTransitionManager loads the target scene
/// and places the player at the target door. Also acts as the arrival point for players coming from other rooms.
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class RoomDoor : MonoBehaviour
{
    private static readonly List<RoomDoor> s_activeDoors = new();

    [Header("This Door")]
    [Tooltip("Edge of the room this door is on. Decides the wipe direction and where arriving players are placed.")]
    [SerializeField] DoorSide _side = DoorSide.East;
    [Tooltip("Name of this door inside its scene. Leave empty to use the side name (e.g. \"East\").")]
    [SerializeField] string _doorId;

#if UNITY_EDITOR
    [Header("Destination")]
    [SerializeField] SceneAsset _targetScene;
#endif
    [SerializeField, HideInInspector] string _targetScenePath;
    [Tooltip("Door in the target scene to arrive at. Leave empty to use the opposite side (East door leads to the West door).")]
    [SerializeField] string _targetDoorId;

    [Header("Arrival")]
    [Tooltip("Where players arriving through this door are placed. Leave empty to place them just inside the room, in front of the door.")]
    [SerializeField] Transform _spawnPoint;
    [Tooltip("Distance from the inner edge of the door trigger to the player, when no Spawn Point is set.")]
    [SerializeField] float _spawnDistance = 1f;

    BoxCollider2D _trigger;
    bool _armed = true;

    public DoorSide Side => _side;
    public string DoorId => string.IsNullOrWhiteSpace(_doorId) ? _side.ToString() : _doorId.Trim();
    public string TargetDoorId => string.IsNullOrWhiteSpace(_targetDoorId) ? _side.Opposite().ToString() : _targetDoorId.Trim();
    public string TargetScenePath => _targetScenePath;

    BoxCollider2D Trigger
    {
        get
        {
            if (_trigger == null)
                _trigger = GetComponent<BoxCollider2D>();
            return _trigger;
        }
    }

    /// <summary>
    /// Position arriving players are placed at.
    /// </summary>
    public Vector2 SpawnPosition
    {
        get
        {
            if (_spawnPoint != null)
                return _spawnPoint.position;

            // Step from the door's center past its inner edge, so the player lands inside the room and outside the trigger
            Vector2 center = transform.TransformPoint(Trigger.offset);
            Vector2 halfSize = Vector2.Scale(Trigger.size, transform.lossyScale) * 0.5f;
            Vector2 outward = _side.ToVector();
            float halfDepth = Mathf.Abs(outward.x) > 0f ? Mathf.Abs(halfSize.x) : Mathf.Abs(halfSize.y);

            return center - outward * (halfDepth + _spawnDistance);
        }
    }

    #region DoorRegistry
    private void OnEnable()
    {
        s_activeDoors.Add(this);
    }

    private void OnDisable()
    {
        s_activeDoors.Remove(this);
    }

    /// <summary>
    /// Finds an active door with the given id in the loaded rooms. Returns null if there is none.
    /// </summary>
    public static RoomDoor Find(string doorId)
    {
        foreach (RoomDoor door in s_activeDoors)
        {
            if (door != null && door.DoorId == doorId)
                return door;
        }
        return null;
    }

    /// <summary>
    /// Comma separated ids of all active doors, for error messages.
    /// </summary>
    public static string ListDoorIds()
    {
        List<string> ids = new();
        foreach (RoomDoor door in s_activeDoors)
        {
            if (door != null)
                ids.Add(door.DoorId);
        }
        return ids.Count > 0 ? string.Join(", ", ids) : "none";
    }
    #endregion

    #region PlayerHandling
    private void Awake()
    {
        Trigger.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_armed || !IsPlayerBody(other))
            return;

        RoomTransitionManager.Instance.TransitionTo(this);
    }

    /// <summary>
    /// Places a player that arrived through this door at the spawn position.
    /// </summary>
    public void ReceivePlayer(PlayerMovement player)
    {
        Vector2 position = SpawnPosition;
        Transform playerTransform = player.transform;
        playerTransform.position = new Vector3(position.x, position.y, playerTransform.position.z);

        if (player.TryGetComponent(out Rigidbody2D body))
        {
            body.position = position;
            body.linearVelocity = Vector2.zero;
        }

        StopAllCoroutines();
        StartCoroutine(DisarmUntilPlayerLeaves(player));
    }

    /// <summary>
    /// Keeps the door from sending the player straight back if they were placed inside its trigger.
    /// </summary>
    private IEnumerator DisarmUntilPlayerLeaves(PlayerMovement player)
    {
        _armed = false;

        List<Collider2D> playerBodies = new();
        foreach (Collider2D collider in player.GetComponentsInChildren<Collider2D>())
        {
            if (!collider.isTrigger)
                playerBodies.Add(collider);
        }

        // Wait for a physics step so contacts reflect the new position
        yield return new WaitForFixedUpdate();

        while (player != null && IsTouchingAny(playerBodies))
            yield return new WaitForFixedUpdate();

        _armed = true;
    }

    private bool IsTouchingAny(List<Collider2D> colliders)
    {
        foreach (Collider2D collider in colliders)
        {
            if (collider != null && Trigger.IsTouching(collider))
                return true;
        }
        return false;
    }

    /// <summary>
    /// Only the player's solid collider counts, so trigger colliders like an interaction range don't open doors early.
    /// </summary>
    private static bool IsPlayerBody(Collider2D other)
    {
        if (other.isTrigger)
            return false;

        return other.attachedRigidbody != null
            ? other.attachedRigidbody.TryGetComponent(out PlayerMovement _)
            : other.TryGetComponent(out PlayerMovement _);
    }
    #endregion

#if UNITY_EDITOR
    #region EditorHandling
    private void Reset()
    {
        BoxCollider2D box = GetComponent<BoxCollider2D>();
        box.isTrigger = true;
        box.size = new Vector2(1f, 3f);
    }

    private void OnValidate()
    {
        _targetScenePath = _targetScene != null ? AssetDatabase.GetAssetPath(_targetScene) : string.Empty;
        EnsureSceneInBuild(_targetScenePath);
    }

    /// <summary>
    /// Scenes can only be loaded at runtime if they are in the build scene list, so target scenes are added automatically.
    /// </summary>
    private static void EnsureSceneInBuild(string scenePath)
    {
        if (string.IsNullOrEmpty(scenePath) || Application.isPlaying || BuildPipeline.isBuildingPlayer)
            return;

        EditorApplication.delayCall += () =>
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            foreach (EditorBuildSettingsScene scene in scenes)
            {
                if (scene.path == scenePath)
                {
                    if (!scene.enabled)
                        Debug.LogWarning($"[RoomDoor] '{scenePath}' is disabled in the build scene list, doors leading to it won't work.");
                    return;
                }
            }

            ArrayUtility.Add(ref scenes, new EditorBuildSettingsScene(scenePath, true));
            EditorBuildSettings.scenes = scenes;
            Debug.Log($"[RoomDoor] Added '{scenePath}' to the build scene list so doors can load it.");
        };
    }
    #endregion
#endif
}
