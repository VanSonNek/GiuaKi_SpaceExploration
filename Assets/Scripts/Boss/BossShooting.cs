using UnityEngine;

public class BossShooting : MonoBehaviour
{
    public GameObject bossBulletPrefab;
    public Transform firePoint;

    [Min(0.01f)]
    public float fireRate = 1.2f;

    public AttackPattern attackPattern = AttackPattern.SingleShot;
    [Range(1f, 45f)] public float spreadAngle = 22f;

    [Header("Optional health phase (Final Boss only)")]
    public bool useHealthPhases;
    [Range(0.1f, 0.9f)] public float phase2HealthFraction = 0.5f;
    public AttackPattern phase2Pattern = AttackPattern.SpreadShot;
    [Min(0.1f)] public float phase2FireRate = 1.8f;
    [Range(1f, 45f)] public float phase2SpreadAngle = 26f;
    [Min(0f)] public float phaseTransitionDelay = 0.35f;

    public bool IsPhase2 { get; private set; }
    public int PhaseTransitionCount { get; private set; }
    public int BurstsFired { get; private set; }
    public AttackPattern CurrentPattern => IsPhase2 ? phase2Pattern : attackPattern;

    private Boss boss;
    private BossMovement bossMovement;
    private Collider2D owner;
    private float nextFireTime;

    void Start()
    {
        boss = GetComponent<Boss>();
        bossMovement = GetComponent<BossMovement>();
        owner = GetComponent<Collider2D>();

        if (boss == null || bossMovement == null || bossBulletPrefab == null || firePoint == null
            || bossBulletPrefab.GetComponent<BossBullet>() == null)
        {
            Debug.LogWarning("Boss cần Boss, BossMovement, Boss Bullet Prefab và Fire Point.", this);
            enabled = false;
            return;
        }
    }

    void Update()
    {
        if (Time.timeScale <= 0f || boss == null || boss.IsDead
            || bossMovement == null || !bossMovement.enabled
            || !bossMovement.IsInPosition || bossBulletPrefab == null || firePoint == null)
        {
            return;
        }

        if (useHealthPhases && !IsPhase2
            && boss.CurrentHealth <= boss.MaxHealth * phase2HealthFraction)
        {
            IsPhase2 = true;
            PhaseTransitionCount++;
            nextFireTime = Mathf.Max(nextFireTime, Time.time + phaseTransitionDelay);
            DamageFlash flash = GetComponent<DamageFlash>();
            if (flash != null)
            {
                flash.Flash();
            }
        }

        if (Time.time >= nextFireTime)
        {
            // Preserve Level 1's original speed, origin and lifetime.
            bool advancedAttack = useHealthPhases || attackPattern != AttackPattern.SingleShot;
            BossBullet.FirePattern(bossBulletPrefab, firePoint.position, CurrentPattern,
                IsPhase2 ? phase2SpreadAngle : spreadAngle, -1f,
                advancedAttack ? owner : null, advancedAttack);
            BurstsFired++;
            nextFireTime = Time.time + Mathf.Max(0.1f, IsPhase2 ? phase2FireRate : fireRate);
        }
    }
}
