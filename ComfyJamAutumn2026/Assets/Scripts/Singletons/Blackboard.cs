using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Blackboard : MonoBehaviour
{
    public static Blackboard Instance { get; private set; }

    Dictionary<int, int> dialogueProgressionDict= new Dictionary<int, int>(); //what conversation should each npc start with
    Dictionary<int, bool> requirementDict = new Dictionary<int, bool>(); //checks if conversation is unlocked (quest complete)

    Dictionary<string, bool> environmentChangeFromJournalDict = new Dictionary<string, bool>(); //checks for dragged and properly set words in journal
    [HideInInspector] public UnityEvent onJournalEnvironmentUpdate = new(); //called when Journal dictionary changes

    Dictionary<string, bool> photographDict = new Dictionary<string, bool>(); //checks for photographed items

    #region SignletonHandling
    private void Awake()
    {
        Initailize();
    }
    private void Initailize()
    {
        if (Instance == null)
        {
            Instance = this;

            DontDestroyOnLoad(Instance);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void OnApplicationQuit()
    {
        if (Instance != null)
            Destroy(Instance);
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

    #region RequirementFromJournalHandling

    public bool GetIsUnlockedFromJournalDict(string key)
    {
        return environmentChangeFromJournalDict.ContainsKey(key) ? environmentChangeFromJournalDict[key] : false;
    }
    public void SetJournalDictBool(string key, bool value)
    {
        if(!environmentChangeFromJournalDict.ContainsKey(key))
            environmentChangeFromJournalDict.Add(key, value);

        environmentChangeFromJournalDict[key] = value;

        onJournalEnvironmentUpdate.Invoke();
    }

    public void SetPhotographDictValue(string key, bool value)
    {
        if (!photographDict.ContainsKey(key))
            photographDict.Add(key, value);

        else photographDict[key] = value;
    }

    public bool GetPhotographDictValue(string key, bool value)
    {
        return photographDict.ContainsKey(key) ? photographDict[key] : false;

    }
    #endregion
}
