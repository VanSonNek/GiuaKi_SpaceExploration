using UnityEngine;
using UnityEngine.UI;

// [BAO VE] Hiển thị HP Player bằng Slider; maxValue lấy từ MaxHealth, value lấy từ CurrentHealth.
public class PlayerHealthUI : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public Slider healthSlider;

    void Start()
    {
        if (healthSlider == null)
        {
            Debug.LogWarning("Hãy gán Health Slider cho PlayerHealthUI.", this);
            enabled = false;
            return;
        }

        healthSlider.minValue = 0f;

        if (playerHealth == null)
        {
            Debug.LogWarning("Hãy gán Player Health cho PlayerHealthUI.", this);
            healthSlider.value = 0f;
            return;
        }

        healthSlider.maxValue = playerHealth.MaxHealth;
        healthSlider.value = playerHealth.CurrentHealth;
    }

    // [BAO VE] Đọc HP mỗi frame để thanh máu theo kịp sát thương và hồi máu; Player đã Destroy thì hiển thị 0.
    void Update()
    {
        if (healthSlider == null)
        {
            return;
        }

        // Unity coi reference tới Player đã bị Destroy là null.
        // [BAO VE - CAP NHAT HP UI] HP hiện được đọc trong Update; không có HealthChanged event trong PlayerHealth.
        healthSlider.value = playerHealth != null ? playerHealth.CurrentHealth : 0f;
    }
}
