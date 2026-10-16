using UnityEngine;

public class NoteComponent : MonoBehaviour, IInteractable
{
    [SerializeField] string content;
    public void Interact()
    {
        NoteManager.Instance.OnNoteInteract(content);
    }
}
