using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
// [BAO VE] Quản lý rơi vật phẩm theo lịch Wave hoặc Enemy bị bắn chết; PickupCollected báo cho PlayerPowerUpEffects áp dụng hiệu ứng.
public class PowerUpDropManager : MonoBehaviour
{
    [Serializable]
    public class WaveDrop
    {
        [Min(1)] public int waveNumber = 1;
        public PowerUpType type;
        [Min(0f)] public float delay = 3f;
        [Range(0f, 1f)] public float viewportX = 0.5f;
        public bool onlyWhenHealthMissing;
    }

    [SerializeField] private Level1Controller levelController;
    [SerializeField] private GameResultManager resultManager;
    [SerializeField] private PlayerHealth player;
    [SerializeField] private Camera gameplayCamera;
    [SerializeField] private PowerUpPickup weaponUpgradePrefab;
    [SerializeField] private PowerUpPickup healthPickupPrefab;
    [SerializeField, Range(0f, 1f)] private float weaponDropChance = 0.08f;
    [SerializeField, Range(0f, 1f)] private float healthDropChance = 0.05f;
    [SerializeField, Min(0)] private int maxRandomDropsPerWave = 1;
    [SerializeField] private bool suppressRandomInScheduledWaves = true;
    [SerializeField] private WaveDrop[] scheduledDrops = new WaveDrop[0];

    public PlayerHealth Player => player;
    public int CurrentWave { get; private set; }
    public int RandomDropsThisWave { get; private set; }
    public int RandomDropsSpawned { get; private set; }
    public int ScheduledDropsSpawned { get; private set; }
    public int WeaponUpgradesCollected { get; private set; }
    public int HealthPickupsCollected { get; private set; }
    public PowerUpType? LastCollectedType { get; private set; }
    public event Action<PowerUpPickup> PickupSpawned;
    public event Action<PowerUpType, PlayerHealth> PickupCollected;

    // [BAO VE - POWER UP DIEU KIEN NHAT] Chỉ nhận khi đang Play, manager hoạt động, Player còn sống, trận chưa xong và chuỗi Level chưa bị hủy.
    public bool CanCollect => Application.isPlaying && isActiveAndEnabled && !ended
        && player != null && player.CurrentHealth > 0
        && resultManager != null && !resultManager.IsGameFinished
        && levelController != null && !levelController.IsSequenceCancelled;

    public int ActivePickupCount
    {
        get
        {
            activePickups.RemoveWhere(pickup => pickup == null);
            return activePickups.Count;
        }
    }

    // Keep loot rolls independent of Unity's movement/shooting random sequence.
    private readonly System.Random dropRandom = new System.Random();
    private readonly HashSet<int> observedWaves = new HashSet<int>();
    private readonly HashSet<PowerUpPickup> activePickups = new HashSet<PowerUpPickup>();
    private bool ended;
    private bool waveOpen;
    private bool hasScheduledDrop;

    // [BAO VE] Đăng ký Enemy.Defeated, WaveStarted, BossWarningStarted và Player.Died; OnDisable gỡ các đăng ký này.
    void OnEnable()
    {
        if (!Application.isPlaying) return;
        if (gameplayCamera == null) gameplayCamera = Camera.main;
        if (levelController == null || resultManager == null || player == null
            || gameplayCamera == null || weaponUpgradePrefab == null || healthPickupPrefab == null)
        {
            Debug.LogWarning("PowerUpDropManager needs controller, result, Player, camera and both pickup prefabs.", this);
            enabled = false;
            return;
        }

        Enemy.Defeated += OnEnemyDefeated;
        levelController.WaveStarted += OnWaveStarted;
        levelController.BossWarningStarted += OnBossWarning;
        player.Died += EndDrops;
    }

    void Update()
    {
        if (!ended && resultManager != null && resultManager.IsGameFinished) EndDrops();
    }

    // [BAO VE] Mỗi Wave chỉ nhận một lần nhờ observedWaves; reset hạn mức rơi ngẫu nhiên và khởi chạy các lịch scheduledDrops phù hợp.
    void OnWaveStarted(int number, int total, string waveName)
    {
        if (ended || (resultManager != null && resultManager.IsGameFinished)
            || number < 1 || !observedWaves.Add(number)) return;

        StopAllCoroutines();
        CurrentWave = number;
        RandomDropsThisWave = 0;
        waveOpen = true;
        hasScheduledDrop = false;
        if (scheduledDrops == null) return;

        // [BAO VE - LICH VAT PHAM THEO WAVE] Mỗi cấu hình trùng waveNumber tạo một lịch chờ; chỉ sinh nếu các kiểm tra trong DropForWave vẫn hợp lệ.
        foreach (WaveDrop drop in scheduledDrops)
        {
            if (drop == null || drop.waveNumber != number) continue;
            hasScheduledDrop = true;
            StartCoroutine(DropForWave(drop, number));
        }
    }

