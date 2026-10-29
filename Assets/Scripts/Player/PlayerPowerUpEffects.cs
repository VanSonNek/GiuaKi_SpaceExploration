using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerHealth), typeof(PlayerShooting))]
// [BAO VE] Cầu nối giữa sự kiện nhặt Power-up và PlayerShooting/PlayerHealth; mỗi loại vật phẩm có hiệu ứng riêng.
public class PlayerPowerUpEffects : MonoBehaviour
{
    [SerializeField] private PowerUpDropManager dropManager;

    private PowerUpDropManager subscribedManager;
    private PlayerHealth playerHealth;
    private PlayerShooting playerShooting;

    // [BAO VE] Lấy component cùng Player và đăng ký PickupCollected trên dropManager được gán trong Scene.
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

    // [BAO VE] Xác nhận đúng Player còn sống và game không Pause; WeaponUpgrade gọi UpgradeWeapon, HealthPickup gọi Heal(1).
    void ApplyPickup(PowerUpType type, PlayerHealth collector)
    {
        if (!isActiveAndEnabled || playerHealth == null || collector != playerHealth
            || playerHealth.CurrentHealth <= 0 || subscribedManager == null
            || !subscribedManager.CanCollect || Time.timeScale <= 0f) return;

        if (type == PowerUpType.WeaponUpgrade)
        {
            // [BAO VE - NHAT NANG SUNG] WeaponUpgrade gọi UpgradeWeapon trên chính Player này; giới hạn cấp 3 nằm trong PlayerShooting.
            if (playerShooting != null) playerShooting.UpgradeWeapon();
        }
        else if (type == PowerUpType.HealthPickup)
        {
            // [BAO VE - NHAT HOI 1 HP] HealthPickup yêu cầu hồi 1 HP; PlayerHealth.Heal tự giới hạn theo maxHealth.
            playerHealth.Heal(1);
        }
    }

    // [BAO VE] Hủy đăng ký đúng manager đã nghe để không áp dụng hiệu ứng nhiều lần sau khi bật lại hoặc Retry.
    void OnDisable()
    {
        if (subscribedManager != null) subscribedManager.PickupCollected -= ApplyPickup;
        subscribedManager = null;
    }
}
