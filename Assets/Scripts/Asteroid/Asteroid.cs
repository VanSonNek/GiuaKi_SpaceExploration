using UnityEngine;

// [BAO VE] Quản lý HP, contactDamage và điểm của thiên thạch; tách khỏi phần chuyển động trong AsteroidMovement.
public class Asteroid : MonoBehaviour
{
    [Min(1)]
    public int maxHealth = 2;

    [Min(1)]
    public int contactDamage = 1;

    [Min(0)]
    public int scoreValue = 50;

    private int currentHealth;
    private bool hasHitPlayer;
    private bool isDead;

    void Start()
    {
        currentHealth = maxHealth;
    }

    // [BAO VE] Đạn trừ HP, nháy màu và cộng scoreValue khi bắn vỡ; isDead chặn xử lý điểm lặp trước khi Destroy.
    public void TakeDamage(int damage)
    {
        if (isDead || hasHitPlayer)
        {
            return;
        }

        // [BAO VE - ASTEROID TRU HP] Bullet.OnTriggerEnter2D gọi Asteroid.TakeDamage; dòng này trừ damage khỏi currentHealth của thiên thạch.
        currentHealth -= damage;

        DamageFlash damageFlash = GetComponent<DamageFlash>();

        if (damage > 0 && damageFlash != null)
        {
            damageFlash.Flash();
        }

        if (currentHealth <= 0)
        {
            // Chỉ cộng một lần khi bị bắn hết HP, trước khi Destroy cuối frame.
            isDead = true;

            if (ScoreManager.Instance != null)
            {
                // [BAO VE - ASTEROID CONG DIEM] Chỉ cộng khi bị bắn vỡ; Asteroid không phát Enemy.Defeated nên không kích hoạt rơi vật phẩm từ Enemy.
                ScoreManager.Instance.AddScore(scoreValue);
            }

            Destroy(gameObject);
        }
    }

    // [BAO VE] Thiên thạch chạm Player gây contactDamage rồi tự hủy; va chạm này không cộng điểm.
    void OnTriggerEnter2D(Collider2D other)
    {
        // Chặn gây sát thương lần nữa trong lúc chờ thiên thạch bị hủy.
        if (hasHitPlayer || isDead)
        {
            return;
        }

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            hasHitPlayer = true;
            playerHealth.TakeDamage(contactDamage);
            Destroy(gameObject);
        }
    }
}
