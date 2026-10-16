using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Small sparkles that twinkle at random spots around this object, to show the player something can happen here.
/// Builds its sparkles at runtime, so it only needs a sprite.
/// </summary>
public class Sparkles : MonoBehaviour
{
    private class Sparkle
    {
        public Transform Transform;
        public float StartTime;
    }

    [SerializeField] Sprite _sprite;
    [SerializeField] Color _color = Color.white;
    [Tooltip("Half the width and height of the area the sparkles appear in.")]
    [SerializeField] Vector2 _area = new(1f, 1f);
    [SerializeField, Min(1)] int _count = 6;
    [Tooltip("Largest size of a sparkle, 1 is the sprite's own size.")]
    [SerializeField] float _size = 2.5f;
    [Tooltip("Seconds one sparkle takes to grow and shrink away.")]
    [SerializeField] float _twinkleDuration = 1.4f;
    [Tooltip("Longest pause before a sparkle twinkles again somewhere else.")]
    [SerializeField] float _maxPause = 0.5f;
    [SerializeField] string _sortingLayer = "OnGround";
    [SerializeField] int _sortingOrder = 20;

    private readonly List<Sparkle> _sparkles = new();

    private void Awake()
    {
        for (int i = 0; i < _count; i++)
        {
            SpriteRenderer sparkle = new GameObject("Sparkle", typeof(SpriteRenderer)).GetComponent<SpriteRenderer>();
            sparkle.transform.SetParent(transform, false);
            sparkle.sprite = _sprite;
            sparkle.color = _color;
            sparkle.sortingLayerName = _sortingLayer;
            sparkle.sortingOrder = _sortingOrder;
            sparkle.transform.localScale = Vector3.zero;

            // Spread out from the start, so they don't all twinkle together
            Sparkle entry = new() { Transform = sparkle.transform };
            MoveSomewhereElse(entry, Time.time + _twinkleDuration * i / _count);
            _sparkles.Add(entry);
        }
    }

    private void Update()
    {
        foreach (Sparkle sparkle in _sparkles)
        {
            float progress = (Time.time - sparkle.StartTime) / _twinkleDuration;
            if (progress >= 1f)
            {
                MoveSomewhereElse(sparkle, Time.time + Random.Range(0f, _maxPause));
                progress = 0f;
            }

            // Grows in and shrinks out, and stays hidden while it waits to start
            float scale = progress > 0f ? Mathf.Sin(progress * Mathf.PI) * _size : 0f;
            sparkle.Transform.localScale = new Vector3(scale, scale, 1f);
        }
    }

    private void MoveSomewhereElse(Sparkle sparkle, float startTime)
    {
        Vector2 spot = Random.insideUnitCircle;
        sparkle.Transform.localPosition = new Vector3(spot.x * _area.x, spot.y * _area.y, 0f);
        sparkle.StartTime = startTime;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Area the sparkles appear in
        Gizmos.color = new Color(1f, 0.95f, 0.6f);
        Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.Scale(new Vector3(_area.x, _area.y, 1f));
        Gizmos.DrawWireSphere(Vector3.zero, 1f);
    }
#endif
}
