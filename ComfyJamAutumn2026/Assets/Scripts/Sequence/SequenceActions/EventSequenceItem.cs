using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Sequences
{
    [System.Serializable]
    public class EventSequenceItem : ISequenceItem
    {
        public UnityEvent invokedEvent = new();

        public SequenceComponent _ownerSeqComp;

        public ISequenceItem _nextSequence;

        public List<string> conditionStrings = new List<string>();
        public List<UnityEvent> eventForCondition = new List<UnityEvent>();
        public List<bool> isFoldedOut = new List<bool>();

        public SequenceEnumAlloc.SequenceType type;

        private bool _bQuitAtStart;
        public virtual void Start()
        {
            if (type == SequenceEnumAlloc.SequenceType.BroadcastEvent) 
            {
                InvokeEvent();
            }
            else if (type == SequenceEnumAlloc.SequenceType.BroadcastConditionalEvent)
            {
                InvokeConditionalEvents();
            }
                _ownerSeqComp.StartCoroutine(WaitTickForNextStart());
            _ownerSeqComp.currentItem = this;
        }

        public virtual void Quit(bool complete)
        {
            _bQuitAtStart = true;

            if (complete)
            {
                _nextSequence?.Start();
                _nextSequence?.Quit(true);
            }
        }

        IEnumerator WaitTickForNextStart()
        {
            _bQuitAtStart = false;
            yield return null;

            if (!_bQuitAtStart)
            {
                _nextSequence?.Start();
            }
        }

        private void InvokeEvent()
        {
            invokedEvent.Invoke();
        }

        private void InvokeConditionalEvents()
        {
            var blackboard = _ownerSeqComp.GetComponent<SequenceBlackboardComponent>();
            for (int i = 0; i < conditionStrings.Count; i++)
            {
                if (blackboard.ReadFromDictionary<bool>(conditionStrings[i]))
                {
                    eventForCondition[i].Invoke();
                }
            }
        }

        private IEnumerator DurationUpdateRoutine()
        {
            yield return null;
        }

        public static void DrawGUI(SerializedProperty serializedObject, SequenceEnumAlloc.SequenceType type)
        {
            SerializedProperty unityEventProp = serializedObject.FindPropertyRelative("invokedEvent");

            SerializedProperty conditionStringsProp = serializedObject.FindPropertyRelative("conditionStrings");
            SerializedProperty eventForConditionProp = serializedObject.FindPropertyRelative("eventForCondition");
            SerializedProperty foldedOutProp = serializedObject.FindPropertyRelative("isFoldedOut");

            SerializedProperty typeProp = serializedObject.FindPropertyRelative("type");
            typeProp.enumValueIndex = Convert.ToInt32(type);

            if((SequenceEnumAlloc.SequenceType)typeProp.enumValueIndex == SequenceEnumAlloc.SequenceType.BroadcastEvent)
            {
                EditorGUILayout.PropertyField(unityEventProp, new GUIContent("Brodcast Event"));
            }
            else if ((SequenceEnumAlloc.SequenceType)typeProp.enumValueIndex == SequenceEnumAlloc.SequenceType.BroadcastConditionalEvent)
            {
                if (GUILayout.Button("Add new conditional event"))
                {
                    conditionStringsProp.InsertArrayElementAtIndex(0);
                    conditionStringsProp.serializedObject.ApplyModifiedProperties();
                    conditionStringsProp.GetArrayElementAtIndex(0).stringValue = "";

                    foldedOutProp.InsertArrayElementAtIndex(0);
                    foldedOutProp.serializedObject.ApplyModifiedProperties();
                    foldedOutProp.GetArrayElementAtIndex(0).boolValue = true;

                    eventForConditionProp.InsertArrayElementAtIndex(0);
                }

                EditorGUI.indentLevel++;

                for(int i = 0; i < conditionStringsProp.arraySize; i++)
                {
                    SerializedProperty foldProp = foldedOutProp.GetArrayElementAtIndex(i);
                    SerializedProperty stringProp = conditionStringsProp.GetArrayElementAtIndex(i);
                    SerializedProperty eventProp = eventForConditionProp.GetArrayElementAtIndex(i);

                    foldProp.boolValue = EditorGUILayout.Foldout(foldProp.boolValue, $"{stringProp.stringValue} Event");

                    if (foldProp.boolValue)
                    {
                        stringProp.stringValue = EditorGUILayout.TextField("Condition", stringProp.stringValue);
                        EditorGUILayout.PropertyField(eventProp, new GUIContent($"Brodcast Event if {stringProp.stringValue}"));


                        if (GUILayout.Button("Insert New Event"))
                        {
                            conditionStringsProp.InsertArrayElementAtIndex(0);
                            conditionStringsProp.serializedObject.ApplyModifiedProperties();
                            conditionStringsProp.GetArrayElementAtIndex(0).stringValue = "";

                            foldedOutProp.InsertArrayElementAtIndex(0);
                            foldedOutProp.serializedObject.ApplyModifiedProperties();
                            foldedOutProp.GetArrayElementAtIndex(0).boolValue = true;

                            eventForConditionProp.InsertArrayElementAtIndex(0);
                        }
                        if (GUILayout.Button("Delete Event"))
                        {
                            conditionStringsProp.DeleteArrayElementAtIndex(i);
                            eventForConditionProp.DeleteArrayElementAtIndex(i);
                            foldedOutProp.DeleteArrayElementAtIndex(i);
                        }
                    }

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();

                }
                EditorGUI.indentLevel--;

                if (GUILayout.Button("Clear Events"))
                {
                    conditionStringsProp.ClearArray();
                    eventForConditionProp.ClearArray();
                    foldedOutProp.ClearArray();
                }
                EditorGUILayout.Space();
                EditorGUILayout.Space();

            }
        }
        public void SetOwnerComponent(SequenceComponent component)
        {
            _ownerSeqComp = component;
        }

        public static void DefaultInitialize(SerializedProperty serializedObject, SequenceEnumAlloc.SequenceType type)
        {
            
        }

        public ISequenceItem GetNext()
        {
            return _nextSequence;
        }
    }
}