    // [BAO VE] Chờ delay theo thời gian game, kiểm tra vẫn đúng Wave; Health có thể chỉ xuất hiện khi Player thiếu HP.
    IEnumerator DropForWave(WaveDrop drop, int wave)
    {
        yield return new WaitForSeconds(Mathf.Max(0f, drop.delay));
        if (!CanSpawn() || CurrentWave != wave) yield break;
        if (drop.onlyWhenHealthMissing && player.CurrentHealth >= player.MaxHealth) yield break;

        PowerUpPickup prefab = GetPrefab(drop.type);
        Vector3 position;
        if (TryGetScriptedPosition(prefab, drop.viewportX, out position) && Spawn(prefab, position))
            ScheduledDropsSpawned++;
    }

    // [BAO VE] Chỉ xét Enemy cùng Scene; dùng xác suất và maxRandomDropsPerWave, đồng thời kiểm tra vị trí trong Camera và không chồng Player.
    void OnEnemyDefeated(Enemy enemy)
    {
        // [BAO VE - POWER UP CHAN ROI] Không xét rơi khi hết Wave/trận, sai Scene, đủ hạn mức hoặc Wave có lịch đang chặn rơi ngẫu nhiên.
        if (!CanSpawn() || enemy == null || enemy.gameObject.scene != gameObject.scene
            || RandomDropsThisWave >= maxRandomDropsPerWave
            || (suppressRandomInScheduledWaves && hasScheduledDrop)) return;

        float weaponChance = Mathf.Clamp01(weaponDropChance);
        float healthChance = Mathf.Min(Mathf.Clamp01(healthDropChance), 1f - weaponChance);
        // [BAO VE - XAC SUAT ROI VAT PHAM] Bốc một số trong [0, 1); khoảng đầu dành cho súng, khoảng kế cho máu, còn lại không rơi.
        double roll = dropRandom.NextDouble();
        PowerUpPickup prefab;
        if (roll < weaponChance) prefab = weaponUpgradePrefab;
        else if (roll < weaponChance + healthChance) prefab = healthPickupPrefab;
        else return;

        Vector3 position = enemy.transform.position;
        Vector3 viewport = gameplayCamera.WorldToViewportPoint(position);
        if (viewport.x < 0f || viewport.x > 1f || viewport.y < 0f || viewport.y > 1f
            || !IsClearOfPlayer(prefab, position)) return;

        if (Spawn(prefab, position))
        {
            RandomDropsThisWave++;
            RandomDropsSpawned++;
        }
    }

    bool CanSpawn()
    {
        return CanCollect && waveOpen && gameplayCamera != null && Time.timeScale > 0f;
    }

    // [BAO VE - CHON PREFAB VAT PHAM] HealthPickup chọn healthPickupPrefab, loại còn lại chọn weaponUpgradePrefab; hai đường sinh dùng chung Spawn.
    PowerUpPickup GetPrefab(PowerUpType type)
    {
        return type == PowerUpType.HealthPickup ? healthPickupPrefab : weaponUpgradePrefab;
    }

    // [BAO VE] Chọn vị trí gần phía trên vùng chơi theo viewportX; thử các vị trí khác nếu điểm dự kiến không an toàn cho Player.
    bool TryGetScriptedPosition(PowerUpPickup prefab, float preferredX, out Vector3 position)
    {
        position = Vector3.zero;
        if (prefab == null || gameplayCamera == null) return false;
        Vector3 bottom = gameplayCamera.ViewportToWorldPoint(Vector3.zero);
        Vector3 top = gameplayCamera.ViewportToWorldPoint(Vector3.one);
        Vector3 size = PickupSize(prefab);
        float minX = bottom.x + size.x * 0.5f + 0.15f;
        float maxX = top.x - size.x * 0.5f - 0.15f;
        if (minX > maxX || top.y - bottom.y < size.y + 0.3f) return false;
        // Start below the top HUD while still entering from the upper play area.
        float spawnY = Mathf.Min(top.y - size.y * 0.5f - 0.15f,
            Mathf.Lerp(bottom.y, top.y, 0.86f));

        foreach (float x in new[] { Mathf.Clamp01(preferredX), 0.25f, 0.75f, 0.1f, 0.9f })
        {
            position = new Vector3(Mathf.Lerp(minX, maxX, x), spawnY, 0f);
            if (IsClearOfPlayer(prefab, position)) return true;
        }
        return false;
    }

