using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;

    // Thời gian chờ giữa hai lần bắn, tính bằng giây.
    [Min(0.01f)]
    public float fireRate = 0.2f;

    [SerializeField, Min(0.01f)] private float sideShotOffset = 0.28f;

    public int WeaponLevel { get; private set; } = 1;
    public event System.Action<int> WeaponLevelChanged;

    private float nextFireTime;
    private PlayerHealth playerHealth;

    void Awake()
    {
        playerHealth = GetComponent<PlayerHealth>();
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
            Instantiate(bulletPrefab, origin, Quaternion.identity);

        if (WeaponLevel >= 2)
        {
            Vector3 offset = Vector3.right * sideShotOffset;
            Instantiate(bulletPrefab, origin - offset, Quaternion.identity);
            Instantiate(bulletPrefab, origin + offset, Quaternion.identity);
        }

        nextFireTime = Time.time + fireRate;
        return true;
    }

    public void UpgradeWeapon()
    {
        if (!Application.isPlaying || Time.timeScale <= 0f || WeaponLevel >= 3
            || (playerHealth != null && playerHealth.CurrentHealth <= 0)) return;

        WeaponLevel++;
        WeaponLevelChanged?.Invoke(WeaponLevel);
    }
}
