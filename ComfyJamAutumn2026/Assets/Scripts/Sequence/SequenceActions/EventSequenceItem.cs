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

        public SequenceComponent ownerSeqComp;

        public ISequenceItem nextSequence;

        private bool quitAtStart;
        public virtual void Start()
        {
            invokedEvent.Invoke();
            ownerSeqComp.StartCoroutine(WaitTickForNextStart());
        }

        public virtual void Quit(bool complete)
        {
            quitAtStart = true;

            if (complete)
            {
                nextSequence?.Start();
                nextSequence?.Quit(true);
            }
        }

        IEnumerator WaitTickForNextStart()
        {
            quitAtStart = false;
            yield return null;

            if (!quitAtStart)
            {
                nextSequence?.Start();
            }
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
            ownerSeqComp = component;
        }

        public static void DefaultInitialize(SerializedProperty serializedObject, SequenceEnumAlloc.SequenceType type)
        {
            
        }
    }
}

