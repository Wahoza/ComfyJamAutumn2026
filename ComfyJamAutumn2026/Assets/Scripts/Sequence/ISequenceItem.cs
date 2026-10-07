using Sequences;
using System;
using System.Runtime.Serialization;
using UnityEditor;
using UnityEngine;

public interface ISequenceItem
{
    void Start();
    void Quit(bool complete);
    void SetOwnerComponent(SequenceComponent component);

    ISequenceItem GetNext();
    //static void DrawGUI(Transform targetObject, AnimSequenceComponent ownerAnimComp, SerializedProperty serializedObject, AnimEnumAlloc.AnimationTypes type);
}
