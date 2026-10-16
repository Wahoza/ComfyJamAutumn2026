using UnityEngine;
using UnityEngine.Events;

public class OnJournalEntryChangeComponent : MonoBehaviour
{
    [SerializeField] string conditionName;
    [SerializeField] UnityEvent broadcast;
    private void Start()
    {
        Blackboard.Instance.onJournalUpdate.AddListener(OnJournalEntry);

        CheckForChange();
    }

    void OnJournalEntry()
    {
        CheckForChange();
    }

    void CheckForChange()
    {
        if (Blackboard.Instance.GetIsUnlockedFromJournalDict(conditionName) || Blackboard.Instance.GetPhotographDictValue(conditionName))
        {
            broadcast.Invoke();
        }
    }

}
