using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DebugHotKeysListener : MonoBehaviour
{
#if UNITY_EDITOR
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            ReloadScene();
        }
    }

    private void ReloadScene()
    {
        DOTween.KillAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
#endif
}