using System.Collections;
using UnityEngine;

public class Level1Controller : MonoBehaviour
{
    public EnemySpawner enemySpawner;
    public AsteroidSpawner asteroidSpawner;
    public GameObject bossPrefab;
    public GameResultManager gameResultManager;

    public Boss CurrentBoss { get; private set; }
    public event System.Action<Boss> BossSpawned;

    private GameObject spawnedBoss;

    // Unity tự chạy Start dạng Coroutine khi game bắt đầu.
    IEnumerator Start()
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
}
