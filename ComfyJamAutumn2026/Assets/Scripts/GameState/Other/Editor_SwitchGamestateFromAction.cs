using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(SwitchGamestateFromAction))]
public class Editor_SwitchGamestateFromAction : Editor
{
    SerializedProperty enabledStatesProp;
    SerializedProperty actionProp;
    SerializedProperty mapProp;
    SerializedProperty gameStateProp;

    private InputSystem_Actions actions;

    void OnEnable()
    {
        actionProp = serializedObject.FindProperty("_sActionName");
        enabledStatesProp = serializedObject.FindProperty("_enabledGameStates");
        mapProp = serializedObject.FindProperty("_sMapName");
        gameStateProp = serializedObject.FindProperty("_targetGameState");

        actions = new();
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(enabledStatesProp);
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(gameStateProp);

        var options = actions.asset.actionMaps.ToList();
        var optionsMapAsString = new List<string>();

        foreach (var map in options)
        {
            optionsMapAsString.Add(map.name);
        }

        int indexMap = optionsMapAsString.IndexOf(mapProp.stringValue);

        if(indexMap >= optionsMapAsString.Count || indexMap < 0)
            indexMap = 0;

        indexMap = EditorGUILayout.Popup(indexMap, optionsMapAsString.ToArray());

        mapProp.stringValue = optionsMapAsString[indexMap];



        var optionsBindingAsString = new List<string>();

        var selectedMap = actions.asset.FindActionMap(optionsMapAsString[indexMap]);

        foreach(var bindingOption in selectedMap.actions)
        {
            optionsBindingAsString.Add(bindingOption.name);
        }

        int indexBinding = optionsBindingAsString.IndexOf(actionProp.stringValue);
        if (indexBinding >= optionsBindingAsString.Count || indexBinding < 0)
            indexBinding = 0;

        indexBinding = EditorGUILayout.Popup(indexBinding, optionsBindingAsString.ToArray());

        actionProp.stringValue = optionsBindingAsString[indexBinding];


        serializedObject.ApplyModifiedProperties();
    }
}
