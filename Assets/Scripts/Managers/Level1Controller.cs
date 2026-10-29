using System.Collections;
using UnityEngine;

// [BAO VE] Điều phối Wave và Boss dùng chung cho các Level; tên class giữ từ Level 1, customWaves chứa kịch bản theo từng Scene.
public class Level1Controller : MonoBehaviour
{
    public EnemySpawner enemySpawner;
    public AsteroidSpawner asteroidSpawner;
    public EnemyFormationSpawner formationSpawner;
    public GameObject bossPrefab;
    public GameResultManager gameResultManager;

    [System.Serializable]
    // [BAO VE] Một Wave cấu hình Enemy, Asteroid, các nhóm Formation và thời gian nghỉ; dữ liệu được gán trong Inspector.
    public class WaveDefinition
    {
        public string name;
        [Min(0)] public int enemyCount;
        [Min(0.01f)] public float enemySpawnInterval = 1f;
        [Min(0)] public int asteroidCount;
        [Min(0.01f)] public float asteroidSpawnInterval = 1f;
        [Min(0f)] public float delayAfterWave = 2f;
        public EnemyFormationSettings[] formationGroups = new EnemyFormationSettings[0];
        [Min(0f)] public float formationGroupDelay = 0.7f;
        public bool useAsteroidCorridor;
        [Min(0.1f)] public float corridorGroupInterval = 2.6f;
        [Min(0.1f)] public float corridorWidth = 2.2f;
        public float[] corridorCenters = new float[] { 0.3f, 0.7f, 0.5f };
    }

    [Header("Custom Waves (off keeps the original Level 1 sequence)")]
    [SerializeField] private bool useCustomWaves;
    // [BAO VE - DU LIEU WAVE] Độ dài customWaves là số Wave của Scene; Enemy thường dùng enemyCount, đội hình dùng enemyCount trong từng formationGroups.
    [SerializeField] private WaveDefinition[] customWaves = new WaveDefinition[0];

    private bool sequenceStarted;
    private bool customWavesComplete;
    private int currentCustomWaveIndex = -1;

    public int CurrentCustomWaveNumber => currentCustomWaveIndex + 1;
    public Boss CurrentBoss { get; private set; }
    public event System.Action<Boss> BossSpawned;
    public event System.Action<int, int, string> WaveStarted;
    public event System.Action<float> BossWarningStarted;
    public int WaveStartedCount { get; private set; }
    public int BossWarningCount { get; private set; }

    private GameObject spawnedBoss;
    private bool formationGroupsComplete;
    private bool sequenceCancelled;
    public bool IsSequenceCancelled => sequenceCancelled;
    bool IsFinished => gameResultManager != null && gameResultManager.IsGameFinished;

    static bool HasFormations(WaveDefinition wave)
    {
        return wave.formationGroups != null && wave.formationGroups.Length > 0;
    }

