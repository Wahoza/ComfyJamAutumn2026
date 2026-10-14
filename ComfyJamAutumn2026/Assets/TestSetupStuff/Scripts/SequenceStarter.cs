using Sequences;
using UnityEngine;

public class SequenceStarter : MonoBehaviour
{
    [SerializeField] SequenceComponent sequenceComp;
    [NaughtyAttributes.Button()]
    void StartSequence()
    {
        sequenceComp.PlaySequence();
    }

    [NaughtyAttributes.Button()]
    void ReloadSequence()
    {
        sequenceComp.PrepSequence();
    }
}
