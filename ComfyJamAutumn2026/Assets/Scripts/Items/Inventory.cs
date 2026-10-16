using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton that holds the items the player carries, one item per slot. Created at startup from the Inventory prefab in Resources
/// and kept between rooms. Interact (E) picks up the closest item in reach, DropItem (Q) drops the selected item at the player's feet,
/// SelectItem (1-3) and CycleItem (mouse wheel) choose the selected slot.
/// Rooms remember which items were taken from them, which were dropped in them, and which ItemReactions already happened.
/// </summary>
public class Inventory : MonoBehaviour
{
    /// <summary>
    /// Name of the prefab in a Resources folder that holds the inventory setup. Without it, there is no inventory.
    /// </summary>
    private const string ResourcesPrefabName = "Inventory";

    private static Inventory m_instance;

    /// <summary>
    /// Instance of Inventory. Null if there is no Inventory prefab in Resources.
    /// </summary>
    public static Inventory Instance => m_instance;

    /// <summary>
    /// An item lying where the player dropped it, so its room can put it back when it loads again.
    /// </summary>
    private class DroppedItem
    {
        public string Id;
        public ItemData Item;
        public string ScenePath;
        public Vector3 Position;
    }

    [Header("Slots")]
    [SerializeField, Min(1)] int _slotCount = 3;

    [Header("Picking Up")]
    [Tooltip("How close the player has to be to an item to pick it up.")]
    [SerializeField] float _pickupRange = 1.2f;

    [Header("Dropping")]
    [Tooltip("Spawned where the player drops an item.")]
    [SerializeField] WorldItem _worldItemPrefab;
    [Tooltip("Where dropped items land, relative to the player.")]
    [SerializeField] Vector2 _dropOffset = new(0f, -0.5f);

    private ItemData[] _slots;
    private int _selectedSlot;
    private Transform _player;
    private PlayerMovement _playerMovement;
    private int _dropCount;
    private readonly List<DroppedItem> _droppedItems = new();
    private readonly HashSet<string> _takenItemIds = new();
    private readonly HashSet<string> _doneReactionIds = new();

    /// <summary>
    /// Called when an item is added or removed, or another slot is selected.
    /// </summary>
    public event Action Changed;

    public int SlotCount => _slots.Length;
    public int SelectedSlot => _selectedSlot;

    /// <summary>
    /// True while there is a player in the loaded rooms.
    /// </summary>
    public bool HasPlayer => _player != null;

    /// <summary>
    /// The player in the loaded rooms, or null if there is none.
    /// </summary>
    public Transform Player => _player;

    /// <summary>
    /// Item the player would pick up with Interact right now, or null if none is in reach.
    /// </summary>
    public WorldItem Target { get; private set; }

    /// <summary>
    /// True if every slot holds an item.
    /// </summary>
    public bool IsFull => Array.IndexOf(_slots, null) < 0;

    #region SingletonHandling
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateOnStartup()
    {
        if (m_instance != null)
            return;

        Inventory prefab = Resources.Load<Inventory>(ResourcesPrefabName);
        if (prefab == null)
        {
            Debug.LogWarning($"[Inventory] No '{ResourcesPrefabName}' prefab in a Resources folder, so the player can't carry items.");
            return;
        }
        Instantiate(prefab).name = ResourcesPrefabName;
    }

    private void Awake()
    {
        if (m_instance != null && m_instance != this)
        {
            // Destroy only happens at the end of the frame, keep Update from running until then
            enabled = false;
            Destroy(gameObject);
            return;
        }
        m_instance = this;

        // DontDestroyOnLoad only works on root objects
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        _slots = new ItemData[_slotCount];
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (m_instance != this)
            return;

        SceneManager.sceneLoaded -= OnSceneLoaded;
        m_instance = null;
    }
    #endregion

    #region SlotHandling
    public ItemData GetItem(int slot)
    {
        return _slots[slot];
    }

    public bool Contains(ItemData item)
    {
        return item != null && Array.IndexOf(_slots, item) >= 0;
    }

    /// <summary>
    /// Puts an item in the first free slot and selects it. False if every slot is taken.
    /// </summary>
    public bool Add(ItemData item)
    {
        int slot = Array.IndexOf(_slots, null);
        if (item == null || slot < 0)
            return false;

        _slots[slot] = item;
        _selectedSlot = slot;
        Changed?.Invoke();
        return true;
    }

    /// <summary>
    /// Takes an item out of the inventory without dropping it, like when it is handed to someone. False if the player doesn't carry it.
    /// </summary>
    public bool Remove(ItemData item)
    {
        int slot = item != null ? Array.IndexOf(_slots, item) : -1;
        if (slot < 0)
            return false;

        _slots[slot] = null;
        Changed?.Invoke();
        return true;
    }

    public void Select(int slot)
    {
        if (slot < 0 || slot >= _slots.Length || slot == _selectedSlot)
            return;

        _selectedSlot = slot;
        Changed?.Invoke();
    }
    #endregion

