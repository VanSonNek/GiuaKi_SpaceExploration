using UnityEngine;
using UnityEngine.UI;

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
        subscribedController.BossSpawned += ShowBossHealth;

        // Khi UI được bật lại, vẫn có thể hiển thị Boss đang chiến đấu.
        if (subscribedController.CurrentBoss != null)
        {
            ShowBossHealth(subscribedController.CurrentBoss);
        }
    }

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

    void OnDisable()
    {
        if (subscribedController != null)
        {
            subscribedController.BossSpawned -= ShowBossHealth;
        }

        subscribedController = null;
    }
}
