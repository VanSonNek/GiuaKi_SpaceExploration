using System;
using UnityEngine;

// [BAO VE] Giữ currentHealth và maxHealth của Player; sự kiện Died thông báo cho hệ thống Game Over.
public class PlayerHealth : MonoBehaviour
{
    [Min(1)]
    public int maxHealth = 3;

    private int currentHealth;
    private bool isDead;

    public event Action Died;

    // [BAO VE - PLAYER DOC HP] Chỉ công khai đọc HP qua property; hiện chưa có sự kiện HP thay đổi, PlayerHealthUI đọc giá trị mỗi frame.
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    // [BAO VE] Mỗi Player mới bắt đầu với currentHealth = maxHealth; Retry tạo lại Player qua việc tải lại Scene.
    void Start()
    {
        currentHealth = maxHealth;
    }

    // [BAO VE] Hồi lượng HP còn thiếu, không vượt maxHealth; trả về số HP thực sự hồi và không hồi sinh Player đã chết.
    public int Heal(int amount)
    {
        if (isDead || currentHealth <= 0 || amount <= 0) return 0;

        // [BAO VE - PLAYER HOI KHONG VUOT MAX] Lấy số nhỏ hơn giữa amount và HP còn thiếu, vì vậy currentHealth không vượt maxHealth.
        int restored = Mathf.Min(amount, Mathf.Max(0, maxHealth - currentHealth));
        currentHealth += restored;
        return restored;
    }

    // [BAO VE] Trừ HP và nháy màu; isDead chặn xử lý chết lặp, Died được phát trước khi Destroy Player.
    public void TakeDamage(int damage)
    {
        if (isDead)
        {
            return;
        }

        // [BAO VE - PLAYER TRU HP] Đây là dòng giảm máu Player khi đạn thù, Enemy, Asteroid hoặc Boss gọi TakeDamage.
        currentHealth -= damage;

        DamageFlash damageFlash = GetComponent<DamageFlash>();

        if (damage > 0 && damageFlash != null)
        {
            damageFlash.Flash();
        }

        // [BAO VE - PLAYER CHET] Hết HP thì đặt isDead, báo Died và hủy Player; không có cơ chế hồi sinh trong Heal.
        if (currentHealth <= 0)
        {
            // Chỉ báo chết một lần, trước khi Player bị hủy cuối frame.
            isDead = true;
            // [BAO VE - PLAYER BAO GAME OVER] GameResultManager nghe Died để gọi ShowGameOver; các hệ thống vật phẩm/banner cũng nghe để dừng phù hợp.
            Died?.Invoke();
            Destroy(gameObject);
        }
    }
}
