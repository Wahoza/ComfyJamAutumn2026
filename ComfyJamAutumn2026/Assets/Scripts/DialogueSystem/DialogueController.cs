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
        //the window is loaded from Resources when the game starts, without it the player would get stuck in the dialogue state
        if (DialogueWindow.Instance == null)
        {
            Debug.LogError("[DialogueController] No DialogueWindow loaded. Is the DialogueCanvas prefab still in a Resources folder?", this);
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
        //a room played on its own only knows the Default state, talking still works there but the player isn't held in place
        if (GameStateManager.Instance.TryGetGameStateByType(GameStateType.Dialogue, out IGameState dialogueState))
            GameStateManager.Instance.RequestGameStateSwitch(dialogueState);
        currentSentenceIndex = 0;
        currentConversation = profile.Conversations[currentConversationIndex];
        if (!IsUnlocked(currentConversation)) {
            currentConversation = profile.Conversations[--currentConversationIndex];
        }
        DialogueWindow.Instance.Show(transform);
    }
    //a conversation is unlocked by its requirement or by any of its photos being in the journal
    bool IsUnlocked(Conversation conversation) {
        if (conversation.requirementId == -1 || Blackboard.Instance.TryGetReqDict(conversation.requirementId))
            return true;
        if (conversation.unlockedByPhotos != null) {
            foreach (string photo in conversation.unlockedByPhotos) {
                if (Blackboard.Instance.GetPhotographDictValue(photo))
                    return true;
            }
        }
        return false;
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
