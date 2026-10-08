using Sequences;
using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[System.Serializable]
public class WaitSequenceItem : ISequenceItem
{
    public ISequenceItem nextSequence;
    public SequenceComponent owner;
    public SequenceEnumAlloc.SequenceType type;
    public List<string> awaitedStrings;
    public int minRequiredAwaitedStrings;

    public float awaitedTime;

    private bool quitDuringWait = false;
    private Coroutine waitRoutine;  
    public void Quit(bool complete)
    {
        quitDuringWait = true;

        if(waitRoutine != null)
            owner.StopCoroutine(waitRoutine);

        if (complete)
        {
            nextSequence?.Start();
            nextSequence?.Quit(true);
        }
    }

    public void SetOwnerComponent(SequenceComponent component)
    {
        owner = component;
    }

    public void Start()
    {
        quitDuringWait = false;
        owner.currentItem = this;

        switch (type)
        {
            case SequenceEnumAlloc.SequenceType.WaitForSeconds:
                waitRoutine = owner.StartCoroutine(WaitTimeRoutine());
                break;
            case SequenceEnumAlloc.SequenceType.WaitForBooleanOnBlackboard:
                waitRoutine = owner.StartCoroutine(WaitStringRoutine());
                break;
        }
    }

    private IEnumerator WaitTimeRoutine()
    {
        yield return new WaitForSeconds(awaitedTime);

        if (!quitDuringWait)
        {
            nextSequence?.Start();
        }
    }

    private IEnumerator WaitStringRoutine()
    {
        SequenceBlackboardComponent blackboard = owner.GetComponent<SequenceBlackboardComponent>();
        int ctr = 0;
        while (!quitDuringWait)
        {
            ctr = 0;

            foreach (string str in awaitedStrings)
            {
                if (blackboard.ReadFromDictionary<bool>(str))
                {
                    ctr++;
                }
            }

            if(ctr > minRequiredAwaitedStrings)
            {
                nextSequence.Start();
                yield break;
            }

            yield return null;
        }
    }
    public static void DrawGUI(SerializedProperty serializedObject, SequenceEnumAlloc.SequenceType type) 
    {
        SerializedProperty typeProp = serializedObject.FindPropertyRelative("type");
        SerializedProperty awaitedStringProp = serializedObject.FindPropertyRelative("awaitedStrings");
        SerializedProperty awaitedTimeProp = serializedObject.FindPropertyRelative("awaitedTime");
        SerializedProperty minStringsProp = serializedObject.FindPropertyRelative("minRequiredAwaitedStrings");

        typeProp.enumValueIndex = (int)type;

        if ((SequenceEnumAlloc.SequenceType)typeProp.enumValueIndex == SequenceEnumAlloc.SequenceType.WaitForSeconds) {
            awaitedTimeProp.floatValue = EditorGUILayout.FloatField("Wait Duration", awaitedTimeProp.floatValue);
        }

        if((SequenceEnumAlloc.SequenceType)typeProp.enumValueIndex == SequenceEnumAlloc.SequenceType.WaitForBooleanOnBlackboard){
            EditorGUILayout.PropertyField(awaitedStringProp, new GUIContent($"Completion Strings"));
            minStringsProp.intValue = EditorGUILayout.IntField("Amount Required For Completion", minStringsProp.intValue);
        }
    }

    public static void DefaultInitialize(SerializedProperty serializedObject, SequenceEnumAlloc.SequenceType type) 
    {
        SerializedProperty typeProp = serializedObject.FindPropertyRelative("type");
        typeProp.enumValueIndex = Convert.ToInt32(type);
    }

    public ISequenceItem GetNext()
    {
        return nextSequence;
    }
}
