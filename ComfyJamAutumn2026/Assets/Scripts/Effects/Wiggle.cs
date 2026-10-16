using UnityEngine;

/// <summary>
/// Rocks an object gently from side to side and squashes it a little, so animals look alive.
/// </summary>
public class Wiggle : MonoBehaviour
{
    [Tooltip("How far it tilts each way, in degrees.")]
    [SerializeField] float _angle = 6f;
    [Tooltip("Wiggles per second.")]
    [SerializeField] float _speed = 1.2f;
    [Tooltip("How much it squashes and stretches. 0.05 is 5%.")]
    [SerializeField] float _squash = 0.05f;

    private Quaternion _startRotation;
    private Vector3 _startScale;
    private float _timeOffset;

    private void Awake()
    {
        _startRotation = transform.localRotation;
        _startScale = transform.localScale;
        // Animals next to each other don't move in step
        _timeOffset = Random.value * 10f;
    }

    private void Update()
    {
        float phase = (Time.time + _timeOffset) * _speed * 2f * Mathf.PI;
        transform.localRotation = _startRotation * Quaternion.Euler(0f, 0f, Mathf.Sin(phase) * _angle);

        float squash = Mathf.Sin(phase * 2f) * _squash;
        transform.localScale = new Vector3(_startScale.x * (1f + squash), _startScale.y * (1f - squash), _startScale.z);
    }

    private void OnDisable()
    {
        transform.localRotation = _startRotation;
        transform.localScale = _startScale;
    }
}
