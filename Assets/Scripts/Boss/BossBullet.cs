using UnityEngine;

// [BAO VE] Định danh kiểu tấn công; BossShooting điều phối Aimed/Burst/Sweep, còn FirePattern tạo Single hoặc Spread.
public enum AttackPattern
{
    SingleShot = 0,
    SpreadShot = 1,
    AimedShot = 2,
    BurstShot = 3,
    SweepShot = 4,
    AimedBurst = 5
}

// [BAO VE] Đạn thù dùng chung cho Boss và Enemy; direction lưu hướng bay cố định, không tự đuổi theo Player.
public class BossBullet : MonoBehaviour
{
    [Min(0f)]
    public float speed = 5f;

    [Min(1)]
    public int damage = 1;

    [Min(0.01f)]
    public float lifeTime = 5f;

    public bool destroyOutsideView;
    public Vector2 Direction => direction;

    private bool hasHit;
    private Vector2 direction = Vector2.down;
    private Camera mainCamera;
    private SpriteRenderer spriteRenderer;

    // [BAO VE] Sinh một viên giữa; nếu SpreadShot thì thêm hai viên lệch góc spreadAngle.
    public static void FirePattern(GameObject prefab, Vector3 position, AttackPattern pattern,
        float spreadAngle, float speedOverride = -1f, Collider2D owner = null,
        bool cleanupOutsideView = false)
    {
        // [BAO VE - BOSS SINGLE SHOT] FirePattern luôn sinh viên giữa trước; SingleShot dừng ở một viên.
        Spawn(prefab, position, 0f, speedOverride, owner, cleanupOutsideView);
        // [BAO VE - BOSS SPREAD SHOT] Thêm hai viên tại góc âm/dương; tổng cộng ba hướng cùng xuất phát tại position.
        if (pattern == AttackPattern.SpreadShot)
        {
            Spawn(prefab, position, -spreadAngle, speedOverride, owner, cleanupOutsideView);
            Spawn(prefab, position, spreadAngle, speedOverride, owner, cleanupOutsideView);
        }
    }

    // [BAO VE] Đổi vector hướng đích sang góc xoay bằng Atan2 rồi tạo đạn; hướng chỉ được tính tại lúc bắn.
    public static void FireDirected(GameObject prefab, Vector3 position, Vector2 direction,
        Collider2D owner = null, bool cleanupOutsideView = true)
    {
        if (direction.sqrMagnitude < 0.0001f) direction = Vector2.down;
        float angle = Mathf.Atan2(direction.x, -direction.y) * Mathf.Rad2Deg;
        Spawn(prefab, position, angle, -1f, owner, cleanupOutsideView);
    }

    // [BAO VE] Instantiate Prefab, gán direction từ Vector2.down đã xoay và IgnoreCollision với chủ bắn để đạn không chạm tàu vừa sinh nó.
    static void Spawn(GameObject prefab, Vector3 position, float angle, float speedOverride,
        Collider2D owner, bool cleanupOutsideView)
    {
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);
        // [BAO VE - SINH DAN THU] EnemyShooting và BossShooting dùng chung dòng Instantiate này để tạo đạn, nhưng truyền Prefab/cấu hình riêng.
        GameObject instance = Instantiate(prefab, position, rotation);
        BossBullet bullet = instance.GetComponent<BossBullet>();
        // [BAO VE - DAN THU KHONG HOMING] Lưu hướng bay một lần khi sinh; Update không đọc lại tọa độ Player nên đạn không bẻ hướng đuổi theo.
        bullet.direction = rotation * Vector2.down;
        if (speedOverride >= 0f)
        {
            bullet.speed = speedOverride;
        }

        bullet.destroyOutsideView |= cleanupOutsideView;
        Collider2D collider = instance.GetComponent<Collider2D>();
        if (owner != null && collider != null)
        {
            Physics2D.IgnoreCollision(collider, owner);
        }
    }

    void Start()
    {
        mainCamera = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();
        Destroy(gameObject, lifeTime);
    }

    // [BAO VE] Bay theo direction * speed * deltaTime; nếu bật destroyOutsideView thì hủy đạn khi Sprite ra hẳn ngoài Camera.
    void Update()
    {
        transform.position += (Vector3)direction * speed * Time.deltaTime;

        if (destroyOutsideView && mainCamera != null && spriteRenderer != null)
        {
            Vector3 bottom = mainCamera.ViewportToWorldPoint(Vector3.zero);
            Vector3 top = mainCamera.ViewportToWorldPoint(Vector3.one);
            Bounds bounds = spriteRenderer.bounds;
            if (bounds.max.y < bottom.y || bounds.min.y > top.y
                || bounds.max.x < bottom.x || bounds.min.x > top.x)
            {
                Destroy(gameObject);
            }
        }
    }

    // [BAO VE] Đạn chạm Player thì gây damage và bị hủy; hasHit chặn gây sát thương nhiều lần trong cùng frame.
    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasHit)
        {
            return;
        }

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            // Destroy có hiệu lực cuối frame, nên đánh dấu trúng trước khi trừ máu.
            hasHit = true;
            playerHealth.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
