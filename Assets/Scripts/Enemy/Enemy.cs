using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Min(1)]
    public int maxHealth = 3;

    [Min(1)]
    public int contactDamage = 1;

    [Min(0)]
    public int scoreValue = 100;

    public GameObject deathEffectPrefab;

    private int currentHealth;
    private bool hasHitPlayer;
    private bool isDead;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (isDead || hasHitPlayer)
        {
            return;
        }

        currentHealth -= damage;

        DamageFlash damageFlash = GetComponent<DamageFlash>();

        if (damage > 0 && damageFlash != null)
        {
            damageFlash.Flash();
        }

        if (currentHealth <= 0)
        {
            // Chỉ cộng một lần khi bị bắn hết HP, trước khi Destroy cuối frame.
            isDead = true;

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.AddScore(scoreValue);
            }

            if (deathEffectPrefab != null)
            {
                Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Chặn gây sát thương lần nữa trong lúc chờ Enemy bị hủy cuối frame.
        if (hasHitPlayer || isDead)
        {
            return;
        }

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            hasHitPlayer = true;
            playerHealth.TakeDamage(contactDamage);

            if (deathEffectPrefab != null)
            {
                Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
            }

            Destroy(gameObject);
        }
    }
}
