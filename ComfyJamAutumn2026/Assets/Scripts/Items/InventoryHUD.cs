using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Shows the Inventory's slots at the bottom of the screen, and the pickup prompt below the item in reach.
/// Builds its own UI at runtime, so it needs no scene setup.
/// </summary>
[RequireComponent(typeof(Inventory))]
public class InventoryHUD : MonoBehaviour, IGameStateListener
{
    private const float PopDuration = 0.2f;
    private const float PopScale = 0.4f;
    // The pixel font is drawn on an 8 pixel grid, so text shadows move one of its pixels
    private const float ShadowOffsetPerFontSize = 1f / 8f;

    private class SlotView
    {
        public RectTransform Root;
        public Image Frame;
        public Image Icon;
        public ItemData Item;
        public bool Popping;
        public float PopStartTime;
    }

    /// <summary>
    /// Text with a dark copy behind it, so it reads on any background.
    /// </summary>
    private class Label
    {
        public RectTransform Root;
        public TextMeshProUGUI Text;
        public TextMeshProUGUI Shadow;

        public void Set(string text)
        {
            if (Text.text == text)
                return;

            Text.text = text;
            Shadow.text = text;
        }
    }

    [Header("Look")]
    [SerializeField] Sprite _slotSprite;
    [Tooltip("Frame of the selected slot. Leave empty to use the Slot Sprite.")]
    [SerializeField] Sprite _selectedSlotSprite;
    [SerializeField] TMP_FontAsset _font;
    [SerializeField] Color _textColor = new(1f, 0.945f, 0.86f, 1f);
    [SerializeField] Color _textShadowColor = new(0.365f, 0.204f, 0.255f, 1f);

    [Header("Layout")]
    [Tooltip("Screen size the HUD is laid out for. It scales with the real screen.")]
    [SerializeField] Vector2 _referenceResolution = new(960f, 540f);
    [Tooltip("Space between two slots.")]
    [SerializeField] float _slotSpacing = 5f;
    [Tooltip("Space between the slots and the bottom of the screen.")]
    [SerializeField] float _bottomMargin = 8f;
    [Tooltip("Largest size of an item picture in its slot. Smaller pictures keep their own pixel size.")]
    [SerializeField] float _iconSize = 32f;
    [Tooltip("How far the selected slot pops up.")]
    [SerializeField] float _selectedRaise = 4f;
    [Tooltip("Draw order among screen canvases. Below 0, dialogue and menus draw over the HUD.")]
    [SerializeField] int _sortingOrder = -1;

    [Header("Text")]
    [SerializeField] float _slotNumberSize = 8f;
    [SerializeField] float _itemNameSize = 16f;
    [SerializeField] float _promptSize = 16f;
    [Tooltip("Shown below the item in reach. {item} is replaced with the item's name.")]
    [SerializeField] string _pickupPrompt = "[E] Pick up {item}";
    [Tooltip("Shown below the item in reach while every slot is taken. {item} is replaced with the item's name.")]
    [SerializeField] string _inventoryFullPrompt = "Hands full";
    [Tooltip("Space between the bottom of the item and the prompt.")]
    [SerializeField] float _promptGap = 2f;

    [Header("Visibility")]
    [Tooltip("Game States that hide the HUD, like taking pictures or reading the journal.")]
    [SerializeField] List<IGameState> _hiddenInGameStates = new();

    private Inventory _inventory;
    private Canvas _canvas;
    private SlotView[] _slots;
    private Label _itemName;
    private Label _prompt;
    private bool _built;
    private bool _hiddenByGameState;

    #region Lifecycle
    private void Start()
    {
        _inventory = GetComponent<Inventory>();

        // This is a duplicate Inventory that is being destroyed
        if (Inventory.Instance != _inventory)
        {
            enabled = false;
            return;
        }

        Build();
        Refresh();
        _built = true;

        _inventory.Changed += Refresh;
        // Runs after the cameras moved for this frame, so the prompt sticks to its item instead of trailing behind it
        Canvas.willRenderCanvases += PlacePrompt;
        (this as IGameStateListener).Subscribe();
    }

