using UnityEngine;

// [BAO VE] Điều phối kỹ năng Boss bằng chu kỳ và bộ đếm thời gian; Prefab cấu hình attackCycle, Phase và khoảng nghỉ.
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

    [Header("Optional advanced Final Boss cycles")]
    public bool useAdvancedHealthCycles;
    public AttackPattern[] phase2AttackCycle = new AttackPattern[0];
    [Min(0.3f)] public float sweepWarningTime = 0.5f;
    [Range(3, 5)] public int sweepShotCount = 5;
    [Range(10f, 40f)] public float sweepHalfAngle = 28f;
    [Min(0.2f)] public float sweepShotInterval = 0.28f;
    [Min(0.3f)] public float aimedBurstInterval = 0.35f;

    public bool IsPhase2 { get; private set; }
    public int PhaseTransitionCount { get; private set; }
    public int BurstsFired { get; private set; }
    public AttackPattern CurrentPattern => UsesAttackCycle
        ? ActiveCycle[cycleIndex % ActiveCycle.Length]
        : IsPhase2 ? phase2Pattern : attackPattern;
    public AttackPattern LastFiredPattern { get; private set; }
    public bool IsPreparingAim { get; private set; }
    public Vector3 AimedTargetPosition { get; private set; }
    public int ShotsRemainingInBurst => burstShotsRemaining;
    public int ShotsRemainingInSweep => sweepShotsRemaining;
    public bool IsPreparingSweep => preparingSweep;
    public bool WarningVisible => warningHalo != null && warningHalo.enabled;
    public AttackPattern PreparingPattern { get; private set; }

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
    private bool preparingSweep;
    private int sweepShotsRemaining;
    private int sweepTotalShots;
    private bool reverseSweep;
    private AttackPattern activeBurstPattern;
    private SpriteRenderer warningHalo;

    // [BAO VE] Advanced Final Boss chỉ hoạt động khi bật cả chu kỳ và Phase; ActiveCycle chọn mảng kỹ năng theo IsPhase2.
    bool UsesAdvancedHealthCycles => useAdvancedHealthCycles && useHealthPhases && useAttackCycle;
    AttackPattern[] ActiveCycle => UsesAdvancedHealthCycles && IsPhase2
        ? phase2AttackCycle : attackCycle;
    bool UsesAttackCycle => useAttackCycle && (!useHealthPhases || UsesAdvancedHealthCycles)
        && ActiveCycle != null && ActiveCycle.Length > 0;

    // [BAO VE] Kiểm tra Boss, BossMovement, Prefab đạn, FirePoint và tìm Player/GameResultManager cùng Scene khi dùng chu kỳ.
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

        if (UsesAdvancedHealthCycles && (attackCycle == null || attackCycle.Length == 0
            || phase2AttackCycle == null || phase2AttackCycle.Length == 0))
        {
            Debug.LogWarning("Advanced Final Boss needs both phase attack cycles.", this);
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

    // [BAO VE] Hủy đòn chờ khi Boss/Player chết hoặc trận kết thúc; Pause và IsInPosition chặn bắn khi chưa được phép.
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

        // [BAO VE - BOSS PHASE 2] Chuyển một lần khi HP tới MaxHealth * phase2HealthFraction; chế độ advanced hủy đòn chờ và reset chu kỳ.
        if (useHealthPhases && !IsPhase2
            && boss.CurrentHealth <= boss.MaxHealth * phase2HealthFraction)
        {
            IsPhase2 = true;
            PhaseTransitionCount++;
            if (UsesAdvancedHealthCycles)
            {
                // Cancel a charged shot or partial volley before starting Phase 2.
                CancelPendingAttack();
                cycleIndex = 0;
                combatStarted = true;
                nextFireTime = Time.time + Mathf.Max(0.1f, phaseTransitionDelay);
            }
            else
                nextFireTime = Mathf.Max(nextFireTime, Time.time + phaseTransitionDelay);
            DamageFlash flash = GetComponent<DamageFlash>();
            if (flash != null)
            {
                flash.Flash();
            }
        }

        if (UsesAttackCycle)
        {
            UpdateAttackCycle();
            return;
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

    // [BAO VE] Máy trạng thái ưu tiên xử lý ngắm, Sweep, Burst đang dở; nextFireTime quyết định thời điểm phát tiếp hoặc chọn kỹ năng mới.
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
            IsPreparingAim = false;
            HideWarning();
            if (PreparingPattern == AttackPattern.AimedBurst)
            {
                activeBurstPattern = AttackPattern.AimedBurst;
                // [BAO VE - BOSS AIMED BURST] Sau cảnh báo, bắn hai viên tới tọa độ đã khóa; không cập nhật lại đích của Player.
                burstShotsRemaining = 2;
                FireBurstShot();
                return;
            }

            // Lock a position while warning; later Player movement never redirects this shot.
            BossBullet.FireDirected(bossBulletPrefab, firePoint.position,
                AimedTargetPosition - firePoint.position, owner);
            // [BAO VE - BOSS AIMED SHOT] FireDirected ngay phía trên vừa bắn từ FirePoint về AimedTargetPosition đã lưu, sau đó mới chuyển kỹ năng.
            LastFiredPattern = AttackPattern.AimedShot;
            BurstsFired++;
            FinishCycleAttack();
            return;
        }

        if (preparingSweep)
        {
            preparingSweep = false;
            HideWarning();
            // [BAO VE - BOSS BAT DAU SWEEP] Sau thời gian cảnh báo, chốt tổng số viên rồi phát lần lượt qua FireSweepShot.
            sweepTotalShots = Mathf.Clamp(sweepShotCount, 3, 5);
            sweepShotsRemaining = sweepTotalShots;
            FireSweepShot();
            return;
        }

        if (sweepShotsRemaining > 0)
        {
            FireSweepShot();
            return;
        }

        if (burstShotsRemaining > 0)
        {
            FireBurstShot();
            return;
        }

        if (CurrentPattern == AttackPattern.AimedShot || CurrentPattern == AttackPattern.AimedBurst)
        {
            // [BAO VE] Khóa tọa độ Player lúc bắt đầu cảnh báo; AimedShot/AimedBurst bắn tới tọa độ đã lưu nên đạn không homing.
            AimedTargetPosition = targetPlayer.transform.position;
            PreparingPattern = CurrentPattern;
            IsPreparingAim = true;
            nextFireTime = Time.time + Mathf.Max(0.1f, aimedShotWarningTime);
            if (UsesAdvancedHealthCycles)
                ShowWarning(false);
            else
            {
                DamageFlash flash = GetComponent<DamageFlash>();
                if (flash != null) flash.Flash();
            }
        }
        else if (CurrentPattern == AttackPattern.SweepShot)
        {
            PreparingPattern = AttackPattern.SweepShot;
            preparingSweep = true;
            ShowWarning(true);
            nextFireTime = Time.time + Mathf.Max(0.3f, sweepWarningTime);
        }
        else if (CurrentPattern == AttackPattern.BurstShot)
        {
            activeBurstPattern = AttackPattern.BurstShot;
            // [BAO VE - BOSS BURST SHOT] Burst thường gồm 2 hoặc 3 viên theo cấu hình; FireBurstShot giãn các viên bằng burstShotInterval.
            burstShotsRemaining = Mathf.Clamp(burstShotCount, 2, 3);
            FireBurstShot();
        }
        else
        {
            BossBullet.FirePattern(bossBulletPrefab, firePoint.position, CurrentPattern,
                UsesAdvancedHealthCycles && IsPhase2 ? phase2SpreadAngle : spreadAngle,
                -1f, owner, true);
            LastFiredPattern = CurrentPattern;
            BurstsFired++;
            FinishCycleAttack();
        }
    }

    // [BAO VE] Bắn từng viên theo bộ đếm burstShotsRemaining; AimedBurst dùng chung đích đã khóa, BurstShot thường bắn thẳng.
    void FireBurstShot()
    {
        if (activeBurstPattern == AttackPattern.AimedBurst)
            BossBullet.FireDirected(bossBulletPrefab, firePoint.position,
                AimedTargetPosition - firePoint.position, owner);
        else
            BossBullet.FirePattern(bossBulletPrefab, firePoint.position, AttackPattern.SingleShot,
                spreadAngle, -1f, owner, true);
        LastFiredPattern = activeBurstPattern;
        BurstsFired++;
        burstShotsRemaining--;
        if (burstShotsRemaining > 0)
            nextFireTime = Time.time + (activeBurstPattern == AttackPattern.AimedBurst
                ? Mathf.Max(0.3f, aimedBurstInterval) : Mathf.Max(0.15f, burstShotInterval));
        else
            FinishCycleAttack();
    }

    // [BAO VE] Lerp góc từ -sweepHalfAngle tới +sweepHalfAngle, bắn từng viên cách nhau sweepShotInterval và đảo chiều ở lượt quét sau.
    void FireSweepShot()
    {
        float angle = Mathf.Lerp(-Mathf.Clamp(sweepHalfAngle, 10f, 40f),
            Mathf.Clamp(sweepHalfAngle, 10f, 40f),
            (sweepTotalShots - sweepShotsRemaining) / (float)(sweepTotalShots - 1));
        if (reverseSweep) angle = -angle;
        BossBullet.FireDirected(bossBulletPrefab, firePoint.position,
            Quaternion.Euler(0f, 0f, angle) * Vector2.down, owner);
        LastFiredPattern = AttackPattern.SweepShot;
        BurstsFired++;
        sweepShotsRemaining--;
        if (sweepShotsRemaining > 0)
            nextFireTime = Time.time + Mathf.Max(0.2f, sweepShotInterval);
        else
        {
            reverseSweep = !reverseSweep;
            FinishCycleAttack();
        }
    }

    // [BAO VE] Tăng cycleIndex theo modulo độ dài chu kỳ; đặt thời gian hồi theo fireRate hoặc phase2FireRate.
    void FinishCycleAttack()
    {
        cycleIndex = (cycleIndex + 1) % ActiveCycle.Length;
        nextFireTime = Time.time + Mathf.Max(0.1f,
            UsesAdvancedHealthCycles && IsPhase2 ? phase2FireRate : fireRate);
    }

    // [BAO VE] Tạo và tái sử dụng viền Sprite phía sau thân Boss: màu vàng khi ngắm, hồng khi quét; không đổi màu thân của DamageFlash.
    void ShowWarning(bool sweep)
    {
        if (warningHalo == null)
        {
            SpriteRenderer body = GetComponent<SpriteRenderer>();
            if (body == null || body.sprite == null) return;
            GameObject halo = new GameObject("AttackWarningHalo");
            halo.transform.SetParent(transform, false);
            halo.transform.localScale = Vector3.one * 1.08f;
            warningHalo = halo.AddComponent<SpriteRenderer>();
            warningHalo.sprite = body.sprite;
            warningHalo.sharedMaterial = body.sharedMaterial;
            warningHalo.sortingLayerID = body.sortingLayerID;
            warningHalo.sortingOrder = body.sortingOrder - 1;
        }
        // Draw behind the hull: DamageFlash retains sole ownership of its color.
        warningHalo.color = sweep ? new Color(1f, 0.15f, 0.8f, 0.9f)
            : new Color(1f, 0.75f, 0.15f, 0.85f);
        warningHalo.enabled = true;
    }

    void HideWarning()
    {
        if (warningHalo != null) warningHalo.enabled = false;
    }

    // [BAO VE] Xóa trạng thái ngắm và bộ đếm Burst/Sweep, đồng thời ẩn cảnh báo để không còn phát bắn chờ sau khi bị hủy.
    void CancelPendingAttack()
    {
        IsPreparingAim = false;
        burstShotsRemaining = 0;
        preparingSweep = false;
        sweepShotsRemaining = 0;
        HideWarning();
    }

    // [BAO VE] Hủy đòn chờ và reset tiến trình bắn khi component tắt; không để kỹ năng tiếp tục trên Boss không còn hoạt động.
    void OnDisable()
    {
        CancelPendingAttack();
        combatStarted = false;
        cycleIndex = 0;
        reverseSweep = false;
    }
}
