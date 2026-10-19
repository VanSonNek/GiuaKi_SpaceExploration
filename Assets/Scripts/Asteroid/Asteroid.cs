using UnityEngine;

public class Asteroid : MonoBehaviour
{
    [Min(1)]
    public int maxHealth = 2;

    [Min(1)]
    public int contactDamage = 1;

    [Min(0)]
    public int scoreValue = 50;

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

            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Chặn gây sát thương lần nữa trong lúc chờ thiên thạch bị hủy.
        if (hasHitPlayer || isDead)
        {
            return;
        }

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            hasHitPlayer = true;
            playerHealth.TakeDamage(contactDamage);
            Destroy(gameObject);
        }
    }
}
