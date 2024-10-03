using UnityEngine;

public class ApplicationExit : MonoBehaviour
{
    public void Exit()
    {
#if UNITY_EDITOR
        Debug.Log("Quit Application");
#else
        Application.Quit();
#endif
    }
}