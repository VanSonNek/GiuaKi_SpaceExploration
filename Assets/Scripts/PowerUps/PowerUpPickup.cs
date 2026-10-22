using System;
using UnityEngine;

public enum PowerUpType
{
    WeaponUpgrade,
    HealthPickup
}

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D), typeof(Rigidbody2D))]
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

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        // Scaled lifetime is also a fallback if the camera disappears.
        Destroy(gameObject, maximumLifetime);
    }

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

    void OnTriggerEnter2D(Collider2D other)
    {
        if (IsCollected || owner == null || !owner.CanCollect || Time.timeScale <= 0f) return;

        PlayerHealth player = other.GetComponentInParent<PlayerHealth>();
        if (player == null || player != owner.Player || player.CurrentHealth <= 0) return;

        // Set the guard before invoking listeners, including multiple Player colliders.
        IsCollected = true;
        GetComponent<BoxCollider2D>().enabled = false;
        try
        {
            // TASK 08F-1 only announces collection. No weapon/health effect is applied.
            Collected?.Invoke(this, player);
        }
        finally
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if (owner != null) owner.ForgetPickup(this);
    }
}
