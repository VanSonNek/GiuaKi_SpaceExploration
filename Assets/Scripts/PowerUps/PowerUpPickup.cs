using System;
using UnityEngine;

public enum PowerUpType
{
    WeaponUpgrade,
    HealthPickup
}

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(Rigidbody2D))]
// [BAO VE] Vật phẩm lưu Type, rơi theo fallSpeed và báo Collected; bản thân Pickup không trực tiếp tăng súng hoặc hồi HP.
public class PowerUpPickup : MonoBehaviour
{
    [SerializeField] private PowerUpType type;
    [SerializeField, Min(0.1f)] private float fallSpeed = 0.85f;
    [SerializeField, Min(1f)] private float maximumLifetime = 20f;

    public PowerUpType Type => type;
    public bool IsCollected { get; private set; }
    public event Action<PowerUpPickup, PlayerHealth> Collected;

    private PowerUpDropManager owner;
    private Camera mainCamera;
    private SpriteRenderer spriteRenderer;

    internal void Initialize(PowerUpDropManager manager, Camera camera)
    {
        owner = manager;
        mainCamera = camera;
    }

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // [BAO VE] Đặt thời gian tự hủy maximumLifetime; Destroy có thời gian chờ dùng thời gian game nên Pause cũng dừng đếm.
    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        // Scaled lifetime is also a fallback if the camera disappears.
        Destroy(gameObject, maximumLifetime);
    }

    // [BAO VE] Cho vật phẩm rơi xuống theo deltaTime, dừng khi Pause và hủy khi đã ra ngoài vùng chơi.
    void Update()
    {
        if (Time.timeScale <= 0f || IsCollected) return;

        transform.position += Vector3.down * fallSpeed * Time.deltaTime;
        if (mainCamera != null && spriteRenderer != null)
        {
            Vector3 bottom = mainCamera.ViewportToWorldPoint(Vector3.zero);
            Vector3 top = mainCamera.ViewportToWorldPoint(Vector3.one);
            Bounds bounds = spriteRenderer.bounds;
            if (bounds.max.y < bottom.y || bounds.max.x < bottom.x || bounds.min.x > top.x)
                Destroy(gameObject);
        }
    }

    // [BAO VE] Chỉ nhận Player còn sống thuộc owner; đánh dấu IsCollected, tắt Collider rồi phát Collected và hủy để tránh nhặt lặp.
    void OnTriggerEnter2D(Collider2D other)
    {
        if (IsCollected || owner == null || !owner.CanCollect || Time.timeScale <= 0f) return;

        PlayerHealth player = other.GetComponentInParent<PlayerHealth>();
        if (player == null || player != owner.Player || player.CurrentHealth <= 0) return;

        // Set the guard before invoking listeners, including multiple Player colliders.
        // [BAO VE - VAT PHAM CHAN NHAT TRUNG] Đặt cờ trước khi gọi listener và tắt Collider ngay sau đó, tránh nhiều Collider Player cùng nhận một vật phẩm.
        IsCollected = true;
        GetComponent<BoxCollider2D>().enabled = false;
        try
        {
            // [BAO VE - SU KIEN NHAT] Manager nhận Collected rồi báo PickupCollected; PlayerPowerUpEffects mới áp dụng tăng súng hoặc hồi HP.
            Collected?.Invoke(this, player);
        }
        finally
        {
            Destroy(gameObject);
        }
    }

    // [BAO VE] Báo owner bỏ vật phẩm khỏi danh sách theo dõi, kể cả khi hết thời gian hoặc rơi khỏi màn hình.
    void OnDestroy()
    {
        if (owner != null) owner.ForgetPickup(this);
    }
}
