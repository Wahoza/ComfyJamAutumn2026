using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Makes an object react to an item lying near it. Appear brings an animal out while the item lies there,
/// and it leaves again when the item is picked up. Change Sprite and Disappear happen once and stay, and the room remembers them.
/// An item with a Player Leave Distance makes the reaction wait until the player has walked that far away.
/// </summary>
public class ItemReaction : MonoBehaviour
{
    public enum Effect
    {
        /// <summary>Hidden until the item lies near it, then fades in. Fades out again when the item is picked up.</summary>
        Appear,
        /// <summary>Switches to the New Sprite for good.</summary>
        ChangeSprite,
        /// <summary>Fades out until it is invisible, for good.</summary>
        Disappear
    }

    [Header("Trigger")]
    [Tooltip("Item that sets off the reaction.")]
    [SerializeField] ItemData _item;
    [Tooltip("How close to this object the item has to lie. How far the player has to walk away first is set on the item.")]
    [SerializeField] float _itemRange = 1.5f;

    [Header("Effect")]
    [Tooltip("Appear: shows up while the item lies near it, leaves when it is picked up. Change Sprite: switches to the New Sprite for good. Disappear: fades out for good.")]
    [SerializeField] Effect _effect;
    [Tooltip("Sprite that appears, changes or fades away.")]
    [SerializeField] SpriteRenderer _target;
    [Tooltip("Only used by Change Sprite.")]
    [SerializeField] Sprite _newSprite;
    [Tooltip("Only used by Appear and Disappear.")]
    [SerializeField] float _fadeDuration = 0.6f;
    [Tooltip("Shown while the reaction hasn't happened, like sparkles marking the spot. Optional.")]
    [SerializeField] GameObject _hint;

    [Header("Dialogue")]
    [Tooltip("Requirement Id of an NPC conversation this reaction unlocks when it happens. -1 unlocks nothing.")]
    [SerializeField] int _completesRequirement = -1;

    private string _id;
    private bool _active;
    private float _visibleAlpha;
    private Sprite _originalSprite;
    // Only the parts that were switched on to begin with are switched back on
    private readonly List<Renderer> _renderers = new();
    private readonly List<Behaviour> _solidParts = new();

    #region ReactionHandling
    private void Awake()
    {
        if (_item == null || _target == null)
        {
            Debug.LogWarning($"[ItemReaction] '{name}' needs an Item and a Target to react.", this);
            enabled = false;
            return;
        }

        _id = HierarchyId.Of(transform);
        _visibleAlpha = _target.color.a;
        _originalSprite = _target.sprite;
        CollectParts();

        // Change Sprite and Disappear may have happened on an earlier visit, Appear only depends on the item lying there now
        bool happenedBefore = _effect != Effect.Appear && Inventory.Instance != null && Inventory.Instance.IsReactionDone(_id);
        SetActive(happenedBefore, false);
    }

    private void Update()
    {
        if (Inventory.Instance == null)
            return;

        bool itemNearby = IsItemNearby();
        if (!_active && itemNearby && IsPlayerFarEnough())
        {
            if (_effect != Effect.Appear)
                Inventory.Instance.MarkReactionDone(_id);
            if (_completesRequirement >= 0)
                Blackboard.Instance.UpdateReqDict(_completesRequirement, true);
            SetActive(true, true);
        }
        else if (_active && _effect == Effect.Appear && !itemNearby)
        {
            // The animal leaves with the item
            SetActive(false, true);
        }
    }

    private bool IsItemNearby()
    {
        foreach (WorldItem worldItem in WorldItem.ActiveItems)
        {
            if (worldItem.Item == _item && Vector2.Distance(worldItem.transform.position, transform.position) <= _itemRange)
                return true;
        }
        return false;
    }

    private bool IsPlayerFarEnough()
    {
        float leaveDistance = _item.PlayerLeaveDistance;
        if (leaveDistance <= 0f)
            return true;

        Transform player = Inventory.Instance.Player;
        return player != null && Vector2.Distance(player.position, transform.position) >= leaveDistance;
    }

    private void SetActive(bool active, bool animate)
    {
        _active = active;
        if (_hint != null)
            _hint.SetActive(!active);

        StopAllCoroutines();
        switch (_effect)
        {
            case Effect.Appear:
                Show(active, animate);
                break;

            case Effect.ChangeSprite:
                _target.sprite = active ? _newSprite : _originalSprite;
                break;

            case Effect.Disappear:
                Show(!active, animate);
                break;
        }
    }
    #endregion

    #region Visibility
    private void CollectParts()
    {
        foreach (Renderer part in _target.GetComponentsInChildren<Renderer>())
        {
            if (part.enabled)
                _renderers.Add(part);
        }

        // Colliders would still block the player and snapshot boundings would still show up in pictures
        foreach (Collider2D part in _target.GetComponentsInChildren<Collider2D>())
        {
            if (part.enabled)
                _solidParts.Add(part);
        }
        foreach (SnapshotableObject part in _target.GetComponentsInChildren<SnapshotableObject>())
        {
            if (part.enabled)
                _solidParts.Add(part);
        }
    }

    private void Show(bool visible, bool animate)
    {
        if (!animate)
        {
            SetPresent(visible);
            SetAlpha(visible ? _visibleAlpha : 0f);
        }
        else if (visible)
        {
            SetPresent(true);
            StartCoroutine(Fade(_target.color.a, _visibleAlpha));
        }
        else
        {
            StartCoroutine(FadeOut());
        }
    }

    /// <summary>
    /// A hidden object can't be seen, bumped into or photographed.
    /// </summary>
    private void SetPresent(bool present)
    {
        foreach (Renderer part in _renderers)
            part.enabled = present;
        foreach (Behaviour part in _solidParts)
            part.enabled = present;
    }

    private void SetAlpha(float alpha)
    {
        Color color = _target.color;
        color.a = alpha;
        _target.color = color;
    }

    private IEnumerator Fade(float from, float to)
    {
        for (float elapsed = 0f; elapsed < _fadeDuration; elapsed += Time.deltaTime)
        {
            SetAlpha(Mathf.Lerp(from, to, elapsed / _fadeDuration));
            yield return null;
        }
        SetAlpha(to);
    }

    private IEnumerator FadeOut()
    {
        yield return Fade(_target.color.a, 0f);
        SetPresent(false);
    }
    #endregion

#if UNITY_EDITOR
    #region EditorHandling
    private void Reset()
    {
        _target = GetComponentInChildren<SpriteRenderer>();
    }

    private void OnDrawGizmosSelected()
    {
        // Where the item has to land
        Gizmos.color = new Color(1f, 0.75f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, _itemRange);

        // How far the player has to walk away
        if (_item != null && _item.PlayerLeaveDistance > 0f)
        {
            Gizmos.color = new Color(0.4f, 0.75f, 1f);
            Gizmos.DrawWireSphere(transform.position, _item.PlayerLeaveDistance);
        }
    }
    #endregion
#endif
}
