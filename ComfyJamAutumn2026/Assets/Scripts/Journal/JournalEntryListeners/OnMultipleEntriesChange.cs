using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OnMultipleEntriesChange : MonoBehaviour
{
    [SerializeField] List<string> conditions;
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
        bool success = false;
        foreach (string condition in conditions)
        {
            if (!(Blackboard.Instance.GetIsUnlockedFromJournalDict(condition) || Blackboard.Instance.GetPhotographDictValue(condition)))
            {
                success = false;
            }
        }

        if (success)
        {
            broadcast.Invoke();
        }
    }

}
