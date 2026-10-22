using UnityEngine;

public enum AttackPattern
{
    SingleShot = 0,
    SpreadShot = 1,
    AimedShot = 2,
    BurstShot = 3
}

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

    public static void FirePattern(GameObject prefab, Vector3 position, AttackPattern pattern,
        float spreadAngle, float speedOverride = -1f, Collider2D owner = null,
        bool cleanupOutsideView = false)
    {
        Spawn(prefab, position, 0f, speedOverride, owner, cleanupOutsideView);
        if (pattern == AttackPattern.SpreadShot)
        {
            Spawn(prefab, position, -spreadAngle, speedOverride, owner, cleanupOutsideView);
            Spawn(prefab, position, spreadAngle, speedOverride, owner, cleanupOutsideView);
        }
    }

    public static void FireDirected(GameObject prefab, Vector3 position, Vector2 direction,
        Collider2D owner = null, bool cleanupOutsideView = true)
    {
        if (direction.sqrMagnitude < 0.0001f) direction = Vector2.down;
        float angle = Mathf.Atan2(direction.x, -direction.y) * Mathf.Rad2Deg;
        Spawn(prefab, position, angle, -1f, owner, cleanupOutsideView);
    }

    static void Spawn(GameObject prefab, Vector3 position, float angle, float speedOverride,
        Collider2D owner, bool cleanupOutsideView)
    {
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);
        GameObject instance = Instantiate(prefab, position, rotation);
        BossBullet bullet = instance.GetComponent<BossBullet>();
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
