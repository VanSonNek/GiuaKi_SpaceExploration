using System;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    public int CurrentScore { get; private set; }

    public event Action<int> ScoreChanged;

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

    public void AddScore(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        CurrentScore += amount;
        ScoreChanged?.Invoke(CurrentScore);
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}
