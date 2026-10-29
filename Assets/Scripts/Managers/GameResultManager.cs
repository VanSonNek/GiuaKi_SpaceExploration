using UnityEngine;

// [BAO VE] Quản lý kết quả một lần; Player.Died gọi ShowGameOver, Level1Controller gọi ShowVictory sau khi Boss bị hủy.
public class GameResultManager : MonoBehaviour
{
    public GameObject victoryPanel;
    public GameObject gameOverPanel;
    public PauseManager pauseManager;
    public PlayerHealth playerHealth;

    public bool IsGameFinished { get; private set; }

    // [BAO VE] Nghe sự kiện Player chết để bật Game Over; OnDisable hủy đăng ký để tránh giữ listener cũ.
    void OnEnable()
    {
        if (playerHealth != null)
        {
            // [BAO VE - DANG KY GAME OVER] Gắn callback ShowGameOver với sự kiện Player.Died; PlayerHealth là nơi phát sự kiện khi HP hết.
            playerHealth.Died += ShowGameOver;
        }
    }

    void Start()
    {
        IsGameFinished = false;

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(false);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        if (victoryPanel == null || gameOverPanel == null || pauseManager == null || playerHealth == null)
        {
            Debug.LogWarning("Hãy gán Victory Panel, Game Over Panel, Pause Manager và Player Health.", this);
        }
    }

    // [BAO VE] Điểm vào cho chiến thắng, chuyển về xử lý chung FinishGame(true).
    public void ShowVictory()
    {
        FinishGame(true);
    }

    // [BAO VE] Điểm vào cho thất bại do Player.Died, chuyển về FinishGame(false).
    public void ShowGameOver()
    {
        FinishGame(false);
    }

    // [BAO VE] IsGameFinished chặn kết quả lặp; khóa Pause, bật đúng một panel thắng/thua và đặt timeScale = 0.
    void FinishGame(bool victory)
    {
        if (IsGameFinished)
        {
            return;
        }

        IsGameFinished = true;

        // Khóa Pause trước khi đổi panel để các callback UI không mở khóa thời gian.
        if (pauseManager != null)
        {
            pauseManager.LockPause();
        }

        if (victoryPanel != null)
        {
            // [BAO VE - HIEN VICTORY PANEL] victory = true bật màn thắng; gameOverPanel bên dưới dùng !victory nên không bật đồng thời.
            victoryPanel.SetActive(victory);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(!victory);
        }

        // [BAO VE - KET QUA DUNG GAME] Dừng thời gian sau khi hiện kết quả; Retry hoặc chuyển Scene phải đặt lại timeScale = 1.
        Time.timeScale = 0f;
    }

    void OnDisable()
    {
        if (playerHealth != null)
        {
            playerHealth.Died -= ShowGameOver;
        }
    }
}
