using UnityEngine;

public class DraggableSubmitAreaComponent : MonoBehaviour
{
    [SerializeField] public string nameKey;
    [SerializeField] public Transform submitPosition;

    private void Start()
    {
        DraggableHandler.Instance.RegisterSubmitLocation(this);
    }
}
