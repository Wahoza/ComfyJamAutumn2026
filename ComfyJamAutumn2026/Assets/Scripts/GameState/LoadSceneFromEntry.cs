using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneFromEntry : MonoBehaviour
{
    [SerializeField] string sceneToLoad;
    void Awake()
    {
        SceneManager.LoadScene(sceneToLoad);
    }
}
