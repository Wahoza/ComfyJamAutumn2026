using Sequences;
using UnityEditor;
using UnityEngine;

[System.Serializable]
public class WaitSequenceItem : ISequenceItem
{
    public ISequenceItem nextSequence;
    public SequenceComponent owner;
    public void Quit(bool complete)
    {
    }

    public void SetOwnerComponent(SequenceComponent component)
    {
        owner = component;
    }

    public void Start()
    {
    }

    public void Update(float deltaTime)
    {
    }

    public static void DrawGUI(SerializedProperty serializedObject, SequenceEnumAlloc.SequenceType type) { }

    public static void DefaultInitialize(SerializedProperty serializedObject, SequenceEnumAlloc.SequenceType type) { }
}
