using UnityEngine;

public class GameResultManager : MonoBehaviour
{
    public GameObject victoryPanel;
    public GameObject gameOverPanel;
    public PauseManager pauseManager;
    public PlayerHealth playerHealth;

    public bool IsGameFinished { get; private set; }

    void OnEnable()
    {
        if (playerHealth != null)
        {
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

    public void ShowVictory()
    {
        FinishGame(true);
    }

    public void ShowGameOver()
    {
        FinishGame(false);
    }

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
            victoryPanel.SetActive(victory);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(!victory);
        }

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
