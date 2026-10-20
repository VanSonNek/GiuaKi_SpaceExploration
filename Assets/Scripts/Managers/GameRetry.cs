using UnityEngine;
using UnityEngine.SceneManagement;

public class GameRetry : MonoBehaviour
{
    private bool isReloading;

    public void PlayAgain()
    {
        if (!Application.isPlaying || isReloading)
        {
            return;
        }

        isReloading = true;
        Time.timeScale = 1f;

        // Scene mới tự khởi tạo lại HP, điểm, wave và Pause qua các manager hiện có.
        SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }
}
