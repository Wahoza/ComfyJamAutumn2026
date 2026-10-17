using System.Collections.Generic;
using UnityEngine;


/*
 You can have multiple interactions with a npc. One interaction is one conversation.
 For each sentence we update the text window display, deleting old sentence and showing new
 */
[System.Serializable]
public class Conversation {
    public int requirementId; //check blackboard dict to see if task is completed and show new dialogue. -1==auto show new dialogue
    [Tooltip("Also unlocked once the player has photographed any of these (snapshot names, like Egg). Leave empty if only the requirement unlocks it.")]
    public List<string> unlockedByPhotos;
    public List<string> sentences;
}

[CreateAssetMenu(fileName = "New DialogueProfileSO", menuName = "Scriptable Objects/DialogueProfile")]
public class DialogueProfileSO : ScriptableObject
{
    [SerializeField] int key; //check blackboardDict to see what conversation next to say
    [SerializeField] List<Conversation> conversations;
    public List<Conversation> Conversations { get => conversations;}
    public int Key { get => key;}
}
