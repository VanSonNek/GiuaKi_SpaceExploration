using TMPro;
using UnityEngine;

// [BAO VE] Hiển thị điểm bằng TMP_Text thông qua sự kiện ScoreManager.ScoreChanged, không cần polling trong Update.
public class ScoreUI : MonoBehaviour
{
    public ScoreManager scoreManager;
    public TMP_Text scoreText;

    private ScoreManager subscribedManager;

    // [BAO VE] Đăng ký nhận thay đổi điểm và hiển thị ngay CurrentScore; OnDisable gỡ đăng ký.
    void OnEnable()
    {
        if (scoreManager == null || scoreText == null)
        {
            Debug.LogWarning("Hãy gán Score Manager và Score Text cho ScoreUI.", this);
            return;
        }

        subscribedManager = scoreManager;
        // [BAO VE - CAP NHAT SCORE UI] Đăng ký hàm đổi chữ điểm; OnDisable gỡ lại bằng -= để không giữ UI cũ.
        subscribedManager.ScoreChanged += UpdateScoreText;

        // Hiển thị ngay điểm hiện tại, kể cả khi UI được bật lại.
        UpdateScoreText(subscribedManager.CurrentScore);
    }

    // [BAO VE] Callback đổi chữ thành SCORE kèm số điểm vừa được manager phát ra.
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
