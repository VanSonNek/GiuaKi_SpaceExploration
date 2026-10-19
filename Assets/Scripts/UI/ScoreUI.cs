using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    public ScoreManager scoreManager;
    public TMP_Text scoreText;

    private ScoreManager subscribedManager;

    void OnEnable()
    {
        if (scoreManager == null || scoreText == null)
        {
            Debug.LogWarning("Hãy gán Score Manager và Score Text cho ScoreUI.", this);
            return;
        }

        subscribedManager = scoreManager;
        subscribedManager.ScoreChanged += UpdateScoreText;

        // Hiển thị ngay điểm hiện tại, kể cả khi UI được bật lại.
        UpdateScoreText(subscribedManager.CurrentScore);
    }

    void UpdateScoreText(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = "SCORE: " + score;
        }
    }

    void OnDisable()
    {
        if (subscribedManager != null)
        {
            subscribedManager.ScoreChanged -= UpdateScoreText;
        }

        subscribedManager = null;
    }
}
