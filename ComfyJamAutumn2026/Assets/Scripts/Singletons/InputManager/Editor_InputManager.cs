using Sequences;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

[CustomEditor(typeof(InputManager))]

public class Editor_InputManager : Editor
{
    SerializedProperty gamestateListProp;
    SerializedProperty gamestateInputActionsProp;
    SerializedProperty mappedActionsSizeProp;

    List<bool> isFoldedOut = new List<bool>();
    private void OnEnable()
    {
        gamestateListProp = serializedObject.FindProperty("_gameStates");
        gamestateInputActionsProp = serializedObject.FindProperty("_boundMaps");
        mappedActionsSizeProp = serializedObject.FindProperty("_allocatedMapSize");

        isFoldedOut = new List<bool>(gamestateListProp.arraySize);
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Gamestate bound action maps");
        InputSystem_Actions tempactions = new();
        
        List<InputActionMap> maps = tempactions.asset.actionMaps.ToList();
        int countToIndex = 0;

        if(GUILayout.Button("Clear All")){
            Clear();
        }
        if (GUILayout.Button("Add New Gamestate Map"))
        {
            AddNew(gamestateListProp.arraySize);
        }

        List<string> seenStates = new List<string>();
        bool showContainedWarning = false;

        for (int i = 0; i < gamestateListProp.arraySize; i++) 
        {
            SerializedProperty gamestateProp = gamestateListProp.GetArrayElementAtIndex(i);

            string name = ((IGameState)gamestateProp.objectReferenceValue)?.name;

            if(seenStates.Contains(name))
                showContainedWarning = true;

            if (name != null)
                seenStates.Add(name);

            if (name == null)
                name = "None";

            while (isFoldedOut.Count <= i)
            {
                isFoldedOut.Add(false);
            }
            isFoldedOut[i] = EditorGUILayout.Foldout(isFoldedOut[i], $"{i} - Gamestate: {name}");

            if(!isFoldedOut[i] )
                continue;

            SerializedProperty mappedSizeProp = mappedActionsSizeProp.GetArrayElementAtIndex(i);

            EditorGUILayout.PropertyField(gamestateProp, new GUIContent("Gamestate Object"));

            List<string> enalbedActions = new List<string>();

            EditorGUI.indentLevel++;
            EditorGUILayout.LabelField("Maps (Toggle True if Enabled during Gamestate)");
            for(int mappedActionsIterator = 0; mappedActionsIterator < mappedSizeProp.intValue; mappedActionsIterator++)
            {
                enalbedActions.Add(gamestateInputActionsProp.GetArrayElementAtIndex(countToIndex + mappedActionsIterator).stringValue);
            }

            Dictionary<string, bool> selectedActions = new Dictionary<string, bool>();

            for (int mapIterator = 0; mapIterator < maps.Count; mapIterator++) 
            {
                string currentMapName = maps[mapIterator].name;
                selectedActions.Add(currentMapName, false);

                if (enalbedActions.Contains(maps[mapIterator].name))
                {
                    selectedActions[currentMapName] = true;
                }

                selectedActions[currentMapName] = EditorGUILayout.Toggle($"Input Map: {currentMapName}", selectedActions[currentMapName]);
            }


            for (int deletionIterator = 0; deletionIterator < mappedSizeProp.intValue; deletionIterator++)
            {
                gamestateInputActionsProp.DeleteArrayElementAtIndex(countToIndex);
            }

            gamestateInputActionsProp.serializedObject.ApplyModifiedProperties();

            mappedSizeProp.intValue = 0;
            //mappedSizeProp.serializedObject.ApplyModifiedProperties();

            foreach (KeyValuePair<string, bool> pair in selectedActions)
            {
                if (pair.Value)
                {
                    ++mappedSizeProp.intValue;
                    mappedSizeProp.serializedObject.ApplyModifiedProperties();

                    gamestateInputActionsProp.InsertArrayElementAtIndex(countToIndex);
                    gamestateInputActionsProp.serializedObject.ApplyModifiedProperties();

                    gamestateInputActionsProp.GetArrayElementAtIndex(countToIndex).stringValue = pair.Key;
                }
            }

            countToIndex += mappedSizeProp.intValue;

            if (GUILayout.Button($"Delete {name} Mapping"))
            {
                Delete(i);
            }

            EditorGUI.indentLevel--;
        }

        if(showContainedWarning)
            EditorGUILayout.HelpBox("Some Game states are contained twice! The latter will not be bound at runtime.", MessageType.Warning);

        serializedObject.ApplyModifiedProperties();
    }

    public void AddNew(int index)
    {
        gamestateListProp.InsertArrayElementAtIndex(index);
        gamestateListProp.GetArrayElementAtIndex(index).objectReferenceValue = null;
        gamestateListProp.serializedObject.ApplyModifiedProperties();

        isFoldedOut.Insert(index, true);
        mappedActionsSizeProp.InsertArrayElementAtIndex(index);
        mappedActionsSizeProp.GetArrayElementAtIndex(index).intValue = 0;
        mappedActionsSizeProp.serializedObject.ApplyModifiedProperties();
    }

    public void Delete(int index) 
    {
        int countToIndex = 0;
        int countToDelete = mappedActionsSizeProp.GetArrayElementAtIndex(index).intValue;

        for (int i = 0; i < index; i++) {
            countToIndex += mappedActionsSizeProp.GetArrayElementAtIndex(i).intValue;
        }

        for(int i = 0; i < countToDelete; i++)
        {
            gamestateInputActionsProp.DeleteArrayElementAtIndex(i);
        }

        mappedActionsSizeProp.DeleteArrayElementAtIndex(index);
        gamestateListProp.DeleteArrayElementAtIndex(index);
        isFoldedOut.RemoveAt(index);
    }

    public void Clear()
    {
        gamestateListProp.ClearArray();
        gamestateInputActionsProp.ClearArray();
        mappedActionsSizeProp.ClearArray();
        isFoldedOut.Clear();
    }
}
