using System.Collections.Generic;
using UnityEngine;


//Script that controlls dialogue of a npc
public class DialogueController : MonoBehaviour
{
    [SerializeField] DialogueProfileSO profile;
    [SerializeField] DialogueWindow window;

    Conversation currentConversation;

    int currentConversationIndex;
    int currentSentenceIndex;

    //when scene starts we initialize blackboard if its empty and read the value it stores
    private void Start()
    {
        Blackboard.Instance.InnitDialogueDict(profile.Key);
        currentConversationIndex = Blackboard.Instance.TryGetDialogueDict(profile.Key);
        foreach (Conversation convo in profile.Conversations) {
            if(convo.requirementId!=-1)
            Blackboard.Instance.InnitReqDict(convo.requirementId);
        }
    }

    //debugging purposes
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            Interract();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Blackboard.Instance.UpdateReqDict(0, true);
        }
    }

    //Player should call this ontriggerenter2d when near an object tagged dialogue or smth and if correct key pressed
    public void Interract()
    {
        if (currentConversation == null)
        {
            StartConversation();
        }
        if (currentSentenceIndex >= currentConversation.sentences.Count) { 
            EndConversation();
            return;
        }
        ShowDialogue();
    }
    //display the correct sentence
    void ShowDialogue() {
        window.NextLine(currentConversation.sentences[currentSentenceIndex++]);
    }
    //firstly gets the conversation it should start displaying, then if it is locked it returns to the last conversation had
    void StartConversation() {
        currentSentenceIndex = 0;
        currentConversation = profile.Conversations[currentConversationIndex];
        if (currentConversation.requirementId!=-1 && Blackboard.Instance.TryGetReqDict(currentConversation.requirementId)==false) {
            currentConversation = profile.Conversations[--currentConversationIndex];
        }
        window.Show(transform);
    }
    //closes the window and increments the conversationIndex
    void EndConversation() {
        currentConversationIndex = Mathf.Min(currentConversationIndex + 1, profile.Conversations.Count - 1);
        Blackboard.Instance.UpdateDialogueDict(profile.Key, currentConversationIndex);
        currentConversation = null;
        window.Close();
    }
}
