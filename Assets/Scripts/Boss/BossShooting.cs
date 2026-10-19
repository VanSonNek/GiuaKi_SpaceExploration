using UnityEngine;

public class BossShooting : MonoBehaviour
{
    public GameObject bossBulletPrefab;
    public Transform firePoint;

    [Min(0.01f)]
    public float fireRate = 1.2f;

    private Boss boss;
    private BossMovement bossMovement;
    private float nextFireTime;

    void Start()
    {
        boss = GetComponent<Boss>();
        bossMovement = GetComponent<BossMovement>();

        if (boss == null || bossMovement == null || bossBulletPrefab == null || firePoint == null)
        {
            Debug.LogWarning("Boss cần Boss, BossMovement, Boss Bullet Prefab và Fire Point.", this);
            enabled = false;
            return;
        }
    }

    void Update()
    {
        if (boss == null || boss.IsDead || bossMovement == null || !bossMovement.enabled
            || !bossMovement.IsInPosition || bossBulletPrefab == null || firePoint == null)
        {
            return;
        }

        if (Time.time >= nextFireTime)
        {
            // BossBullet tự bay xuống theo trục Y của thế giới.
            Instantiate(bossBulletPrefab, firePoint.position, Quaternion.identity);
            nextFireTime = Time.time + fireRate;
        }
    }
}
