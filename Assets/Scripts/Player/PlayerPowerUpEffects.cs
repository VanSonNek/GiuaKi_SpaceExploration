using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerHealth), typeof(PlayerShooting))]
public class PlayerPowerUpEffects : MonoBehaviour
{
    [SerializeField] private PowerUpDropManager dropManager;

    private PowerUpDropManager subscribedManager;
    private PlayerHealth playerHealth;
    private PlayerShooting playerShooting;

    void OnEnable()
    {
        if (!Application.isPlaying) return;

        playerHealth = GetComponent<PlayerHealth>();
        playerShooting = GetComponent<PlayerShooting>();
        if (dropManager == null)
        {
            Debug.LogWarning("PlayerPowerUpEffects needs this Scene's PowerUpDropManager.", this);
            enabled = false;
            return;
        }

        subscribedManager = dropManager;
        subscribedManager.PickupCollected += ApplyPickup;
    }

    void ApplyPickup(PowerUpType type, PlayerHealth collector)
    {
        if (!isActiveAndEnabled || playerHealth == null || collector != playerHealth
            || playerHealth.CurrentHealth <= 0 || subscribedManager == null
            || !subscribedManager.CanCollect || Time.timeScale <= 0f) return;

        if (type == PowerUpType.WeaponUpgrade)
        {
            if (playerShooting != null) playerShooting.UpgradeWeapon();
        }
        else if (type == PowerUpType.HealthPickup)
        {
            playerHealth.Heal(1);
        }
    }

    void OnDisable()
    {
        if (subscribedManager != null) subscribedManager.PickupCollected -= ApplyPickup;
        subscribedManager = null;
    }
}
