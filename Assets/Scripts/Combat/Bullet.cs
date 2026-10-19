using UnityEngine;

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

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // Bay thẳng lên theo trục Y của thế giới.
        transform.position += Vector3.up * speed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Destroy có hiệu lực cuối frame, nên chặn đạn gây sát thương lần nữa.
        if (hasHit)
        {
            return;
        }

        Enemy enemy = other.GetComponent<Enemy>();

        if (enemy != null)
        {
            hasHit = true;
            enemy.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        Asteroid asteroid = other.GetComponent<Asteroid>();

        if (asteroid != null)
        {
            hasHit = true;
            asteroid.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        Boss boss = other.GetComponent<Boss>();

        if (boss != null)
        {
            hasHit = true;
            boss.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
