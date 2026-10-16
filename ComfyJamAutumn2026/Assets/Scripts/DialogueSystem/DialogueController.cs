using System.Collections.Generic;
using UnityEngine;


//Script that controlls dialogue of a npc
public class DialogueController : MonoBehaviour, IInteractable
{
    [SerializeField] DialogueProfileSO profile;

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

    //Player should call this ontriggerenter2d when near an object tagged dialogue or smth and if correct key pressed
    public void Interact()
    {
        //the window is kept from the scene the game started in, without it the player would get stuck in the dialogue state
        if (DialogueWindow.Instance == null)
        {
            Debug.LogError("[DialogueController] No DialogueWindow loaded. Put the dialogue canvas in the scene the game starts in.", this);
            return;
        }
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
        DialogueWindow.Instance.NextLine(currentConversation.sentences[currentSentenceIndex++]);
    }
    //firstly gets the conversation it should start displaying, then if it is locked it returns to the last conversation had
    void StartConversation() {
        GameStateManager.Instance.RequestGameStateSwitch(GameStateManager.Instance.GetGameStateByType(GameStateType.Dialogue));
        currentSentenceIndex = 0;
        currentConversation = profile.Conversations[currentConversationIndex];
        if (currentConversation.requirementId!=-1 && Blackboard.Instance.TryGetReqDict(currentConversation.requirementId)==false) {
            currentConversation = profile.Conversations[--currentConversationIndex];
        }
        DialogueWindow.Instance.Show(transform);
    }
    //closes the window and increments the conversationIndex
    void EndConversation() {
        currentConversationIndex = Mathf.Min(currentConversationIndex + 1, profile.Conversations.Count - 1);
        Blackboard.Instance.UpdateDialogueDict(profile.Key, currentConversationIndex);
        currentConversation = null;
        DialogueWindow.Instance.Close();
        GameStateManager.Instance.RequestGameStateSwitch(GameStateManager.Instance.GetGameStateByType(GameStateType.Default));
    }
}
