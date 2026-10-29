using UnityEngine;

[RequireComponent(typeof(Enemy), typeof(SpriteRenderer))]
// [BAO VE] Cho Enemy bắn bằng Prefab có BossBullet; shootingChance chọn tàu biết bắn, fireInterval là khoảng nghỉ giữa các loạt.
public class EnemyShooting : MonoBehaviour
{
    public GameObject bulletPrefab;
    public bool canShoot = true;
    [Range(0f, 1f)] public float shootingChance = 0.3f;
    [Min(0.1f)] public float fireInterval = 2.8f;
    [Min(0.1f)] public float bulletSpeed = 3f;
    [Min(0f)] public float initialDelay = 1.2f;
    [Range(0f, 1f)] public float spreadChance;
    [Range(1f, 45f)] public float spreadAngle = 18f;
    [Min(0f)] public float muzzleGap = 0.12f;

    public bool IsShooter { get; private set; }
    public AttackPattern CurrentPattern { get; private set; }
    public int BurstsFired { get; private set; }

    private Enemy enemy;
    private SpriteRenderer spriteRenderer;
    private Collider2D owner;
    private Camera mainCamera;
    private float nextFireTime;
    private float bulletHalfHeight;

    // [BAO VE] Chọn IsShooter và SingleShot/SpreadShot một lần cho mỗi Enemy; initialDelay trì hoãn phát đầu.
    void Start()
    {
        enemy = GetComponent<Enemy>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        owner = GetComponent<Collider2D>();
        mainCamera = Camera.main;

        if (mainCamera == null || bulletPrefab == null
            || bulletPrefab.GetComponent<BossBullet>() == null
            || bulletPrefab.GetComponent<SpriteRenderer>() == null)
        {
            Debug.LogWarning("EnemyShooting needs a camera and a hostile bullet prefab.", this);
            enabled = false;
            return;
        }

        // Decide once per spawned Enemy; Update never rerolls this probability.
        // [BAO VE - ENEMY XAC SUAT BAN] Chỉ bốc xác suất một lần lúc Start; không bốc lại mỗi frame để biến mọi Enemy thành tàu bắn.
        IsShooter = canShoot && Random.value < shootingChance;
        CurrentPattern = spreadChance > 0f && Random.value < spreadChance
            ? AttackPattern.SpreadShot : AttackPattern.SingleShot;
        bulletHalfHeight = bulletPrefab.GetComponent<SpriteRenderer>().bounds.extents.y;
        nextFireTime = Time.time + initialDelay;
    }

    // [BAO VE] Chỉ bắn khi Enemy còn sống, hết thời gian chờ và thân tàu đã vào đủ trong Camera; FirePattern tạo đạn dưới thân tàu.
    void Update()
    {
        if (!canShoot || !IsShooter || !enemy.IsAlive || Time.timeScale <= 0f
            || Time.time < nextFireTime)
        {
            return;
        }

        Bounds bounds = spriteRenderer.bounds;
        Vector3 bottom = mainCamera.ViewportToWorldPoint(Vector3.zero);
        Vector3 top = mainCamera.ViewportToWorldPoint(Vector3.one);
        Vector3 muzzle = new Vector3(bounds.center.x,
            bounds.min.y - bulletHalfHeight - muzzleGap, transform.position.z);

        // Enter the screen fully, and leave room for the projectile below the hull.
        if (bounds.min.x < bottom.x || bounds.max.x > top.x
            || bounds.min.y < bottom.y || bounds.max.y > top.y
            || muzzle.y - bulletHalfHeight <= bottom.y)
        {
            return;
        }

        // [BAO VE - SINH DAN ENEMY] Đây là lệnh tạo đạn thù qua BossBullet; dùng bulletPrefab, CurrentPattern và tốc độ riêng của Enemy.
        BossBullet.FirePattern(bulletPrefab, muzzle, CurrentPattern, spreadAngle,
            bulletSpeed, owner, true);
        BurstsFired++;
        nextFireTime = Time.time + Mathf.Max(0.1f, fireInterval);
    }
}
