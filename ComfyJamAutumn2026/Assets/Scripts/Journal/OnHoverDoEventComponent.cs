using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class OnHoverDoEventComponent : MonoBehaviour
{
    [SerializeField] float hoverDurationForAction;
    [SerializeField] UnityEvent eventToBrodcast;
    bool isEntered;
    float elapsedTime;

    public void Start()
    {
        isEntered = true;
        InputManager.InputActions.Journal.Point.performed += OnMousePositionChange;
    }
    void OnMousePositionChange(InputAction.CallbackContext ctx)
    {
        var mousePos = ctx.ReadValue<Vector2>();
        mousePos = GetComponent<RectTransform>().InverseTransformPoint(mousePos);
        isEntered = GetComponent<RectTransform>().rect.Contains(mousePos);
    }
    void Update()
    {
        if (isEntered)
        {
            Debug.Log("Goo");
            elapsedTime += Time.deltaTime;

            if (elapsedTime > hoverDurationForAction)
            {
                eventToBrodcast.Invoke();
                elapsedTime = 0;
            }
        }
        else
        {
            elapsedTime = 0;
        }
    }
}
