using UnityEngine;
using UnityEngine.SceneManagement;

// [BAO VE] Callback dùng chung cho nút Play Again trong các Scene gameplay.
public class GameRetry : MonoBehaviour
{
    private bool isReloading;

    // [BAO VE] Chặn bấm lặp, trả timeScale về 1 rồi LoadScene Single theo tên Scene hiện tại; HP, điểm, súng và Wave khởi tạo lại từ Scene mới.
    public void PlayAgain()
    {
        if (!Application.isPlaying || isReloading)
        {
            return;
        }

        isReloading = true;
        Time.timeScale = 1f;

        // Scene mới tự khởi tạo lại HP, điểm, wave và Pause qua các manager hiện có.
        // [BAO VE - RETRY LOAD SCENE] Đây là lệnh tải lại Scene hiện tại; Scene mới tạo Player/Manager mới nên không cần tự gán lại từng HP, Score và cấp súng.
        SceneManager.LoadScene(SceneManager.GetActiveScene().name, LoadSceneMode.Single);
    }
}
