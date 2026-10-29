using UnityEngine;
using UnityEngine.UI;

// [BAO VE] Nghe Level1Controller.BossSpawned để gắn thanh máu với đúng Boss vừa xuất hiện.
public class BossHealthUI : MonoBehaviour
{
    public Level1Controller level1Controller;
    public GameObject bossHealthBar;
    public Slider healthSlider;

    private Boss currentBoss;
    private Level1Controller subscribedController;

    void OnEnable()
    {
        currentBoss = null;

        if (bossHealthBar != null)
        {
            bossHealthBar.SetActive(false);
        }

        if (level1Controller == null || bossHealthBar == null || healthSlider == null)
        {
            Debug.LogWarning("Hãy gán Level1Controller, Boss Health Bar và Health Slider cho BossHealthUI.", this);
            return;
        }

        subscribedController = level1Controller;
        // [BAO VE - BOSS HEALTH BAR] BossSpawned nối Boss mới với UI; controller đợi Boss.Start khởi tạo HP trước khi phát sự kiện.
        subscribedController.BossSpawned += ShowBossHealth;

        // Khi UI được bật lại, vẫn có thể hiển thị Boss đang chiến đấu.
        if (subscribedController.CurrentBoss != null)
        {
            ShowBossHealth(subscribedController.CurrentBoss);
        }
    }

    // [BAO VE] Gán Boss hiện tại, cập nhật maxValue/CurrentHealth và bật bossHealthBar.
    void ShowBossHealth(Boss boss)
    {
        if (boss == null || bossHealthBar == null || healthSlider == null)
        {
            Debug.LogWarning("Thiếu Boss hoặc reference thanh máu trong BossHealthUI.", this);
            return;
        }

        currentBoss = boss;
        healthSlider.minValue = 0f;
        healthSlider.maxValue = boss.MaxHealth;
        healthSlider.value = boss.CurrentHealth;
        bossHealthBar.SetActive(true);
    }

    // [BAO VE] Cập nhật Slider theo HP Boss mỗi frame; ẩn thanh khi Boss đã bị Destroy.
    void Update()
    {
        if (currentBoss == null)
        {
            // Unity coi reference tới Boss đã bị Destroy là null.
            if (bossHealthBar != null && bossHealthBar.activeSelf)
            {
                bossHealthBar.SetActive(false);
            }

            return;
        }

        if (healthSlider != null)
        {
            healthSlider.value = currentBoss.CurrentHealth;
        }
    }

    // [BAO VE] Gỡ BossSpawned trên đúng controller đã đăng ký để tránh callback cũ khi đổi Scene hoặc bật lại UI.
    void OnDisable()
    {
        if (subscribedController != null)
        {
            subscribedController.BossSpawned -= ShowBossHealth;
        }

        subscribedController = null;
    }
}
