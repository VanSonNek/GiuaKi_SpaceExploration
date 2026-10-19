using System;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Min(1)]
    public int maxHealth = 3;

    private int currentHealth;
    private bool isDead;

    public event Action Died;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
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
            // Chỉ báo chết một lần, trước khi Player bị hủy cuối frame.
            isDead = true;
            Died?.Invoke();
            Destroy(gameObject);
        }
    }
}
