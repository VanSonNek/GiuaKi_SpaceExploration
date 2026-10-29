using UnityEngine;

// [BAO VE] Quản lý HP, điểm và va chạm của Enemy; Defeated báo Enemy bị tiêu diệt bằng sát thương cho hệ thống rơi vật phẩm.
public class Enemy : MonoBehaviour
{
    [Min(1)]
    public int maxHealth = 3;

    [Min(1)]
    public int contactDamage = 1;

    [Min(0)]
    public int scoreValue = 100;

    public GameObject deathEffectPrefab;

    private int currentHealth;
    private bool hasHitPlayer;
    private bool isDead;

    public bool IsAlive => !isDead && !hasHitPlayer;

    // Only Bullet currently calls TakeDamage; contact/despawn never raise this event.
    public static event System.Action<Enemy> Defeated;

    // [BAO VE - ENEMY HP BAN DAU] Mỗi Enemy mới lấy currentHealth từ maxHealth của Prefab.
    void Start()
    {
        currentHealth = maxHealth;
    }

    // [BAO VE] Trừ currentHealth; hết HP thì cộng scoreValue, tạo hiệu ứng, phát Defeated và Destroy; isDead chặn cộng điểm lặp.
    public void TakeDamage(int damage)
    {
        // [BAO VE - ENEMY CHAN DAMAGE LAP] Không nhận thêm damage nếu đã chết hoặc đã va Player và đang chờ Destroy.
        if (isDead || hasHitPlayer)
        {
            return;
        }

        // [BAO VE - ENEMY TRU HP] Bullet.OnTriggerEnter2D gọi TakeDamage(damage); dòng này thực sự trừ HP Enemy.
        currentHealth -= damage;

        DamageFlash damageFlash = GetComponent<DamageFlash>();

        if (damage > 0 && damageFlash != null)
        {
            damageFlash.Flash();
        }

        // [BAO VE - ENEMY CHET] Chỉ đi vào nhánh tiêu diệt khi HP đã hết; Enemy chưa hết HP vẫn tiếp tục tồn tại.
        if (currentHealth <= 0)
        {
            // Chỉ cộng một lần khi bị bắn hết HP, trước khi Destroy cuối frame.
            // [BAO VE - ENEMY DANH DAU CHET] Đặt cờ trước khi cộng điểm/phát sự kiện để không xử lý chết lần nữa trong frame này.
            isDead = true;

            if (ScoreManager.Instance != null)
            {
                // [BAO VE - ENEMY CONG DIEM] Gửi scoreValue sang ScoreManager; chỉ nhánh chết do TakeDamage mới cộng điểm.
                ScoreManager.Instance.AddScore(scoreValue);
            }

            if (deathEffectPrefab != null)
            {
                // [BAO VE - ENEMY HIEU UNG NO] Nếu có deathEffectPrefab, tạo hiệu ứng tại vị trí Enemy trước khi hủy nó.
                Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            }

            // [BAO VE - ENEMY ROI VAT PHAM] Báo cho PowerUpDropManager xét xác suất rơi; phát sự kiện không có nghĩa chắc chắn sinh vật phẩm.
            Defeated?.Invoke(this);
            // [BAO VE - ENEMY HUY SAU KHI CHET] Hủy GameObject Enemy đã hết HP sau khi hoàn tất điểm, hiệu ứng và sự kiện.
            Destroy(gameObject);
        }
    }

    // [BAO VE] Va chạm Player gây contactDamage rồi hủy Enemy; nhánh này không cộng điểm và không phát Defeated.
    void OnTriggerEnter2D(Collider2D other)
    {
        // Chặn gây sát thương lần nữa trong lúc chờ Enemy bị hủy cuối frame.
        // [BAO VE - ENEMY CHAN VA CHAM LAP] Chặn callback Trigger lặp khi Enemy đã gây damage hoặc đã chết.
        if (hasHitPlayer || isDead)
        {
            return;
        }

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            // [BAO VE - ENEMY VA CHAM PLAYER] Đánh dấu trước khi gây contactDamage để Enemy này không làm Player mất máu hai lần.
            hasHitPlayer = true;
            // [BAO VE - ENEMY CONTACT DAMAGE] PlayerHealth của đối tượng va chạm nhận contactDamage; không đi qua Enemy.TakeDamage.
            playerHealth.TakeDamage(contactDamage);

            if (deathEffectPrefab != null)
            {
                // [BAO VE - ENEMY NO KHI VA CHAM] Va Player cũng có thể tạo hiệu ứng nổ nếu deathEffectPrefab đã được gán.
                Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            }

            // [BAO VE - ENEMY HUY SAU VA CHAM] Hủy Enemy sau va Player; không gọi AddScore hoặc Defeated nên không cộng điểm hay xét rơi vật phẩm.
            Destroy(gameObject);
        }
    }
}
