using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum RoomTransitionStyle
{
    Wipe,
    Fade
}

/// <summary>
/// Full screen overlay that hides the screen while a room loads.
/// Builds its own UI at runtime, so it needs no prefab or scene setup.
/// </summary>
public class RoomTransitionScreen : MonoBehaviour
{
    [Header("Look")]
    [SerializeField] RoomTransitionStyle _style = RoomTransitionStyle.Wipe;
    [SerializeField] Color _coverColor = new(0.11f, 0.08f, 0.06f, 1f);
    [Tooltip("Width of the soft wipe edge, as a fraction of the screen.")]
    [SerializeField, Range(0f, 0.5f)] float _edgeSoftness = 0.2f;
    [SerializeField] int _sortingOrder = 1000;

    [Header("Timing")]
    [SerializeField] float _coverDuration = 0.45f;
    [SerializeField] float _revealDuration = 0.55f;
    [SerializeField] AnimationCurve _coverCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] AnimationCurve _revealCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    Canvas _canvas;
    RectTransform _band;
    CanvasGroup _bandGroup;
    Image[] _bandImages;
    RectTransform _leadingEdge;
    RectTransform _trailingEdge;

    Texture2D _edgeTexture;
    Sprite _edgeSprite;

    #region PublicInterface
    /// <summary>
    /// Animates the overlay in until the screen is fully covered.
    /// </summary>
    /// <param name="exitSide">Side the player leaves through. The wipe moves against it, like the world scrolling past.</param>
    public IEnumerator Cover(DoorSide exitSide)
    {
        ApplyColors();
        _canvas.enabled = true;
        yield return Animate(exitSide, 0f, 1f, _coverDuration, _coverCurve);
    }

    /// <summary>
    /// Animates the overlay out, continuing in the same direction it came in.
    /// </summary>
    public IEnumerator Reveal(DoorSide exitSide)
    {
        yield return Animate(exitSide, 1f, 2f, _revealDuration, _revealCurve);
        _canvas.enabled = false;
    }
    #endregion

    #region Animation
    /// <summary>
    /// Progress 0 is fully hidden before the room change, 1 is fully covered, 2 is fully hidden after.
    /// </summary>
    private IEnumerator Animate(DoorSide exitSide, float from, float to, float duration, AnimationCurve curve)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            ApplyProgress(exitSide, Mathf.LerpUnclamped(from, to, curve.Evaluate(elapsed / duration)));
            yield return null;
            elapsed += Time.unscaledDeltaTime;
        }

        ApplyProgress(exitSide, to);
    }

    private void ApplyProgress(DoorSide exitSide, float progress)
    {
        Vector2 screenSize = new(Screen.width, Screen.height);

        if (_style == RoomTransitionStyle.Fade)
        {
            _band.localRotation = Quaternion.identity;
            _band.sizeDelta = screenSize;
            _band.anchoredPosition = Vector2.zero;
            _leadingEdge.sizeDelta = Vector2.zero;
            _trailingEdge.sizeDelta = Vector2.zero;
            _bandGroup.alpha = 1f - Mathf.Abs(progress - 1f);
            return;
        }

        // The band is laid out along its local X axis and rotated to face the travel direction
        Vector2 travel = -exitSide.ToVector();
        bool horizontal = Mathf.Abs(travel.x) > 0f;
        float along = horizontal ? screenSize.x : screenSize.y;
        float across = horizontal ? screenSize.y : screenSize.x;
        float edge = along * _edgeSoftness;

        _band.localRotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(travel.y, travel.x) * Mathf.Rad2Deg);
        _band.sizeDelta = new Vector2(along, across + 4f);
        _leadingEdge.sizeDelta = new Vector2(edge, 0f);
        _trailingEdge.sizeDelta = new Vector2(edge, 0f);
        _band.anchoredPosition = travel * ((progress - 1f) * (along + edge));
        _bandGroup.alpha = 1f;
    }
    #endregion

    #region Building
    private void Awake()
    {
        Build();
    }

    private void OnDestroy()
    {
        Destroy(_edgeSprite);
        Destroy(_edgeTexture);
    }

    private void Build()
    {
        _edgeSprite = CreateEdgeSprite(out _edgeTexture);

        GameObject canvasObject = new("RoomTransitionCanvas", typeof(RectTransform), typeof(Canvas));
        canvasObject.transform.SetParent(transform, false);
        _canvas = canvasObject.GetComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = _sortingOrder;
        RectTransform canvasRect = (RectTransform)canvasObject.transform;

        // Band: solid center that covers the screen, with a soft edge on each end
        _band = CreateRect("Band", canvasRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
        _bandGroup = _band.gameObject.AddComponent<CanvasGroup>();

        RectTransform solid = CreateRect("Solid", _band, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f));
        _leadingEdge = CreateRect("LeadingEdge", _band, new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(0f, 0.5f));
        _trailingEdge = CreateRect("TrailingEdge", _band, new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0.5f));
        _trailingEdge.localScale = new Vector3(-1f, 1f, 1f);

        _bandImages = new[]
        {
            AddImage(solid, null),
            AddImage(_leadingEdge, _edgeSprite),
            AddImage(_trailingEdge, _edgeSprite)
        };

        ApplyColors();
        _canvas.enabled = false;
    }

    private void ApplyColors()
    {
        foreach (Image image in _bandImages)
            image.color = _coverColor;
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

    private static Image AddImage(RectTransform rect, Sprite sprite)
    {
        Image image = rect.gameObject.AddComponent<Image>();
        image.sprite = sprite;
        image.raycastTarget = false;
        return image;
    }

    /// <summary>
    /// Horizontal gradient, opaque on the left fading to transparent on the right.
    /// </summary>
    private static Sprite CreateEdgeSprite(out Texture2D texture)
    {
        const int width = 64;
        texture = new Texture2D(width, 1, TextureFormat.RGBA32, false)
        {
            name = "RoomTransitionEdge",
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear
        };

        for (int x = 0; x < width; x++)
        {
            float alpha = 1f - Mathf.SmoothStep(0f, 1f, x / (width - 1f));
            texture.SetPixel(x, 0, new Color(1f, 1f, 1f, alpha));
        }
        texture.Apply();

        return Sprite.Create(texture, new Rect(0f, 0f, width, 1f), new Vector2(0.5f, 0.5f));
    }
    #endregion
}