    static Vector3 PickupSize(PowerUpPickup prefab)
    {
        SpriteRenderer renderer = prefab.GetComponent<SpriteRenderer>();
        return Vector3.Scale(renderer.sprite.bounds.size, prefab.transform.localScale);
    }

    bool IsClearOfPlayer(PowerUpPickup prefab, Vector3 position)
    {
        if (prefab == null || player == null) return false;
        Bounds playerBounds;
        BoxCollider2D box = player.GetComponent<BoxCollider2D>();
        if (box != null)
            playerBounds = new Bounds(player.transform.TransformPoint(box.offset),
                Vector3.Scale(box.size, player.transform.lossyScale));
        else
            playerBounds = new Bounds(player.transform.position, Vector3.one);
        // Use XY even though 2D collider bounds have zero depth.
        Vector3 half = PickupSize(prefab) * 0.5f;
        return Mathf.Abs(position.x - playerBounds.center.x) > playerBounds.extents.x + half.x + 0.2f
            || Mathf.Abs(position.y - playerBounds.center.y) > playerBounds.extents.y + half.y + 0.2f;
    }

    // [BAO VE] Instantiate Pickup, đưa vào đúng Scene, gán owner/Camera và thêm vào activePickups để quản lý vòng đời.
    bool Spawn(PowerUpPickup prefab, Vector3 position)
    {
        if (prefab == null || !CanSpawn()) return false;
        // [BAO VE - SINH VAT PHAM] Dòng tạo WeaponUpgradePickup hoặc HealthPickup thực tế; prefab đã được chọn từ loại hoặc kết quả xác suất.
        PowerUpPickup pickup = Instantiate(prefab, position, Quaternion.identity);
        // Own the pickup in this Scene even when another Scene is active additively.
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(pickup.gameObject, gameObject.scene);
        pickup.Initialize(this, gameplayCamera);
        activePickups.Add(pickup);
        pickup.Collected += OnCollected;
        PickupSpawned?.Invoke(pickup);
        return true;
    }

    // [BAO VE] Remove khỏi HashSet trước khi phát PickupCollected để một vật phẩm chỉ được tính một lần; hiệu ứng súng/HP do bên nghe xử lý.
    void OnCollected(PowerUpPickup pickup, PlayerHealth collector)
    {
        if (!activePickups.Remove(pickup)) return;
        pickup.Collected -= OnCollected;
        LastCollectedType = pickup.Type;
        if (pickup.Type == PowerUpType.WeaponUpgrade) WeaponUpgradesCollected++;
        else HealthPickupsCollected++;
        PickupCollected?.Invoke(pickup.Type, collector);
    }

    internal void ForgetPickup(PowerUpPickup pickup)
    {
        activePickups.Remove(pickup);
        pickup.Collected -= OnCollected;
    }

    // [BAO VE] Đóng lịch rơi của Wave và dừng coroutine chờ khi bắt đầu cảnh báo Boss; không xóa các Pickup đã sinh.
    void OnBossWarning(float duration)
    {
        waveOpen = false;
        StopAllCoroutines();
    }

    // [BAO VE] Khi Player chết hoặc trận kết thúc thì khóa sinh/nhặt mới và dừng các lịch rơi đang chờ.
    void EndDrops()
    {
        ended = true;
        waveOpen = false;
        StopAllCoroutines();
    }

    // [BAO VE] Gỡ sự kiện, đặc biệt Enemy.Defeated là static, để Retry/chuyển Scene không giữ listener của Scene cũ.
    void OnDisable()
    {
        Enemy.Defeated -= OnEnemyDefeated;
        if (levelController != null)
        {
            levelController.WaveStarted -= OnWaveStarted;
            levelController.BossWarningStarted -= OnBossWarning;
        }
        if (player != null) player.Died -= EndDrops;
        waveOpen = false;
        StopAllCoroutines();
    }
}
