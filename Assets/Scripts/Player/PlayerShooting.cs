using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;

    // Thời gian chờ giữa hai lần bắn, tính bằng giây.
    [Min(0.01f)]
    public float fireRate = 0.2f;

    [SerializeField, Min(0.01f)] private float sideShotOffset = 0.28f;
    [SerializeField, Range(0f, 45f)] private float levelThreeSpreadAngle = 15f;

    public int WeaponLevel { get; private set; } = 1;
    public event System.Action<int> WeaponLevelChanged;

    private float nextFireTime;
    private PlayerHealth playerHealth;
    private Collider2D playerCollider;

    void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
        playerCollider = GetComponent<Collider2D>();
    }

    void Start()
    {
        if (bulletPrefab == null || firePoint == null)
        {
            Debug.LogWarning("Hãy gán Bullet Prefab và Fire Point cho PlayerShooting.", this);
        }
    }

    void Update()
    {
        // Giữ Space để bắn liên tục theo fireRate.
        if (Input.GetKey(KeyCode.Space)) TryFire();
    }

    public bool TryFire()
    {
        if (!Application.isPlaying || !isActiveAndEnabled || Time.timeScale <= 0f
            || bulletPrefab == null || firePoint == null || Time.time < nextFireTime
            || (playerHealth != null && playerHealth.CurrentHealth <= 0)) return false;

        Vector3 origin = firePoint.position;
        if (WeaponLevel != 2)
            SpawnBullet(origin, 0f);

        if (WeaponLevel >= 2)
        {
            Vector3 offset = Vector3.right * sideShotOffset;
            float angle = WeaponLevel == 3 ? Mathf.Clamp(levelThreeSpreadAngle, 0f, 45f) : 0f;
            SpawnBullet(origin - offset, angle);
            SpawnBullet(origin + offset, -angle);
        }

        nextFireTime = Time.time + fireRate;
        return true;
    }

    void SpawnBullet(Vector3 position, float angle)
    {
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);
        BoxCollider2D bulletCollider = bulletPrefab.GetComponent<BoxCollider2D>();
        if (playerCollider != null && bulletCollider != null)
        {
            Vector3 scale = bulletPrefab.transform.localScale;
            Vector3 halfSize = Vector3.Scale((Vector3)bulletCollider.size * 0.5f, scale);
            Vector3 centerOffset = rotation * Vector3.Scale((Vector3)bulletCollider.offset, scale);
            float radians = angle * Mathf.Deg2Rad;
            float halfHeight = Mathf.Abs(Mathf.Sin(radians) * halfSize.x)
                               + Mathf.Abs(Mathf.Cos(radians) * halfSize.y);

            // Giữ toàn bộ Collider đạn ở trên Player, kể cả khi tia ngoài xoay.
            position.y = Mathf.Max(position.y, playerCollider.bounds.max.y
                + halfHeight - centerOffset.y + 0.01f);
        }

        Instantiate(bulletPrefab, position, rotation);
    }

    public void UpgradeWeapon()
    {
        if (!Application.isPlaying || Time.timeScale <= 0f || WeaponLevel >= 3
            || (playerHealth != null && playerHealth.CurrentHealth <= 0)) return;

        WeaponLevel++;
        WeaponLevelChanged?.Invoke(WeaponLevel);
    }
}
