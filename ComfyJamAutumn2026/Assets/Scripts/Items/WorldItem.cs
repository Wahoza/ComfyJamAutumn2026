using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// An item lying in a room that the player can pick up. Place the WorldItem prefab in a room and assign its Item.
/// The Inventory also spawns one wherever the player drops an item.
/// </summary>
public class WorldItem : MonoBehaviour
{
    private static readonly List<WorldItem> s_activeItems = new();

    [SerializeField] ItemData _item;
    [Tooltip("Shows the item's World Sprite.")]
    [SerializeField] SpriteRenderer _renderer;

    /// <summary>
    /// Items lying in the loaded rooms.
    /// </summary>
    public static IReadOnlyList<WorldItem> ActiveItems => s_activeItems;

    public ItemData Item => _item;

    /// <summary>
    /// Tells this item apart from the others across room visits, so items the player took stay gone.
    /// </summary>
    public string Id { get; private set; }

    /// <summary>
    /// Point just below the item, where the pickup prompt hangs.
    /// </summary>
    public Vector3 PromptAnchor
    {
        get
        {
            if (_renderer == null || _renderer.sprite == null)
                return transform.position;

            Bounds bounds = _renderer.bounds;
            return new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
        }
    }

    #region ItemHandling
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetRegistry()
    {
        // Domain reload is turned off, so the list would otherwise keep items from the last play session
        s_activeItems.Clear();
    }

    private void Awake()
    {
        // A placed item sits at the same spot in the hierarchy every time its room loads
        Id = GetHierarchyPath();
        ApplySprite();
    }

    private void Start()
    {
        // The player took this item on an earlier visit
        if (Inventory.Instance != null && Inventory.Instance.WasTaken(Id))
        {
            // Disabling takes it out of reach right away, Destroy only happens at the end of the frame
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        s_activeItems.Add(this);
    }

    private void OnDisable()
    {
        s_activeItems.Remove(this);
    }

    /// <summary>
    /// Sets up an item spawned at runtime, like one the player dropped.
    /// </summary>
    public void Initialize(ItemData item, string id)
    {
        _item = item;
        Id = id;
        ApplySprite();
    }

    private void ApplySprite()
    {
        if (_renderer != null && _item != null && _item.WorldSprite != null)
            _renderer.sprite = _item.WorldSprite;
    }

    private string GetHierarchyPath()
    {
        StringBuilder path = new();
        for (Transform current = transform; current != null; current = current.parent)
            path.Insert(0, $"/{current.name}#{current.GetSiblingIndex()}");

        return gameObject.scene.path + path;
    }
    #endregion

#if UNITY_EDITOR
    #region EditorHandling
    private void Reset()
    {
        _renderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void OnValidate()
    {
        // Shows the item's sprite in the scene as soon as it is assigned. Changing a sprite inside OnValidate makes Unity warn, so it waits a tick
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this != null)
                ApplySprite();
        };
    }
    #endregion
#endif
}
