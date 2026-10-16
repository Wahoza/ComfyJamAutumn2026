using UnityEngine;

public class NoteComponent : MonoBehaviour, IInteractable
{
    [SerializeField] string content;
    [SerializeField] string blackboardData;
    public void Interact()
    {
        NoteManager.Instance.OnNoteInteract(content);

        Blackboard.Instance.SetJournalDictBool(blackboardData, true);
    }
}
