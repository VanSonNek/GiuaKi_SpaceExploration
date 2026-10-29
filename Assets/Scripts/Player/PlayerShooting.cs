using UnityEngine;

// [BAO VE] Quản lý cấp súng và tạo đạn; bulletPrefab là mẫu đạn, firePoint là vị trí bắn, fireRate là số giây giữa hai loạt.
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

    // [BAO VE] Giữ Space gọi TryFire mỗi frame; TryFire tự kiểm tra thời gian chờ nên không bắn liên tục vô hạn trong một frame.
    void Update()
    {
        // Giữ Space để bắn liên tục theo fireRate.
        if (Input.GetKey(KeyCode.Space)) TryFire();
    }

    // [BAO VE] Chặn bắn khi Pause, hết HP hoặc chưa tới nextFireTime; WeaponLevel chọn một tia, hai tia song song hoặc ba tia hình quạt.
    public bool TryFire()
    {
        // [BAO VE - PLAYER DIEU KIEN BAN] timeScale chặn bắn khi Pause; CurrentHealth chặn Player chết, nextFireTime chặn bắn quá sớm.
        if (!Application.isPlaying || !isActiveAndEnabled || Time.timeScale <= 0f
            || bulletPrefab == null || firePoint == null || Time.time < nextFireTime
            || (playerHealth != null && playerHealth.CurrentHealth <= 0)) return false;

        Vector3 origin = firePoint.position;
        // [BAO VE - PLAYER TIA GIUA] Cấp 1 và 3 có tia giữa; cấp 2 bỏ tia giữa và chỉ dùng hai tia ngoài.
        if (WeaponLevel != 2)
            SpawnBullet(origin, 0f);

        // [BAO VE - PLAYER HAI TIA NGOAI] Từ cấp 2 tạo hai tia đối xứng quanh firePoint với khoảng cách sideShotOffset.
        if (WeaponLevel >= 2)
        {
            Vector3 offset = Vector3.right * sideShotOffset;
            // [BAO VE] Cấp 3 xoay hai tia ngoài ngược dấu; levelThreeSpreadAngle cấu hình góc quạt, cấp 2 giữ góc bằng 0.
            float angle = WeaponLevel == 3 ? Mathf.Clamp(levelThreeSpreadAngle, 0f, 45f) : 0f;
            SpawnBullet(origin - offset, angle);
            SpawnBullet(origin + offset, -angle);
        }

        // [BAO VE - PLAYER TOC DO BAN] fireRate là thời gian nghỉ mỗi loạt, không phải số viên/giây; chỉ được bắn lại khi Time.time đạt mốc này.
        nextFireTime = Time.time + fireRate;
        return true;
    }

    // [BAO VE] Tính vị trí để Collider đạn nằm trên Player rồi Instantiate Prefab với góc xoay; Bullet dùng transform.up để bay theo góc đó.
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

        // [BAO VE - SINH DAN PLAYER] Đây là dòng tạo GameObject đạn thật từ bulletPrefab tại vị trí và góc đã tính.
        Instantiate(bulletPrefab, position, rotation);
    }

    // [BAO VE] Tăng WeaponLevel tối đa 3 và phát WeaponLevelChanged để HUD cập nhật; không đổi damage, speed hoặc fireRate.
    public void UpgradeWeapon()
    {
        // [BAO VE - PLAYER GIOI HAN CAP SUNG] WeaponLevel >= 3 chặn nâng thêm; nhặt tiếp không tăng cấp vượt 3.
        if (!Application.isPlaying || Time.timeScale <= 0f || WeaponLevel >= 3
            || (playerHealth != null && playerHealth.CurrentHealth <= 0)) return;

        WeaponLevel++;
        // [BAO VE - PLAYER SU KIEN CAP SUNG] Sau khi WeaponLevel tăng, báo cấp mới cho WeaponLevelUI; ?.Invoke an toàn khi không có bên nghe.
        WeaponLevelChanged?.Invoke(WeaponLevel);
    }
}
