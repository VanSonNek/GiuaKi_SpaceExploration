using UnityEngine;
using UnityEngine.SceneManagement;

// [BAO VE] Callback NEXT LEVEL; nextSceneName được cấu hình theo Scene và phải có trong Build Settings.
public class LevelNavigation : MonoBehaviour
{
    [SerializeField]
    private string nextSceneName;

    private bool isLoading;

    // [BAO VE] Kiểm tra Scene đích, chặn tải lặp, trả timeScale về 1 rồi LoadScene Single để thay toàn bộ Scene cũ.
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
        // [BAO VE - CHUYEN LEVEL] Đây là lệnh NEXT LEVEL tải Scene đích; nextSceneName được gán trong Inspector.
        SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
    }
}
