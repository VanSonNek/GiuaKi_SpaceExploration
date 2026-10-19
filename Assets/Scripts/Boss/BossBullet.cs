using UnityEngine;

public class BossBullet : MonoBehaviour
{
    [Min(0f)]
    public float speed = 5f;

    [Min(1)]
    public int damage = 1;

    [Min(0.01f)]
    public float lifeTime = 5f;

    private bool hasHit;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.position += Vector3.down * speed * Time.deltaTime;
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
