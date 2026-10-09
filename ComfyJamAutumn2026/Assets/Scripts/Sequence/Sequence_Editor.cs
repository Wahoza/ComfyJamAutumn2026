using System;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace Sequences
{
    [CustomEditor(typeof(SequenceComponent))]
    public class Sequence_Editor : Editor
    {
        SerializedProperty serializedDataForEditor;

        SerializedProperty serializedTransformSequenceItems;
        SerializedProperty transformSequenceItemIndexes;

        SerializedProperty serializedEventSequenceItems;
        SerializedProperty eventSequenceItemIndexes;

        SerializedProperty serializedWaitSequenceItems;
        SerializedProperty waitSequenceItemIndexes;
        SerializedProperty targetToSequenceProp;

        private void OnEnable()
        {
            serializedDataForEditor = serializedObject.FindProperty("serializedDataForEditor");

            serializedTransformSequenceItems = serializedObject.FindProperty("serializedTransformSequenceItems");
            transformSequenceItemIndexes = serializedObject.FindProperty("transformSequenceItemIndexes"); 

            serializedEventSequenceItems = serializedObject.FindProperty("serializedEventSequenceItems");
            eventSequenceItemIndexes = serializedObject.FindProperty("serializedEventSequenceItemIndexes");

            serializedWaitSequenceItems = serializedObject.FindProperty("serializedWaitSequenceItems");
            waitSequenceItemIndexes = serializedObject.FindProperty("serializedWaitSequenceItemIndexes");
            targetToSequenceProp = serializedObject.FindProperty("targetToApplySequence");
        }

        /// <summary>
        /// Draw UI
        /// </summary>
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            var script = (SequenceComponent)target;


            targetToSequenceProp.objectReferenceValue = (Transform)EditorGUILayout.ObjectField("SequenceTarget", targetToSequenceProp.objectReferenceValue ? targetToSequenceProp.objectReferenceValue : script.transform, typeof(Transform), allowSceneObjects: true);


            EditorGUILayout.Space();
            EditorGUILayout.Space();

            GUILayout.Label("Animation Sequence");

            EditorGUILayout.Space();

            EditorGUI.indentLevel = 1;

            //    script.animationSequence = new();

            if (GUILayout.Button("Add New"))
            {
                script.AddNew();
            }

            while(script.insertionIndexQueue.Count > 0)
            {

#region Update Indices

                for (int i = 0; i < transformSequenceItemIndexes.arraySize; i++)
                {
                    SerializedProperty intRef = transformSequenceItemIndexes.GetArrayElementAtIndex(i);
                    int initValue = intRef.intValue;

                    if(script.insertionIndexQueue.Peek() <= intRef.intValue + 1)
                        intRef.intValue = (intRef.intValue + 1);

                }
                for (int i = 0; i < eventSequenceItemIndexes.arraySize; i++)
                {
                    SerializedProperty intRef = eventSequenceItemIndexes.GetArrayElementAtIndex(i);
                    int initValue = intRef.intValue;

                    if (script.insertionIndexQueue.Peek() <= intRef.intValue + 1)
                        intRef.intValue = (intRef.intValue + 1);

                }
                for (int i = 0; i < waitSequenceItemIndexes.arraySize; i++)
                {
                    SerializedProperty intRef = waitSequenceItemIndexes.GetArrayElementAtIndex(i);
                    int initValue = intRef.intValue;

                    if (script.insertionIndexQueue.Peek() <= intRef.intValue + 1)
                        intRef.intValue = (intRef.intValue + 1);

                }
#endregion
                int insertionPos = script.insertionIndexQueue.Dequeue();
                SerializedProperty newItem;
                if (serializedDataForEditor.arraySize >= insertionPos)
                {
                    serializedDataForEditor.InsertArrayElementAtIndex(insertionPos);
                    newItem = serializedDataForEditor.GetArrayElementAtIndex(insertionPos);
                }
                else
                {
                    serializedDataForEditor.InsertArrayElementAtIndex(serializedDataForEditor.arraySize);
                    newItem = serializedDataForEditor.GetArrayElementAtIndex(serializedDataForEditor.arraySize - 1);
                }

                newItem.FindPropertyRelative("isFoldedOut").boolValue = false;
                newItem.FindPropertyRelative("type").enumValueIndex = Convert.ToInt32(SequenceEnumAlloc.SequenceType.None);
            }


            EditorGUI.indentLevel = 2;

            for (int i = 0; i < serializedDataForEditor.arraySize; i++)
            {

                SerializedProperty listRef = serializedDataForEditor.GetArrayElementAtIndex(i);
                SerializedProperty isFoldedOut = listRef.FindPropertyRelative("isFoldedOut");
                SerializedProperty overrideTarget = listRef.FindPropertyRelative("overrideTarget");
                SerializedProperty target = listRef.FindPropertyRelative("target");
                SerializedProperty type = listRef.FindPropertyRelative("type");

                isFoldedOut.boolValue = EditorGUILayout.Foldout(isFoldedOut.boolValue, $"{i} - {(SequenceEnumAlloc.SequenceType)type.enumValueIndex}");

                if (isFoldedOut.boolValue)
                {
                    EditorGUI.indentLevel++;

                    var newType = Convert.ToInt32(EditorGUILayout.EnumPopup("Type", (SequenceEnumAlloc.SequenceType)type.enumValueIndex));
                    if(type.enumValueIndex != newType)
                    {
                        ModifyTypeAtIndex(i, (SequenceEnumAlloc.SequenceType)newType, target, script, 
                            SequenceEnumAlloc.TypeToClassDictionary[(SequenceEnumAlloc.SequenceType)newType] != SequenceEnumAlloc.TypeToClassDictionary[(SequenceEnumAlloc.SequenceType)type.enumValueIndex]
                        );

                        type.enumValueIndex = newType;
                    }

                    if (SequenceEnumAlloc.TypeToClassDictionary[(SequenceEnumAlloc.SequenceType)type.enumValueIndex] == typeof(TransformCurveSequenceItem))
                    {
                        overrideTarget.boolValue = EditorGUILayout.Toggle("Override Target", overrideTarget.boolValue);

                        if (overrideTarget.boolValue)
                        {
                            target.objectReferenceValue = EditorGUILayout.ObjectField("Target Transform", target.objectReferenceValue, typeof(Transform), true);
                        }
                        else
                        {
                            target.objectReferenceValue = script.targetToApplySequence;
                        }
                    }


#region Custom Draw Call

                    bool foundAnimCurveItem = false; 
                    
                    for (int SeqIterator = 0; SeqIterator < transformSequenceItemIndexes.arraySize; SeqIterator++)
                    {
                        if (transformSequenceItemIndexes.GetArrayElementAtIndex(SeqIterator).intValue == i)
                        {
                            TransformCurveSequenceItem.DrawGUI(target, serializedTransformSequenceItems.GetArrayElementAtIndex(SeqIterator), (SequenceEnumAlloc.SequenceType)type.enumValueIndex);
                            foundAnimCurveItem = true;
                            break;
                        }
                    }

                    bool foundEventItem = false;

                    if (!foundAnimCurveItem)
                    {
                        for (int SeqIterator = 0; SeqIterator < eventSequenceItemIndexes.arraySize; SeqIterator++)
                        {
                            if (eventSequenceItemIndexes.GetArrayElementAtIndex(SeqIterator).intValue == i)
                            {
                                EventSequenceItem.DrawGUI(serializedEventSequenceItems.GetArrayElementAtIndex(SeqIterator), (SequenceEnumAlloc.SequenceType)type.enumValueIndex);
                                foundEventItem = true;
                                break;
                            }
                        }
                    }

                    if(!foundAnimCurveItem && !foundEventItem)
                    {
                        for (int SeqIterator = 0; SeqIterator < waitSequenceItemIndexes.arraySize; SeqIterator++)
                        {
                            if (waitSequenceItemIndexes.GetArrayElementAtIndex(SeqIterator).intValue == i)
                            {
                                WaitSequenceItem.DrawGUI(serializedWaitSequenceItems.GetArrayElementAtIndex(SeqIterator), (SequenceEnumAlloc.SequenceType)type.enumValueIndex);
                                break;
                            }
                        }
                    }

#endregion

                    #region Deletion

                    if (GUILayout.Button("Delete"))
                    {
                        serializedDataForEditor.DeleteArrayElementAtIndex(i);

                        for (int seqIterator = 0; seqIterator < transformSequenceItemIndexes.arraySize; seqIterator++)
                        {
                            if (transformSequenceItemIndexes.GetArrayElementAtIndex(seqIterator).intValue == i)
                            {
                                transformSequenceItemIndexes.DeleteArrayElementAtIndex(seqIterator);
                                serializedTransformSequenceItems.DeleteArrayElementAtIndex(seqIterator);
                                break;
                            }
                        }

                        for (int seqIterator = 0; seqIterator < eventSequenceItemIndexes.arraySize; seqIterator++)
                        {
                            if (eventSequenceItemIndexes.GetArrayElementAtIndex(seqIterator).intValue == i)
                            {
                                eventSequenceItemIndexes.DeleteArrayElementAtIndex(seqIterator);
                                serializedEventSequenceItems.DeleteArrayElementAtIndex(seqIterator);
                                break;
                            }
                        }

                        for (int seqIterator = 0; seqIterator < waitSequenceItemIndexes.arraySize; seqIterator++)
                        {
                            if (waitSequenceItemIndexes.GetArrayElementAtIndex(seqIterator).intValue == i)
                            {
                                waitSequenceItemIndexes.DeleteArrayElementAtIndex(seqIterator);
                                serializedWaitSequenceItems.DeleteArrayElementAtIndex(seqIterator);
                                break;
                            }
                        }
                    }

                    #endregion

                    if (GUILayout.Button("Insert New Below"))
                    {
                        script.InsertNewAt(i + 1);
                    }

                    EditorGUI.indentLevel--;

                }
            }
            EditorGUI.indentLevel = 0;



            if (GUILayout.Button("Clear"))
            {
                serializedDataForEditor.ClearArray();

                serializedTransformSequenceItems.ClearArray();
                transformSequenceItemIndexes.ClearArray();

                serializedEventSequenceItems.ClearArray();
                eventSequenceItemIndexes.ClearArray();

                serializedWaitSequenceItems.ClearArray();
                waitSequenceItemIndexes.ClearArray();
            }



            EditorGUI.indentLevel = 2;
            serializedObject.ApplyModifiedProperties();

        }

        /// <summary>
        /// Modify type
        /// </summary>

        public void ModifyTypeAtIndex(int index, SequenceEnumAlloc.SequenceType newType, SerializedProperty targetObject, SequenceComponent script, bool isNewType)
        {
            if(isNewType){
                #region make room for new objects
                for (int seqIterator = 0; seqIterator < transformSequenceItemIndexes.arraySize; seqIterator++)
                {
                    if (transformSequenceItemIndexes.GetArrayElementAtIndex(seqIterator).intValue == index)
                    {
                        transformSequenceItemIndexes.DeleteArrayElementAtIndex(seqIterator);
                        serializedTransformSequenceItems.DeleteArrayElementAtIndex(seqIterator);
                        break;
                    }
                }

                for (int seqIterator = 0; seqIterator < eventSequenceItemIndexes.arraySize; seqIterator++)
                {
                    if (eventSequenceItemIndexes.GetArrayElementAtIndex(seqIterator).intValue == index)
                    {
                        eventSequenceItemIndexes.DeleteArrayElementAtIndex(seqIterator);
                        serializedEventSequenceItems.DeleteArrayElementAtIndex(seqIterator);
                        break;
                    }
                }

                for (int seqIterator = 0; seqIterator < waitSequenceItemIndexes.arraySize; seqIterator++)
                {
                    if (waitSequenceItemIndexes.GetArrayElementAtIndex(seqIterator).intValue == index)
                    {
                        waitSequenceItemIndexes.DeleteArrayElementAtIndex(seqIterator);
                        serializedWaitSequenceItems.DeleteArrayElementAtIndex(seqIterator);
                        break;
                    }
                }
                #endregion
            }

            var newObject = Activator.CreateInstance(SequenceEnumAlloc.TypeToClassDictionary[newType]);
            #region create new objects
            if (newObject is TransformCurveSequenceItem)
            {
            
                for (int i = 0; i < transformSequenceItemIndexes.arraySize; i++)
                {
                    var intRef = transformSequenceItemIndexes.GetArrayElementAtIndex(i);
            
                    if (intRef.intValue == index)
                    {
                        return;
                    }
                }
                int size = transformSequenceItemIndexes.arraySize;
                transformSequenceItemIndexes.InsertArrayElementAtIndex(size);
                serializedTransformSequenceItems.InsertArrayElementAtIndex(size);
            
                transformSequenceItemIndexes.GetArrayElementAtIndex(size).intValue = index; 
                TransformCurveSequenceItem.DefaultInitialize(targetObject, script, serializedTransformSequenceItems.GetArrayElementAtIndex(size), newType);
            }
            
            if (newObject is EventSequenceItem)
            {
            
                for (int i = 0; i < eventSequenceItemIndexes.arraySize; i++)
                {
                    var intRef = eventSequenceItemIndexes.GetArrayElementAtIndex(i);
            
                    if (intRef.intValue == index)
                    {
                        return;
                    }
                }
                int size = eventSequenceItemIndexes.arraySize;
                eventSequenceItemIndexes.InsertArrayElementAtIndex(size);
                serializedEventSequenceItems.InsertArrayElementAtIndex(size);
            
                eventSequenceItemIndexes.GetArrayElementAtIndex(size).intValue = index;
                EventSequenceItem.DefaultInitialize(serializedEventSequenceItems.GetArrayElementAtIndex(size), newType);
            }
            
            if (newObject is WaitSequenceItem)
            {
            
                for (int i = 0; i < waitSequenceItemIndexes.arraySize; i++)
                {
                    var intRef = waitSequenceItemIndexes.GetArrayElementAtIndex(i);
            
                    if (intRef.intValue == index)
                    {
                        return;
                    }
                }
                int size = waitSequenceItemIndexes.arraySize;
                waitSequenceItemIndexes.InsertArrayElementAtIndex(size);
                serializedWaitSequenceItems.InsertArrayElementAtIndex(size);
            
                waitSequenceItemIndexes.GetArrayElementAtIndex(size).intValue = index;
                WaitSequenceItem.DefaultInitialize(serializedWaitSequenceItems.GetArrayElementAtIndex(size), newType);
            }
            #endregion
        }
    }
}
