using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab;

    [Min(0.01f)]
    public float spawnInterval = 1.5f;

    // Wave chạy từ lúc bắt đầu sinh đến khi Enemy cuối cùng đã bị hủy.
    public bool IsWaveRunning { get; private set; }
    public bool IsWaveComplete { get; private set; }

    private Camera mainCamera;
    private readonly List<GameObject> spawnedEnemies = new List<GameObject>();

    public void StartEnemyWave()
    {
        // Giữ cách gọi cũ: 4 Enemy với khoảng cách sinh trong Inspector.
        StartEnemyWave(4, spawnInterval);
    }

    public void StartEnemyWave(int count, float interval)
    {
        if (!Application.isPlaying || !isActiveAndEnabled || IsWaveRunning)
        {
            return;
        }

        IsWaveComplete = false;

        if (count <= 0 || interval <= 0f)
        {
            Debug.LogWarning("Số Enemy và khoảng cách sinh phải lớn hơn 0.", this);
            return;
        }

        mainCamera = Camera.main;

        if (enemyPrefab == null || mainCamera == null)
        {
            Debug.LogWarning("Hãy gán Enemy Prefab và đặt tag MainCamera cho camera.", this);
            return;
        }

        SpriteRenderer prefabRenderer = enemyPrefab.GetComponent<SpriteRenderer>();

        if (prefabRenderer == null || prefabRenderer.sprite == null)
        {
            Debug.LogWarning("Enemy Prefab cần SpriteRenderer có sprite trên object gốc.", this);
            return;
        }

        spawnedEnemies.Clear();
        IsWaveRunning = true;
        StartCoroutine(SpawnWave(count, interval));
    }

    IEnumerator SpawnWave(int count, float interval)
    {
        // Mỗi lần gọi chỉ sinh đủ số Enemy được yêu cầu.
        for (int i = 0; i < count; i++)
        {
            Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(Vector3.zero);
            Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));

            Vector3 spawnPosition = new Vector3(
                (bottomLeft.x + topRight.x) / 2f,
                topRight.y,
                enemyPrefab.transform.position.z
            );

            GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

            // Lấy bounds của bản sao để kích thước đã tính cả Scale.
            Bounds enemyBounds = enemy.GetComponent<SpriteRenderer>().bounds;
            float minX = bottomLeft.x + enemyBounds.extents.x;
            float maxX = topRight.x - enemyBounds.extents.x;

            if (minX > maxX)
            {
                Debug.LogWarning("Enemy quá rộng để nằm hoàn toàn trong camera. Hãy giảm Scale của prefab.", this);
                Destroy(enemy);
                IsWaveRunning = false;
                yield break;
            }

            float randomX = Random.Range(minX, maxX);

            // Đặt tâm sprite ở X ngẫu nhiên và cạnh dưới ngay trên màn hình.
            // Dùng bounds nên vẫn đúng khi pivot của sprite không nằm ở giữa.
            enemy.transform.position += new Vector3(
                randomX - enemyBounds.center.x,
                topRight.y + 0.1f - enemyBounds.min.y,
                0f
            );

            // Chỉ theo dõi Enemy do chính wave này sinh ra.
            spawnedEnemies.Add(enemy);

            // Chỉ chờ nếu còn Enemy tiếp theo cần sinh.
            if (i < count - 1)
            {
                yield return new WaitForSeconds(interval);
            }
        }

        // Đã sinh đủ Enemy. Tiếp tục chờ chúng bị Destroy bằng bất kỳ cách nào.
        yield return new WaitUntil(AllEnemiesDestroyed);

        IsWaveComplete = true;
        IsWaveRunning = false;
    }

    bool AllEnemiesDestroyed()
    {
        foreach (GameObject enemy in spawnedEnemies)
        {
            // Trong Unity, reference tới object đã bị Destroy được so sánh bằng null.
            if (enemy != null)
            {
                return false;
            }
        }

        return true;
    }

    void OnDisable()
    {
        // Wave bị dừng giữa chừng sẽ không được báo là đã hoàn tất.
        StopAllCoroutines();
        IsWaveRunning = false;
    }
}
