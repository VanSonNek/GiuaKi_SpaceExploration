using UnityEngine;

// [BAO VE] Đạn của Player: speed là tốc độ, damage là sát thương, lifeTime giới hạn thời gian tồn tại.
public class Bullet : MonoBehaviour
{
    [Min(0f)]
    public float speed = 10f;

    [Min(1)]
    public int damage = 1;

    // Thời gian tồn tại của đạn, tính bằng giây.
    [Min(0.01f)]
    public float lifeTime = 3f;

    private bool hasHit;

    // [BAO VE] Lên lịch Destroy theo lifeTime để đạn không tồn tại mãi khi không va chạm.
    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    // [BAO VE] Bay theo trục up riêng của viên đạn; góc xoay khi Instantiate quyết định tia thẳng hay tia chếch.
    void Update()
    {
        // Tia giữa bay lên; tia ngoài bay theo góc được gán khi tạo đạn.
        transform.position += transform.up * speed * Time.deltaTime;
    }

    // [BAO VE] Nhận Trigger với Enemy, Asteroid hoặc Boss rồi gọi TakeDamage; hasHit bảo đảm một viên chỉ gây sát thương một lần.
    void OnTriggerEnter2D(Collider2D other)
    {
        // Destroy có hiệu lực cuối frame, nên chặn đạn gây sát thương lần nữa.
        if (hasHit)
        {
            return;
        }

        // [BAO VE - DAN PHAT HIEN ENEMY] Unity gọi OnTriggerEnter2D khi có Trigger 2D; GetComponent xác định Collider vừa chạm có Enemy hay không.
        Enemy enemy = other.GetComponent<Enemy>();

        if (enemy != null)
        {
            hasHit = true;
            // [BAO VE - DAN GAY DAMAGE ENEMY] Chuyển damage của viên đạn sang Enemy.TakeDamage; việc trừ HP và xử lý chết nằm trong Enemy.cs.
            enemy.TakeDamage(damage);
            // [BAO VE - DAN HUY SAU TRUNG ENEMY] Xóa viên đạn sau khi gây sát thương; hasHit đã bật trước đó để không đánh trúng thêm mục tiêu.
            Destroy(gameObject);
            return;
        }

        Asteroid asteroid = other.GetComponent<Asteroid>();

        if (asteroid != null)
        {
            hasHit = true;
            // [BAO VE - DAN GAY DAMAGE ASTEROID] Chuyển damage sang Asteroid, không tự sửa HP hay cộng điểm trong Bullet.
            asteroid.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        Boss boss = other.GetComponent<Boss>();

        if (boss != null)
        {
            hasHit = true;
            // [BAO VE - DAN GAY DAMAGE BOSS] Chuyển damage sang Boss.TakeDamage; Boss.cs xử lý HP và điểm khi Boss chết.
            boss.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
