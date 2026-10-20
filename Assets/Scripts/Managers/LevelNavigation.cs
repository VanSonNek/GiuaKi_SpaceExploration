using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelNavigation : MonoBehaviour
{
    [SerializeField]
    private string nextSceneName;

    private bool isLoading;

    public void LoadNextLevel()
    {
        if (!Application.isPlaying || isLoading || string.IsNullOrEmpty(nextSceneName))
        {
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(nextSceneName))
        {
            Debug.LogWarning("Scene tiếp theo chưa có hoặc chưa được bật trong Build Settings: " + nextSceneName, this);
            return;
        }

        isLoading = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
    }
}
