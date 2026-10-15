using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChestManager : MonoBehaviour
{
    public List<Button> UpButtons = new List<Button>();
    public List<Button> DownButtons = new List<Button>();
    public List<TextMeshPro> NumIndicator = new List<TextMeshPro>();
    public List<int> inputValues = new List<int>() { 0, 0, 0, 0 };

    [HideInInspector] public static ChestManager Instance;
    [HideInInspector] public List<GameObject> completedChests;
    [HideInInspector] public ChestInteractable currentChest;
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
    }

    public void DecreaseValue(int index) 
    {
        inputValues[index] -= index;
        if (inputValues[index] < 0)
        {
            inputValues[index] = 9;
        }
    }
    public void IncreaseValue(int index) 
    {
        inputValues[index] += index;
        if (inputValues[index] > 9)
        {
            inputValues[index] = 0;
        }
    }
}
