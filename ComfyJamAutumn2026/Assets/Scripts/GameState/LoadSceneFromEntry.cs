using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneFromEntry : MonoBehaviour
{
    [SerializeField] string sceneToLoad;
    void Awake()
    {
        StartCoroutine(DoDelayedLoad());
    }

    IEnumerator DoDelayedLoad()
    {
        yield return null;

        SceneManager.LoadScene(sceneToLoad);

    }
}
