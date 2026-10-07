using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Sequences
{
    [System.Serializable]
    public class EventSequenceItem : ISequenceItem
    {
        public UnityEvent invokedEvent = new();

        public SequenceComponent ownerAnimComp;

        public ISequenceItem nextSequence;
        public virtual void Start()
        {
            ownerAnimComp.StartCoroutine(DurationUpdateRoutine());

        }

        public virtual void Quit(bool complete)
        {
        }

        public virtual void Update(float elapsedTime)
        {
        }

        private IEnumerator DurationUpdateRoutine()
        {
            yield return null;
        }

        public static void DrawGUI(SerializedProperty serializedObject, SequenceEnumAlloc.SequenceType type)
        {
            SerializedProperty unityEventProp = serializedObject.FindPropertyRelative("invokedEvent");
            EditorGUILayout.PropertyField(unityEventProp);
        }

        public void SetOwnerComponent(SequenceComponent component)
        {
            ownerAnimComp = component;
        }

        public static void DefaultInitialize(SerializedProperty serializedObject, SequenceEnumAlloc.SequenceType type)
        {
            
        }
    }
}

