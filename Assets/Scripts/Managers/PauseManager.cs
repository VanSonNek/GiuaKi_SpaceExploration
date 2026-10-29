using UnityEngine;
using UnityEngine.UI;

// [BAO VE] Quản lý Pause bằng IsPaused và Time.timeScale; isPauseLocked ngăn Resume sau Victory/Game Over.
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
        // [BAO VE - PAUSE PHIM ESC] Escape gọi TogglePause; Update vẫn chạy khi timeScale = 0 nên có thể Resume nếu chưa bị khóa.
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    // [BAO VE] Nút Pause hoặc Escape đảo trạng thái nếu component còn hoạt động và chưa bị khóa kết quả trận.
    public void TogglePause()
    {
        if (!isActiveAndEnabled || isPauseLocked)
        {
            return;
        }

        SetPaused(!IsPaused);
    }

    // [BAO VE] Dừng game rồi khóa Pause khi kết thúc trận; người chơi không thể Escape để tiếp tục sau thắng/thua.
    public void LockPause()
    {
        SetPaused(true);
        isPauseLocked = true;
    }

    // [BAO VE] Đặt timeScale = 0 hoặc 1 và đổi Sprite nút; Update vẫn chạy nhưng deltaTime và các WaitForSeconds bị dừng khi Pause.
    void SetPaused(bool paused)
    {
        // Victory/Game Over phải giữ game dừng, kể cả khi component bị tắt/hủy.
        if (isPauseLocked)
        {
            Time.timeScale = 0f;
            return;
        }

        IsPaused = paused;
        // [BAO VE - PAUSE RESUME] IsPaused true đặt thời gian về 0; false trả về 1 để chuyển động, cooldown và WaitForSeconds tiếp tục.
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
