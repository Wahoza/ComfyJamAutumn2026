using UnityEngine;
using TMPro;

public class DialogueWindow : MonoBehaviour
{
    public static DialogueWindow Instance { get; private set; }

    [Header("References")]
    [SerializeField] private RectTransform bubble;   // the panel (pivot at bottom-center)
    [SerializeField] private TMP_Text text;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        //DontDestroyOnLoad only works on root objects, so the canvas the window sits on is kept too
        DontDestroyOnLoad(transform.root.gameObject);
        bubble.gameObject.SetActive(false);
    }
    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
    public void Show(Transform speaker)
    {
        //every room has its own camera, so look it up each time instead of keeping the first room's
        bubble.transform.position = Camera.main.WorldToScreenPoint(speaker.position);
        bubble.gameObject.SetActive(true);
    }
    public void NextLine(string newLine) {
        text.text = newLine;
    }
    public void Close() {
        bubble.gameObject.SetActive(false);
    }
}