    private void OnDestroy()
    {
        if (!_built)
            return;

        _inventory.Changed -= Refresh;
        Canvas.willRenderCanvases -= PlacePrompt;
        (this as IGameStateListener).Unsubscribe();
    }

    public void NotifyStateChange(IGameState newState, IGameState oldState)
    {
        _hiddenByGameState = _hiddenInGameStates.Contains(newState);
    }

    private void LateUpdate()
    {
        // Rooms without a player, like menus, show no HUD
        bool visible = !_hiddenByGameState && _inventory.HasPlayer;
        if (_canvas.enabled != visible)
            _canvas.enabled = visible;

        AnimateIcons();
        UpdatePrompt();
    }
    #endregion

    #region Slots
    private void Refresh()
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            SlotView slot = _slots[i];
            ItemData item = _inventory.GetItem(i);
            bool selected = i == _inventory.SelectedSlot;

            if (item != slot.Item)
            {
                Sprite icon = item != null ? item.Icon : null;
                slot.Item = item;
                slot.Icon.sprite = icon;
                slot.Icon.enabled = icon != null;
                if (icon != null)
                    slot.Icon.rectTransform.sizeDelta = GetIconSize(icon);
                slot.Popping = item != null;
                slot.PopStartTime = Time.unscaledTime;
            }

