using System;
using UnityEngine;

// [BAO VE] Một Instance trong Scene giữ CurrentScore; Enemy/Asteroid/Boss gọi AddScore, UI nghe ScoreChanged.
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    public int CurrentScore { get; private set; }

    public event Action<int> ScoreChanged;

    // [BAO VE] Chống manager trùng trong Scene và khởi tạo điểm bằng 0; không dùng DontDestroyOnLoad để giữ điểm qua Scene.
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Chỉ cần một ScoreManager trong Scene.", this);
            Destroy(this);
            return;
        }

        Instance = this;
        CurrentScore = 0;
    }

    // [BAO VE] Chỉ nhận điểm dương, cộng CurrentScore và phát ScoreChanged để cập nhật chữ điểm.
    public void AddScore(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        // [BAO VE - CONG SCORE] Đây là dòng tăng điểm thật; Enemy, Asteroid và Boss chỉ gửi lượng điểm qua AddScore.
        CurrentScore += amount;
        // [BAO VE - SU KIEN SCORE] Báo tổng điểm mới cho ScoreUI; UI cập nhật khi có sự kiện thay vì đọc điểm mỗi frame.
        ScoreChanged?.Invoke(CurrentScore);
    }

    // [BAO VE] Xóa Instance nếu chính manager này bị hủy để Scene sau có thể khởi tạo manager mới.
    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
