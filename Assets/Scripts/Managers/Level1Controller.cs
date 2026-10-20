using System.Collections;
using UnityEngine;

public class Level1Controller : MonoBehaviour
{
    public EnemySpawner enemySpawner;
    public AsteroidSpawner asteroidSpawner;
    public GameObject bossPrefab;
    public GameResultManager gameResultManager;

    [System.Serializable]
    public class WaveDefinition
    {
        public string name;
        [Min(0)] public int enemyCount;
        [Min(0.01f)] public float enemySpawnInterval = 1f;
        [Min(0)] public int asteroidCount;
        [Min(0.01f)] public float asteroidSpawnInterval = 1f;
        [Min(0f)] public float delayAfterWave = 2f;
    }

    [Header("Custom Waves (off keeps the original Level 1 sequence)")]
    [SerializeField] private bool useCustomWaves;
    [SerializeField] private WaveDefinition[] customWaves = new WaveDefinition[0];

    private bool sequenceStarted;
    private bool customWavesComplete;
    private int currentCustomWaveIndex = -1;

    public int CurrentCustomWaveNumber => currentCustomWaveIndex + 1;
    public Boss CurrentBoss { get; private set; }
    public event System.Action<Boss> BossSpawned;

    private GameObject spawnedBoss;

    // Unity tự chạy Start dạng Coroutine khi game bắt đầu.
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

            // Wave 2: cả hai Coroutine bắt đầu cùng giai đoạn.
            enemySpawner.StartEnemyWave(6, 1.2f);
            asteroidSpawner.StartAsteroidWave(3, 2.2f);

            // Chờ cả hai spawner của Wave 2 kết thúc.
            yield return new WaitUntil(() =>
                enemySpawner == null || asteroidSpawner == null ||
                (!enemySpawner.IsWaveRunning && !asteroidSpawner.IsWaveRunning));

            // Spawner bị dừng hoặc hủy giữa chừng không được tính là hoàn tất Wave 2.
            if (enemySpawner == null || asteroidSpawner == null
                || !enemySpawner.IsWaveComplete || !asteroidSpawner.IsWaveComplete)
            {
                yield break;
            }

            yield return new WaitForSeconds(2.5f);
        }

        if (bossPrefab == null)
        {
            Debug.LogWarning("Hãy gán Boss Prefab cho Level1Controller.", this);
            yield break;
        }

        // BossMovement sẽ tự đặt Boss phía trên camera khi Start chạy.
        spawnedBoss = Instantiate(bossPrefab, Vector3.zero, Quaternion.identity);
        CurrentBoss = spawnedBoss.GetComponent<Boss>();

        // Chờ Boss.Start() khởi tạo HP trước khi UI đọc máu hiện tại.
        yield return null;

        if (CurrentBoss != null)
        {
            BossSpawned?.Invoke(CurrentBoss);
        }
        else if (spawnedBoss != null)
        {
            Debug.LogWarning("Boss Prefab cần có component Boss trên GameObject gốc.", this);
        }

        // Chỉ đợi Boss do controller này sinh ra bị Destroy.
        yield return new WaitUntil(() => spawnedBoss == null);

        if (gameResultManager == null)
        {
            Debug.LogWarning("Hãy gán Game Result Manager cho Level1Controller.", this);
            yield break;
        }

        if (!gameResultManager.IsGameFinished)
        {
            Debug.Log("LEVEL 1 COMPLETE");
            gameResultManager.ShowVictory();
        }
    }

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
                || (wave.enemyCount == 0 && wave.asteroidCount == 0)
                || float.IsNaN(wave.delayAfterWave) || float.IsInfinity(wave.delayAfterWave)
                || wave.delayAfterWave < 0f
                || (wave.enemyCount > 0 && (enemySpawner == null
                    || !enemySpawner.isActiveAndEnabled || enemySpawner.enemyPrefab == null
                    || !IsValidInterval(wave.enemySpawnInterval)))
                || (wave.asteroidCount > 0 && (asteroidSpawner == null
                    || !asteroidSpawner.isActiveAndEnabled || asteroidSpawner.asteroidPrefab == null
                    || !IsValidInterval(wave.asteroidSpawnInterval))))
            {
                Debug.LogWarning("Cấu hình Custom Wave " + (i + 1) + " không hợp lệ.", this);
                return false;
            }
        }

        return true;
    }

    static bool IsValidInterval(float interval)
    {
        return interval > 0f && !float.IsNaN(interval) && !float.IsInfinity(interval);
    }

    IEnumerator RunCustomWaves()
    {
        customWavesComplete = false;

        for (int i = 0; i < customWaves.Length; i++)
        {
            WaveDefinition wave = customWaves[i];
            bool usesEnemies = wave.enemyCount > 0;
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

            currentCustomWaveIndex = i;

            if (usesEnemies)
            {
                enemySpawner.StartEnemyWave(wave.enemyCount, wave.enemySpawnInterval);
            }

            if (usesAsteroids)
            {
                asteroidSpawner.StartAsteroidWave(wave.asteroidCount, wave.asteroidSpawnInterval);
            }

            if ((usesEnemies && !enemySpawner.IsWaveRunning)
                || (usesAsteroids && !asteroidSpawner.IsWaveRunning))
            {
                yield break;
            }

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

            if (wave.delayAfterWave > 0f)
            {
                yield return new WaitForSeconds(wave.delayAfterWave);
            }
        }

        customWavesComplete = true;
    }
}
