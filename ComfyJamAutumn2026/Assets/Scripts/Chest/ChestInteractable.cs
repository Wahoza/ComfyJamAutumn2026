using UnityEngine;

public class ChestInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] public int code;
    [SerializeField] public string uID;
    [SerializeField] GameObject returnedItem;
    [SerializeField] Transform returnedItemPosition;
    [SerializeField] Sprite openedSprite;

    private bool completed = false;

    void Start()
    {
        if (ChestManager.Instance.GetIsCompleted(this))
        {
            OnSuccessfulInput();
        }
    }
    public void Interact()
    {
        if(!completed)
            ChestManager.Instance.OnChestStart(this);
    }

    public bool CheckForCode(int input)
    {
        if(input == code)
        {
            OnSuccessfulInput();
        }

        return input == code;    
    }

    private void OnSuccessfulInput()
    {
        completed = true;
        ChestManager.Instance.SetIsCompleted(this);

        if(returnedItem != null)
        {
            var item = Instantiate(returnedItem);
            item.transform.position = returnedItemPosition != null ? returnedItemPosition.position : transform.position;
        }
        if (GetComponent<SpriteRenderer>() != null) { 
            GetComponent<SpriteRenderer>().sprite = openedSprite;
        }
    }
}