            slot.Frame.sprite = selected && _selectedSlotSprite != null ? _selectedSlotSprite : _slotSprite;
            slot.Root.anchoredPosition = new Vector2(slot.Root.anchoredPosition.x, selected ? _selectedRaise : 0f);
        }

        ItemData selectedItem = _inventory.GetItem(_inventory.SelectedSlot);
        _itemName.Set(selectedItem != null ? selectedItem.DisplayName : string.Empty);
    }

    /// <summary>
    /// Pixel art that fits keeps its own pixel size so it stays crisp, bigger pictures shrink to fit the slot.
    /// </summary>
    private Vector2 GetIconSize(Sprite icon)
    {
        Vector2 size = icon.rect.size;
        float largest = Mathf.Max(size.x, size.y);
        return largest > _iconSize ? size * (_iconSize / largest) : size;
    }

    /// <summary>
    /// A new item starts a bit bigger and settles into its slot, so it is easy to spot.
    /// </summary>
    private void AnimateIcons()
    {
        foreach (SlotView slot in _slots)
        {
            if (!slot.Popping)
                continue;

            float progress = Mathf.Clamp01((Time.unscaledTime - slot.PopStartTime) / PopDuration);
            float scale = 1f + PopScale * (1f - progress) * (1f - progress);
            slot.Icon.rectTransform.localScale = new Vector3(scale, scale, 1f);
            slot.Popping = progress < 1f;
        }
    }
    #endregion

    #region Prompt
    private void UpdatePrompt()
    {
        WorldItem target = _inventory.Target;
        bool show = target != null && _canvas.enabled;
        if (_prompt.Root.gameObject.activeSelf != show)
            _prompt.Root.gameObject.SetActive(show);

        if (show)
        {
            string prompt = _inventory.IsFull ? _inventoryFullPrompt : _pickupPrompt;
            _prompt.Set(prompt.Replace("{item}", target.Item.DisplayName));
        }
    }

    private void PlacePrompt()
    {
        WorldItem target = _inventory.Target;
        Camera camera = Camera.main;
        if (target == null || camera == null || !_prompt.Root.gameObject.activeSelf)
            return;

        // Screen space overlay canvases use screen pixels as their world space
        Vector3 screenPoint = camera.WorldToScreenPoint(target.PromptAnchor);
        float gap = _promptGap * _canvas.scaleFactor;
        _prompt.Root.position = new Vector3(Mathf.Round(screenPoint.x), Mathf.Round(screenPoint.y - gap), 0f);
    }
    #endregion

    #region Building
    private void Build()
    {
        GameObject canvasObject = new("InventoryCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        canvasObject.transform.SetParent(transform, false);
        _canvas = canvasObject.GetComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = _sortingOrder;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = _referenceResolution;
        scaler.matchWidthOrHeight = 0.5f;

        RectTransform canvasRect = (RectTransform)canvasObject.transform;
        Vector2 bottomCenter = new(0.5f, 0f);

        // Row of slots at the bottom center of the screen, slots keep the frame sprite's size so its pixels stay square
        RectTransform row = CreateRect("Slots", canvasRect, bottomCenter, bottomCenter, bottomCenter);
        row.anchoredPosition = new Vector2(0f, _bottomMargin);

        Vector2 slotSize = _slotSprite != null ? _slotSprite.rect.size : new Vector2(48f, 48f);
        float step = slotSize.x + _slotSpacing;
        _slots = new SlotView[_inventory.SlotCount];
        for (int i = 0; i < _slots.Length; i++)
        {
            float x = (i - (_slots.Length - 1) * 0.5f) * step;
            _slots[i] = CreateSlot(row, i, new Vector2(x, 0f), slotSize);
        }

        _itemName = CreateLabel("SelectedItemName", row, _itemNameSize, TextAlignmentOptions.Bottom, bottomCenter, 400f);
        _itemName.Root.anchoredPosition = new Vector2(0f, slotSize.y + _selectedRaise + 4f);

        // Moved below the item in reach every frame
        _prompt = CreateLabel("PickupPrompt", canvasRect, _promptSize, TextAlignmentOptions.Top, new Vector2(0.5f, 1f), 400f);
        _prompt.Root.gameObject.SetActive(false);
    }

    private SlotView CreateSlot(RectTransform row, int index, Vector2 position, Vector2 size)
    {
        Vector2 bottomCenter = new(0.5f, 0f);
        Vector2 center = new(0.5f, 0.5f);
        Vector2 topLeft = new(0f, 1f);

        RectTransform root = CreateRect($"Slot{index + 1}", row, bottomCenter, bottomCenter, bottomCenter);
        root.anchoredPosition = position;
        root.sizeDelta = size;

        Image frame = AddImage(CreateRect("Frame", root, Vector2.zero, Vector2.one, center), _slotSprite);

        RectTransform iconRect = CreateRect("Icon", root, center, center, center);
        iconRect.sizeDelta = new Vector2(_iconSize, _iconSize);
        Image icon = AddImage(iconRect, null);
        icon.preserveAspect = true;
        icon.enabled = false;

        // Number key that selects this slot
        Label number = CreateLabel("Number", root, _slotNumberSize, TextAlignmentOptions.TopLeft, topLeft, 20f);
        number.Root.anchoredPosition = new Vector2(5f, -4f);
        number.Set((index + 1).ToString());

        return new SlotView { Root = root, Frame = frame, Icon = icon };
    }

    private Label CreateLabel(string name, RectTransform parent, float size, TextAlignmentOptions alignment, Vector2 anchor, float width)
    {
        RectTransform root = CreateRect(name, parent, anchor, anchor, anchor);
        root.sizeDelta = new Vector2(width, size * 1.5f);

        // Created first, so it draws behind the text
        TextMeshProUGUI shadow = AddText(CreateRect("Shadow", root, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f)), size, alignment, _textShadowColor);
        float offset = size * ShadowOffsetPerFontSize;
        shadow.rectTransform.anchoredPosition = new Vector2(offset, -offset);

        TextMeshProUGUI text = AddText(CreateRect("Text", root, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f)), size, alignment, _textColor);
        return new Label { Root = root, Text = text, Shadow = shadow };
    }

    private TextMeshProUGUI AddText(RectTransform rect, float size, TextAlignmentOptions alignment, Color color)
    {
        TextMeshProUGUI text = rect.gameObject.AddComponent<TextMeshProUGUI>();
        if (_font != null)
            text.font = _font;
        text.fontSize = size;
        text.alignment = alignment;
        text.color = color;
        text.textWrappingMode = TextWrappingModes.NoWrap;
        text.overflowMode = TextOverflowModes.Overflow;
        text.raycastTarget = false;
        return text;
    }

    private static Image AddImage(RectTransform rect, Sprite sprite)
    {
        Image image = rect.gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.raycastTarget = false;
        return image;
    }

    private static RectTransform CreateRect(string name, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot)
    {
        RectTransform rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = Vector2.zero;
        return rect;
    }
    #endregion
}
