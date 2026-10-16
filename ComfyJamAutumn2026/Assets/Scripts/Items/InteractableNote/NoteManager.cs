using TMPro;
using UnityEngine;

public class NoteManager : MonoBehaviour
{
    public static NoteManager Instance {get; private set;}

    [SerializeField] TextMeshProUGUI textField;
    [SerializeField] IGameState noteGameState;
    private void Awake()
    {
        if(Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }
    private void Start()
    {
        InputManager.InputActions.UI.QuitChest.performed += QuitNoteInteraction;
    }
    public void CloseNote()
    {
        GameStateManager.Instance.RequestGameStateSwitchPrevious();
    }
    private void QuitNoteInteraction(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        CloseNote();
    }

    public void OnNoteInteract(string noteContent)
    {
        GameStateManager.Instance.RequestGameStateSwitch(noteGameState);
        textField.text = noteContent;
    }
}


