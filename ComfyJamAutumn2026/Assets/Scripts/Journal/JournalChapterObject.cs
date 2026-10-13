using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using UnityEngine;


[CreateAssetMenu(fileName = "JournalObject", menuName = "Scriptable Objects/JournalObject")]

public class JournalChapterObject : ScriptableObject
{
    [SerializedDictionary("Page Prefab", "Condition To Be Shown")]
    [SerializeField] public SerializedDictionary<GameObject, string> filledPagePrefabCondition;
}
