using TMPro;
using UnityEngine;

public class PageComponent : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI pageIndicator;
    
    public void SetPageIndicator(string text)
    {
        pageIndicator.text = text;
    }

    public virtual void Initalize()
    {

    }
}