    // Unity tự chạy Start dạng Coroutine khi game bắt đầu.
    // [BAO VE] Chạy chuỗi Wave một lần: useCustomWaves chọn kịch bản Scene, ngược lại dùng trình tự Level 1 cũ.
    IEnumerator Start()
    {
        if (sequenceStarted)
        {
            yield break;
        }

        sequenceStarted = true;

        if (useCustomWaves)
        {
            if (!ValidateCustomWaves())
            {
                yield break;
            }

            yield return RunCustomWaves();

            if (!customWavesComplete)
            {
                yield break;
            }
        }
        else
        {
            if (enemySpawner == null || asteroidSpawner == null)
            {
                Debug.LogWarning("Hãy gán Enemy Spawner và Asteroid Spawner cho Level1Controller.", this);
                yield break;
            }

            // Wave 1: 4 Enemy, giữ khoảng cách sinh hiện tại trong Inspector.
            enemySpawner.StartEnemyWave(4, enemySpawner.spawnInterval);

            if (enemySpawner.IsWaveRunning)
            {
                NotifyWaveStarted(1, 3, "ENEMY ATTACK");
            }

            // Chờ wave kết thúc. Nếu spawner bị hủy hoặc dừng thì không chuyển tiếp.
            yield return new WaitUntil(() => enemySpawner == null || !enemySpawner.IsWaveRunning);

            if (enemySpawner == null || !enemySpawner.IsWaveComplete)
            {
                yield break;
            }

            yield return new WaitForSeconds(2f);

            if (asteroidSpawner == null)
            {
                yield break;
            }

            // Asteroid Section: 6 thiên thạch, giữ khoảng cách sinh trong Inspector.
            asteroidSpawner.StartAsteroidWave(6, asteroidSpawner.spawnInterval);

            if (asteroidSpawner.IsWaveRunning)
            {
                NotifyWaveStarted(2, 3, "ASTEROID STORM");
            }

            yield return new WaitUntil(() => asteroidSpawner == null || !asteroidSpawner.IsWaveRunning);

            if (asteroidSpawner == null || !asteroidSpawner.IsWaveComplete)
            {
                yield break;
            }

            yield return new WaitForSeconds(2f);

            if (enemySpawner == null || asteroidSpawner == null)
            {
                yield break;
            }

            // Trình tự cũ, Wave 3: cả hai Coroutine bắt đầu cùng giai đoạn.
            enemySpawner.StartEnemyWave(6, 1.2f);
            asteroidSpawner.StartAsteroidWave(3, 2.2f);

            if (enemySpawner.IsWaveRunning && asteroidSpawner.IsWaveRunning)
            {
                NotifyWaveStarted(3, 3, "COMBINED ASSAULT");
            }

            // Chờ cả hai spawner của Wave 3 trong trình tự cũ kết thúc.
            yield return new WaitUntil(() =>
                enemySpawner == null || asteroidSpawner == null ||
                (!enemySpawner.IsWaveRunning && !asteroidSpawner.IsWaveRunning));

            // Spawner bị dừng hoặc hủy giữa chừng không được tính là hoàn tất Wave 3 trong trình tự cũ.
            if (enemySpawner == null || asteroidSpawner == null
                || !enemySpawner.IsWaveComplete || !asteroidSpawner.IsWaveComplete)
            {
                yield break;
            }

            NotifyBossWarning(2.5f);
            yield return new WaitForSeconds(2.5f);
        }

        if (IsFinished || sequenceCancelled) yield break;

        if (bossPrefab == null)
        {
            Debug.LogWarning("Hãy gán Boss Prefab cho Level1Controller.", this);
            yield break;
        }

        // BossMovement sẽ tự đặt Boss phía trên camera khi Start chạy.
        // [BAO VE] Sau khi hoàn tất các Wave và thời gian cảnh báo, Instantiate bossPrefab của Scene; BossMovement tự đưa Boss vào màn hình.
        spawnedBoss = Instantiate(bossPrefab, Vector3.zero, Quaternion.identity);
        CurrentBoss = spawnedBoss.GetComponent<Boss>();

        // Chờ Boss.Start() khởi tạo HP trước khi UI đọc máu hiện tại.
        yield return null;

        if (CurrentBoss != null)
        {
            // [BAO VE] Báo Boss đã có HP sau một frame để BossHealthUI và các bên nghe sự kiện nhận đúng Boss.
            BossSpawned?.Invoke(CurrentBoss);
        }
        else if (spawnedBoss != null)
        {
            Debug.LogWarning("Boss Prefab cần có component Boss trên GameObject gốc.", this);
        }

        // Chỉ đợi Boss do controller này sinh ra bị Destroy.
        // [BAO VE] Chờ chính Boss vừa sinh bị Destroy rồi gọi ShowVictory nếu trận chưa kết thúc.
        yield return new WaitUntil(() => spawnedBoss == null);

        if (gameResultManager == null)
        {
            Debug.LogWarning("Hãy gán Game Result Manager cho Level1Controller.", this);
            yield break;
        }

        if (!gameResultManager.IsGameFinished)
        {
            Debug.Log("LEVEL 1 COMPLETE");
            // [BAO VE - BOSS KET THUC VA VICTORY] Chỉ gọi sau khi Boss do controller sinh đã bị hủy và IsGameFinished vẫn false.
            gameResultManager.ShowVictory();
        }
    }

