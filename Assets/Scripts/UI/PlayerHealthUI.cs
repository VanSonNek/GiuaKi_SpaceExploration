using UnityEngine;
using UnityEngine.UI;

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

    void Update()
    {
        if (healthSlider == null)
        {
            return;
        }

        // Unity coi reference tới Player đã bị Destroy là null.
        healthSlider.value = playerHealth != null ? playerHealth.CurrentHealth : 0f;
    }
}
