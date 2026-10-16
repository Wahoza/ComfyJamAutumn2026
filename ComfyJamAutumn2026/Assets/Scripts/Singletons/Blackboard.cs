using System.Collections.Generic;
using UnityEngine;

public class Blackboard : MonoBehaviour
{
    private static Blackboard m_instance;

    /// <summary>
    /// Instance of Blackboard. Created when first needed if no scene has one, so dialogue works in every room.
    /// </summary>
    public static Blackboard Instance
    {
        get
        {
            if (m_instance == null)
                new GameObject("Blackboard").AddComponent<Blackboard>();
            return m_instance;
        }
    }

    Dictionary<int, int> dialogueProgressionDict= new Dictionary<int, int>(); //what conversation should each npc start with
    Dictionary<int, bool> requirementDict = new Dictionary<int, bool>(); //checks if conversation is unlocked (quest complete)

    #region SignletonHandling
    private void Awake()
    {
        Initailize();
    }
    private void Initailize()
    {
        if (m_instance == null)
        {
            m_instance = this;

            DontDestroyOnLoad(m_instance);
        }
    }
    private void OnApplicationQuit()
    {
        if (m_instance != null)
            Destroy(m_instance);
    }
    #endregion

    #region DialogueDictionaryHandling
    public void InnitDialogueDict(int key) {
        if(!dialogueProgressionDict.ContainsKey(key))
            dialogueProgressionDict.Add(key, 0);
    }
    public void UpdateDialogueDict(int key, int value)
    {
        if (dialogueProgressionDict.ContainsKey(key))
            dialogueProgressionDict[key] = value;
    }
    public int TryGetDialogueDict(int key) {
       return dialogueProgressionDict.TryGetValue(key, out int value) ? value : -1; 
    }

    #endregion

    #region RequirementDictionaryHandling
    public void InnitReqDict(int key) {
        if (!requirementDict.ContainsKey(key))
            requirementDict.Add(key, false);
    }
    public void UpdateReqDict(int key, bool value)
    {
        if (requirementDict.ContainsKey(key))
            requirementDict[key] = value;
    }
    public bool TryGetReqDict(int key)
    {
        return requirementDict.TryGetValue(key, out bool value) && value;
    }

    #endregion
}
