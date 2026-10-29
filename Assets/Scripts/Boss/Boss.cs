using UnityEngine;

// [BAO VE] Quản lý maxHealth, currentHealth, IsDead và scoreValue; logic chọn Phase nằm trong BossShooting.
public class Boss : MonoBehaviour
{
    [Min(1)]
    public int maxHealth = 25;

    [Min(1)]
    public int contactDamage = 1;

    [Min(0)]
    public int scoreValue = 1000;

    public bool IsDead { get; private set; }
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    private int currentHealth;

    // [BAO VE] Khởi tạo HP từ maxHealth trên từng Boss Prefab; không dùng một giá trị HP chung cho mọi Level.
    void Start()
    {
        currentHealth = maxHealth;
    }

    // [BAO VE] Trừ HP và nháy màu; hết HP thì đánh dấu IsDead, cộng điểm một lần và Destroy để Level1Controller nhận biết thắng.
    public void TakeDamage(int damage)
    {
        if (IsDead)
        {
            return;
        }

        // [BAO VE - BOSS TRU HP] Bullet gọi TakeDamage; currentHealth giảm nhưng maxHealth của Prefab không đổi.
        currentHealth -= damage;

        DamageFlash damageFlash = GetComponent<DamageFlash>();

        if (damage > 0 && damageFlash != null)
        {
            damageFlash.Flash();
        }

        // [BAO VE - BOSS CHET] Hết HP thì đánh dấu IsDead, cộng điểm và hủy Boss; BossShooting đọc IsDead để dừng đòn còn chờ.
        if (currentHealth <= 0)
        {
            IsDead = true;

            if (ScoreManager.Instance != null)
            {
                // [BAO VE - BOSS CONG DIEM] Cộng scoreValue cấu hình trên Boss Prefab đúng một lần nhờ cờ IsDead.
                ScoreManager.Instance.AddScore(scoreValue);
            }

            // [BAO VE - BOSS HUY VA BAO THANG] Hủy Boss; Level1Controller đang chờ object này thành null sẽ xét gọi ShowVictory.
            Destroy(gameObject);
        }
    }

    // [BAO VE] Boss chạm Player gây contactDamage qua PlayerHealth.TakeDamage; Boss không tự chết khi va chạm.
    void OnTriggerEnter2D(Collider2D other)
    {
        if (IsDead)
        {
            return;
        }

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            // Va chạm chỉ trừ máu Player, Boss vẫn sống.
            playerHealth.TakeDamage(contactDamage);
        }
    }
}
