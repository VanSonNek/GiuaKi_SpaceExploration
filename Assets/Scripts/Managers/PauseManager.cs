using UnityEngine;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    public Image pauseButtonImage;
    public Sprite pauseSprite;
    public Sprite resumeSprite;

    public bool IsPaused { get; private set; }

    private bool isPauseLocked;

    void Start()
    {
        SetPaused(false);
    }

    void Update()
    {
        // Update vẫn chạy khi timeScale = 0, nên Escape có thể tiếp tục game.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (!isActiveAndEnabled || isPauseLocked)
        {
            return;
        }

        SetPaused(!IsPaused);
    }

    public void LockPause()
    {
        SetPaused(true);
        isPauseLocked = true;
    }

    void SetPaused(bool paused)
    {
        // Victory/Game Over phải giữ game dừng, kể cả khi component bị tắt/hủy.
        if (isPauseLocked)
        {
            Time.timeScale = 0f;
            return;
        }

        IsPaused = paused;
        Time.timeScale = IsPaused ? 0f : 1f;

        if (pauseButtonImage == null || pauseSprite == null || resumeSprite == null)
        {
            Debug.LogWarning("Hãy gán Pause Button Image, Pause Sprite và Resume Sprite cho PauseManager.", this);
        }

        Sprite buttonSprite = IsPaused ? resumeSprite : pauseSprite;

        // Thiếu reference vẫn pause/resume an toàn, chỉ bỏ qua đổi ảnh.
        if (pauseButtonImage != null && buttonSprite != null)
        {
            pauseButtonImage.sprite = buttonSprite;
        }
    }

    void OnDisable()
    {
        SetPaused(false);
    }

    void OnDestroy()
    {
        SetPaused(false);
    }
}
