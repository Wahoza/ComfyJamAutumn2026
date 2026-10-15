using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ChestManager : MonoBehaviour
{
    public List<TextMeshProUGUI> numIndicator = new List<TextMeshProUGUI>();
    [HideInInspector] public List<int> inputValues = new List<int>() { 0, 0, 0, 0 };

    [HideInInspector] public static ChestManager Instance;
    [HideInInspector] public List<GameObject> completedChests;
    [HideInInspector] public ChestInteractable currentChest;
    [SerializeField] IGameState chestGameState;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            completedChests = new List<GameObject>();
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }
    private void Start()
    {
        InputManager.InputActions.UI.QuitChest.performed += QuitChestInteraction;
    }
    private void Update()
    {
        Debug.Log(GameStateManager.Instance.GetGameState().name);
    }
    public bool GetIsCompleted(GameObject chest)
    {
        return completedChests.Contains(chest);
    }
    public void SetIsCompleted(GameObject chest)
    {
        completedChests.Add(chest);
    }
    public void OnSubmit()
    {
        int val = 0;
        for (int i = 0; i < inputValues.Count; i++) 
        {
            val += inputValues[i] * (int)Mathf.Pow(10, inputValues.Count - i);
        }
        currentChest.CheckForCode(0);
    }
    public void OnChestStart(ChestInteractable chest)
    {
        currentChest = chest;
        inputValues = new List<int>() { 0, 0, 0, 0 };

        GameStateManager.Instance.RequestGameStateSwitch(chestGameState);

        UpdateNums();
    }

    private void UpdateNums()
    {
        for (int i = 0;i < inputValues.Count; i++)
        {
            numIndicator[i].text = inputValues[i].ToString();
        }
    }
    public void QuitChestInteraction(InputAction.CallbackContext callbackContext)
    {
        Debug.Log("BOOB");
        if (GameStateManager.Instance.GetGameState() == chestGameState)
            GameStateManager.Instance.RequestGameStateSwitchPrevious();
    }
    public void DecreaseValue(int index) 
    {
        inputValues[index] -= 1;
        if (inputValues[index] < 0)
        {
            inputValues[index] = 9;
        }

        UpdateNums();
    }
    public void IncreaseValue(int index) 
    {
        inputValues[index] += 1;
        if (inputValues[index] > 9)
        {
            inputValues[index] = 0;
        }

        UpdateNums();
    }
}