    // [BAO VE] Kiểm tra dữ liệu từng Wave và các Spawner/Prefab trước khi chạy; cấu hình không hợp lệ thì dừng chuỗi.
    bool ValidateCustomWaves()
    {
        if (customWaves == null || customWaves.Length == 0
            || bossPrefab == null || bossPrefab.GetComponent<Boss>() == null)
        {
            Debug.LogWarning("Custom Waves cần danh sách wave và Boss Prefab hợp lệ.", this);
            return false;
        }

        for (int i = 0; i < customWaves.Length; i++)
        {
            WaveDefinition wave = customWaves[i];

            if (wave == null || wave.enemyCount < 0 || wave.asteroidCount < 0
                || (wave.enemyCount == 0 && wave.asteroidCount == 0 && !HasFormations(wave))
                || float.IsNaN(wave.delayAfterWave) || float.IsInfinity(wave.delayAfterWave)
                || wave.delayAfterWave < 0f
                || (wave.enemyCount > 0 && !HasFormations(wave) && (enemySpawner == null
                    || !enemySpawner.isActiveAndEnabled || enemySpawner.enemyPrefab == null
                    || !IsValidInterval(wave.enemySpawnInterval)))
                || (wave.asteroidCount > 0 && (asteroidSpawner == null
                    || !asteroidSpawner.isActiveAndEnabled || asteroidSpawner.asteroidPrefab == null
                    || !IsValidInterval(wave.asteroidSpawnInterval)))
                || (HasFormations(wave) && (formationSpawner == null
                    || !formationSpawner.isActiveAndEnabled
                    || !IsNonNegative(wave.formationGroupDelay)
                    || !ValidateFormationGroups(wave.formationGroups)))
                || (wave.useAsteroidCorridor && (wave.asteroidCount < 1 || asteroidSpawner == null
                    || !asteroidSpawner.IsCorridorConfigurationValid(wave.corridorGroupInterval,
                        wave.corridorWidth, wave.corridorCenters))))
            {
                Debug.LogWarning("Cấu hình Custom Wave " + (i + 1) + " không hợp lệ.", this);
                return false;
            }
        }

        return true;
    }

    bool ValidateFormationGroups(EnemyFormationSettings[] groups)
    {
        foreach (EnemyFormationSettings group in groups)
            if (!formationSpawner.IsConfigurationValid(group)) return false;
        return true;
    }

    static bool IsNonNegative(float value)
    {
        return value >= 0f && !float.IsNaN(value) && !float.IsInfinity(value);
    }

    static bool IsValidInterval(float interval)
    {
        return interval > 0f && !float.IsNaN(interval) && !float.IsInfinity(interval);
    }

    // [BAO VE] Phát WaveStarted với số thứ tự, tổng Wave và tên; UI thông báo và Power-up dùng chung sự kiện này.
    void NotifyWaveStarted(int number, int total, string waveName)
    {
        if (gameResultManager == null || !gameResultManager.IsGameFinished)
        {
            WaveStartedCount++;
            WaveStarted?.Invoke(number, total, waveName);
        }
    }

    // [BAO VE] Phát BossWarningStarted theo thời gian nghỉ sẵn có; banner chỉ hiển thị, không tự thay đổi thời điểm sinh Boss.
    void NotifyBossWarning(float existingDelay)
    {
        if (gameResultManager == null || !gameResultManager.IsGameFinished)
        {
            BossWarningCount++;
            BossWarningStarted?.Invoke(existingDelay);
        }
    }

