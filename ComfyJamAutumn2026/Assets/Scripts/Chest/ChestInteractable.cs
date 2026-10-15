using UnityEngine;

public class ChestInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] int code;
    [SerializeField] GameObject returnedItem;
    [SerializeField] Transform returnedItemPosition;
    void Start()
    {
        if (ChestManager.Instance.GetIsCompleted(gameObject))
        {
            OnSuccessfulInput();
        }
    }
    public void Interact()
    {
        ChestManager.Instance.OnChestStart(this);
    }

    public void CheckForCode(int input)
    {
        if(input == code)
        {
            OnSuccessfulInput();
        }
    }

    private void OnSuccessfulInput()
    {
        if(returnedItem != null)
        {
            var item = Instantiate(returnedItem);
            item.transform.position = returnedItemPosition != null ? returnedItemPosition.position : transform.position;
        }
    }
}
