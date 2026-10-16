using UnityEngine;

/// <summary>
/// An item the player can pick up, carry and drop. Every item takes up a whole inventory slot, items don't stack.
/// </summary>
[CreateAssetMenu(fileName = "NewItem", menuName = "Scriptable Objects/Item")]
public class ItemData : ScriptableObject
{
    [Tooltip("Name shown in the pickup prompt and above the inventory. Leave empty to use the asset name.")]
    [SerializeField] string _displayName;
    [Tooltip("Picture shown in the inventory slot. Leave empty to use the World Sprite.")]
    [SerializeField] Sprite _icon;
    [Tooltip("Picture shown while the item lies in the world. Leave empty to use the Icon.")]
    [SerializeField] Sprite _worldSprite;

    public string DisplayName => string.IsNullOrWhiteSpace(_displayName) ? name : _displayName.Trim();
    public Sprite Icon => _icon != null ? _icon : _worldSprite;
    public Sprite WorldSprite => _worldSprite != null ? _worldSprite : _icon;
}