    // [BAO VE] Duyệt customWaves, chạy Enemy hoặc Formation cùng Asteroid; chờ các phần hoàn tất thật sự rồi nghỉ và chuyển Wave.
    IEnumerator RunCustomWaves()
    {
        customWavesComplete = false;

        for (int i = 0; i < customWaves.Length; i++)
        {
            WaveDefinition wave = customWaves[i];
            bool usesFormations = HasFormations(wave);
            bool usesEnemies = !usesFormations && wave.enemyCount > 0;
            bool usesAsteroids = wave.asteroidCount > 0;

            // Một spawner đang bận hoặc bị dừng không được tính là wave mới.
            if ((usesEnemies && (enemySpawner == null || !enemySpawner.isActiveAndEnabled
                    || enemySpawner.IsWaveRunning))
                || (usesAsteroids && (asteroidSpawner == null || !asteroidSpawner.isActiveAndEnabled
                    || asteroidSpawner.IsWaveRunning)))
            {
                Debug.LogWarning("Không thể bắt đầu Custom Wave " + (i + 1) + ": spawner chưa sẵn sàng.", this);
                yield break;
            }

            while (Time.timeScale <= 0f && !IsFinished) yield return null;
            if (IsFinished || sequenceCancelled) yield break;
            // [BAO VE - BAT DAU WAVE] Cập nhật chỉ số Wave hiện tại sau khi qua kiểm tra Pause/kết thúc; i bắt đầu từ 0, số hiển thị là i + 1.
            currentCustomWaveIndex = i;

            if (usesEnemies)
            {
                // [BAO VE - GOI SPAWN ENEMY THUONG] Controller truyền số lượng và nhịp sinh; Instantiate thực tế nằm trong EnemySpawner.SpawnWave.
                enemySpawner.StartEnemyWave(wave.enemyCount, wave.enemySpawnInterval);
            }

            if (usesAsteroids)
            {
                if (wave.useAsteroidCorridor)
                    asteroidSpawner.StartCorridorWave(wave.asteroidCount, wave.corridorGroupInterval,
                        wave.corridorWidth, wave.corridorCenters);
                else
                    asteroidSpawner.StartAsteroidWave(wave.asteroidCount, wave.asteroidSpawnInterval);
            }

            if ((usesEnemies && !enemySpawner.IsWaveRunning)
                || (usesAsteroids && !asteroidSpawner.IsWaveRunning))
            {
                yield break;
            }

            // [BAO VE - THONG BAO WAVE] Phát sự kiện cho banner và lịch Power-up sau khi các Spawner cần dùng đã khởi chạy.
            NotifyWaveStarted(i + 1, customWaves.Length, wave.name);

            if (usesFormations)
            {
                yield return RunFormationGroups(wave);
                if (!formationGroupsComplete || IsFinished) yield break;
            }

            // [BAO VE] Chờ các Spawner cần cho Wave dừng, sau đó kiểm tra IsWaveComplete; bị hủy giữa chừng không được coi là qua Wave.
            yield return new WaitUntil(() =>
                (usesEnemies && enemySpawner == null)
                || (usesAsteroids && asteroidSpawner == null)
                || ((!usesEnemies || !enemySpawner.IsWaveRunning)
                    && (!usesAsteroids || !asteroidSpawner.IsWaveRunning)));

            if ((usesEnemies && (enemySpawner == null || !enemySpawner.IsWaveComplete))
                || (usesAsteroids && (asteroidSpawner == null || !asteroidSpawner.IsWaveComplete)))
            {
                yield break;
            }

            if (i == customWaves.Length - 1)
            {
                NotifyBossWarning(wave.delayAfterWave);
            }

            if (wave.delayAfterWave > 0f)
            {
                // [BAO VE - CHUYEN WAVE TIEP THEO] Chờ thời gian nghỉ sau Wave rồi vòng for tăng i; chỉ tới Boss sau khi toàn bộ customWaves hoàn tất.
                yield return new WaitForSeconds(wave.delayAfterWave);
            }
        }

        customWavesComplete = true;
    }

    // [BAO VE] Chạy lần lượt từng nhóm Formation và chờ IsWaveComplete; nhóm bị hủy sẽ dừng chuỗi thay vì bỏ qua đến Boss.
    IEnumerator RunFormationGroups(WaveDefinition wave)
    {
        formationGroupsComplete = false;
        for (int i = 0; i < wave.formationGroups.Length; i++)
        {
            while (Time.timeScale <= 0f && !IsFinished) yield return null;
            if (IsFinished) yield break;
            // [BAO VE - GOI SPAWN FORMATION] Mỗi nhóm gửi cấu hình sang StartFormation; lệnh Instantiate Enemy nằm trong EnemyFormationSpawner.Run.
            if (formationSpawner == null || !formationSpawner.StartFormation(wave.formationGroups[i]))
            {
                Debug.LogWarning("Formation group could not start; level sequence stopped safely.", this);
                StopSequence();
                yield break;
            }
            yield return new WaitUntil(() => IsFinished || formationSpawner == null
                || !formationSpawner.IsWaveRunning);
            if (IsFinished) yield break;
            if (formationSpawner == null || !formationSpawner.IsWaveComplete)
            {
                Debug.LogWarning("Formation was cancelled; remaining groups and Boss were not skipped.", this);
                StopSequence();
                yield break;
            }
            if (i < wave.formationGroups.Length - 1 && wave.formationGroupDelay > 0f)
                yield return new WaitForSeconds(wave.formationGroupDelay);
        }
        formationGroupsComplete = true;
    }

    void Update()
    {
        if (IsFinished && !sequenceCancelled) StopSequence();
    }

    // [BAO VE] Khi trận kết thúc hoặc controller bị tắt: dừng coroutine, hủy Formation và tắt Spawner để không sinh tiếp.
    void StopSequence()
    {
        sequenceCancelled = true;
        StopAllCoroutines();
        if (formationSpawner != null && formationSpawner.IsWaveRunning)
            formationSpawner.CancelFormation();
        if (enemySpawner != null) enemySpawner.enabled = false;
        if (asteroidSpawner != null) asteroidSpawner.enabled = false;
    }

    void OnDisable()
    {
        if (Application.isPlaying) StopSequence();
    }
}
