using Sequences;
using UnityEngine;

public class SequenceStarter : MonoBehaviour
{
    [NaughtyAttributes.Button()]
    void StartSequence()
    {
        GetComponent<SequenceComponent>().PlaySequence();
    }

    [NaughtyAttributes.Button()]
    void ReloadSequence()
    {
        GetComponent<SequenceComponent>().PrepSequence();
    }
}
