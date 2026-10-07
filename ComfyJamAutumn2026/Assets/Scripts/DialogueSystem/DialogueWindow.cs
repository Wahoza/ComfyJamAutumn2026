using UnityEngine;
using TMPro;

public class DialogueWindow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform bubble;   // the panel (pivot at bottom-center)
    [SerializeField] private TMP_Text text;

    private Camera cam;
    void Awake()
    {
        cam = Camera.main;
        bubble.gameObject.SetActive(false);
    }
    public void Show(Transform speaker)
    {
        bubble.transform.position = cam.WorldToScreenPoint(speaker.position);
        bubble.gameObject.SetActive(true);
    }
    public void NextLine(string newLine) { 
        text.text = newLine;
    }
    public void Close() {
        bubble.gameObject.SetActive(false);
    }
}
