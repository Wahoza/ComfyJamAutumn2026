using UnityEngine;
using UnityEngine.UI;

public class DraggableComponent : MonoBehaviour
{
    //Adds key to dictionary if placed on target that also holds key
    [SerializeField] public string nameKey;

    public bool isSubmittedCorrectly = false;
    private void Start()
    {
        DraggableHandler.Instance.RegisterDraggable(this);
    }

    public void OnHover()
    {
    }

    public void OnClick()
    {
        if(!isSubmittedCorrectly)
            DraggableHandler.Instance.OnDraggableClick(this);
    }
}