    #region PlayerInput
    private void Update()
    {
        InputSystem_Actions actions = InputManager.InputActions;

        // Only while the player can walk around, so nothing is picked up during dialogue, the journal or room transitions.
        // The Player map stays on during dialogue so Interact can advance it, movement being on is what tells walking around apart
        if (!FindPlayer() || !_playerMovement.enabled || actions == null || !actions.Player.enabled)
        {
            Target = null;
            return;
        }

        InputSystem_Actions.PlayerActions input = actions.Player;
        WorldItem closest = FindClosestItem();

        if (closest != null && input.Interact.WasPressedThisFrame())
            PickUp(closest);

        if (input.DropItem.WasPressedThisFrame())
            DropSelected();

        if (input.SelectItem.WasPressedThisFrame())
            SelectFromBinding(input.SelectItem);

        if (input.CycleItem.WasPerformedThisFrame())
            CycleSelection(input.CycleItem.ReadValue<float>());

        // Picking up or dropping changes what is in reach
        Target = FindClosestItem();
    }

    private bool FindPlayer()
    {
        if (_player == null)
        {
            _playerMovement = FindAnyObjectByType<PlayerMovement>();
            _player = _playerMovement != null ? _playerMovement.transform : null;
        }
        return _player != null;
    }

    private WorldItem FindClosestItem()
    {
        WorldItem closest = null;
        float closestDistance = _pickupRange * _pickupRange;
        Vector2 playerPosition = _player.position;

        foreach (WorldItem item in WorldItem.ActiveItems)
        {
            if (item.Item == null)
                continue;

            float distance = ((Vector2)item.transform.position - playerPosition).sqrMagnitude;
            if (distance <= closestDistance)
            {
                closest = item;
                closestDistance = distance;
            }
        }
        return closest;
    }

    /// <summary>
    /// The first binding of SelectItem selects the first slot, the second binding the second slot, and so on.
    /// </summary>
    private void SelectFromBinding(InputAction action)
    {
        // The pressed key is only known while it is held, so a tap shorter than a frame is missed
        if (action.activeControl != null)
            Select(action.GetBindingIndexForControl(action.activeControl));
    }

    private void CycleSelection(float direction)
    {
        if (direction == 0f)
            return;

        int step = direction > 0f ? 1 : -1;
        Select((_selectedSlot + step + _slots.Length) % _slots.Length);
    }
    #endregion

    #region WorldItemHandling
    /// <summary>
    /// Moves an item from the world into the inventory. False if every slot is taken.
    /// </summary>
    public bool PickUp(WorldItem worldItem)
    {
        if (worldItem == null || !Add(worldItem.Item))
            return false;

        // A dropped item is no longer put back into its room, a placed item stays gone when its room loads again
        if (_droppedItems.RemoveAll(dropped => dropped.Id == worldItem.Id) == 0)
            _takenItemIds.Add(worldItem.Id);

        // Disabling takes it out of reach right away, Destroy only happens at the end of the frame
        worldItem.gameObject.SetActive(false);
        Destroy(worldItem.gameObject);
        return true;
    }

    /// <summary>
    /// Drops the item in the selected slot at the player's feet. False if the slot is empty.
    /// </summary>
    public bool DropSelected()
    {
        ItemData item = _slots[_selectedSlot];
        if (item == null || !FindPlayer())
            return false;

        if (_worldItemPrefab == null)
        {
            Debug.LogWarning("[Inventory] No World Item Prefab assigned, so items can't be dropped.", this);
            return false;
        }

        Scene room = _player.gameObject.scene;
        DroppedItem dropped = new()
        {
            Id = $"Dropped/{++_dropCount}",
            Item = item,
            ScenePath = room.path,
            Position = _player.position + (Vector3)_dropOffset
        };
        _droppedItems.Add(dropped);
        Spawn(dropped, room);

        _slots[_selectedSlot] = null;
        Changed?.Invoke();
        return true;
    }

    /// <summary>
    /// True if the player took the item with this id, so it stays gone when its room loads again.
    /// </summary>
    public bool WasTaken(string itemId)
    {
        return _takenItemIds.Contains(itemId);
    }

    /// <summary>
    /// Remembers that an ItemReaction happened, so it stays that way when its room loads again.
    /// </summary>
    public void MarkReactionDone(string reactionId)
    {
        _doneReactionIds.Add(reactionId);
    }

    public bool IsReactionDone(string reactionId)
    {
        return _doneReactionIds.Contains(reactionId);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Put back the items the player dropped in this room
        foreach (DroppedItem dropped in _droppedItems)
        {
            if (dropped.ScenePath == scene.path)
                Spawn(dropped, scene);
        }
    }

    private void Spawn(DroppedItem dropped, Scene room)
    {
        WorldItem worldItem = Instantiate(_worldItemPrefab, dropped.Position, Quaternion.identity);
        worldItem.name = dropped.Item.name;

        // Belongs to the room, so it unloads with it instead of following the player
        SceneManager.MoveGameObjectToScene(worldItem.gameObject, room);
        worldItem.Initialize(dropped.Item, dropped.Id);
    }
    #endregion
}
