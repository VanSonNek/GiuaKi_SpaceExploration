using UnityEngine;

public class BossShooting : MonoBehaviour
{
    public GameObject bossBulletPrefab;
    public Transform firePoint;

    [Min(0.01f)]
    public float fireRate = 1.2f;

    public AttackPattern attackPattern = AttackPattern.SingleShot;
    [Range(1f, 45f)] public float spreadAngle = 22f;

    [Header("Optional attack cycle (Level 1 and Level 2)")]
    public bool useAttackCycle;
    public AttackPattern[] attackCycle = new AttackPattern[0];
    [Min(0.1f)] public float initialAttackDelay = 0.35f;
    [Min(0.1f)] public float aimedShotWarningTime = 0.5f;
    [Range(2, 3)] public int burstShotCount = 3;
    [Min(0.15f)] public float burstShotInterval = 0.3f;

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
    public AttackPattern CurrentPattern => UsesAttackCycle
        ? attackCycle[cycleIndex % attackCycle.Length]
        : IsPhase2 ? phase2Pattern : attackPattern;
    public AttackPattern LastFiredPattern { get; private set; }
    public bool IsPreparingAim { get; private set; }
    public Vector3 AimedTargetPosition { get; private set; }
    public int ShotsRemainingInBurst => burstShotsRemaining;

    private Boss boss;
    private BossMovement bossMovement;
    private Collider2D owner;
    private float nextFireTime;
    private PlayerHealth targetPlayer;
    private GameResultManager resultManager;
    private int cycleIndex;
    private int burstShotsRemaining;
    private bool combatStarted;
    private bool cycleStopped;

    // Health phases retain their original path, including inherited prefab settings.
    bool UsesAttackCycle => useAttackCycle && !useHealthPhases
        && attackCycle != null && attackCycle.Length > 0;

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

        if (UsesAttackCycle)
        {
            foreach (PlayerHealth player in FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None))
                if (player.gameObject.scene == gameObject.scene) { targetPlayer = player; break; }
            foreach (GameResultManager result in FindObjectsByType<GameResultManager>(FindObjectsSortMode.None))
                if (result.gameObject.scene == gameObject.scene) { resultManager = result; break; }

            if (targetPlayer == null)
            {
                Debug.LogWarning("Boss attack cycle needs a Player in the same Scene.", this);
                enabled = false;
            }
        }
    }

    void Update()
    {
        if (UsesAttackCycle && (cycleStopped || boss == null || boss.IsDead
            || targetPlayer == null || targetPlayer.CurrentHealth <= 0
            || (resultManager != null && resultManager.IsGameFinished)))
        {
            cycleStopped = true;
            CancelPendingAttack();
            return;
        }

        if (Time.timeScale <= 0f || boss == null || boss.IsDead
            || bossMovement == null || !bossMovement.enabled
            || !bossMovement.IsInPosition || bossBulletPrefab == null || firePoint == null)
        {
            return;
        }

        if (UsesAttackCycle)
        {
            UpdateAttackCycle();
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
            LastFiredPattern = CurrentPattern;
            nextFireTime = Time.time + Mathf.Max(0.1f, IsPhase2 ? phase2FireRate : fireRate);
        }
    }

    void UpdateAttackCycle()
    {
        if (!combatStarted)
        {
            combatStarted = true;
            nextFireTime = Time.time + Mathf.Max(0.1f, initialAttackDelay);
            return;
        }
        if (Time.time < nextFireTime) return;

        if (IsPreparingAim)
        {
            // Lock a position while warning; later Player movement never redirects this shot.
            BossBullet.FireDirected(bossBulletPrefab, firePoint.position,
                AimedTargetPosition - firePoint.position, owner);
            LastFiredPattern = AttackPattern.AimedShot;
            BurstsFired++;
            IsPreparingAim = false;
            FinishCycleAttack();
            return;
        }

        if (burstShotsRemaining > 0)
        {
            FireBurstShot();
            return;
        }

        if (CurrentPattern == AttackPattern.AimedShot)
        {
            AimedTargetPosition = targetPlayer.transform.position;
            IsPreparingAim = true;
            nextFireTime = Time.time + Mathf.Max(0.1f, aimedShotWarningTime);
            DamageFlash flash = GetComponent<DamageFlash>();
            if (flash != null) flash.Flash();
        }
        else if (CurrentPattern == AttackPattern.BurstShot)
        {
            burstShotsRemaining = Mathf.Clamp(burstShotCount, 2, 3);
            FireBurstShot();
        }
        else
        {
            BossBullet.FirePattern(bossBulletPrefab, firePoint.position, CurrentPattern,
                spreadAngle, -1f, owner, true);
            LastFiredPattern = CurrentPattern;
            BurstsFired++;
            FinishCycleAttack();
        }
    }

    void FireBurstShot()
    {
        // One straight projectile per short burst, not repeated three-way spreads.
        BossBullet.FirePattern(bossBulletPrefab, firePoint.position, AttackPattern.SingleShot,
            spreadAngle, -1f, owner, true);
        LastFiredPattern = AttackPattern.BurstShot;
        BurstsFired++;
        burstShotsRemaining--;
        if (burstShotsRemaining > 0)
            nextFireTime = Time.time + Mathf.Max(0.15f, burstShotInterval);
        else
            FinishCycleAttack();
    }

    void FinishCycleAttack()
    {
        cycleIndex = (cycleIndex + 1) % attackCycle.Length;
        nextFireTime = Time.time + Mathf.Max(0.1f, fireRate);
    }

    void CancelPendingAttack()
    {
        IsPreparingAim = false;
        burstShotsRemaining = 0;
    }

    void OnDisable()
    {
        CancelPendingAttack();
        combatStarted = false;
        cycleIndex = 0;
    }
}
