using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidSpawner : MonoBehaviour
{
    public GameObject asteroidPrefab;

    [Min(1)]
    public int asteroidCount = 6;

    [Min(0.01f)]
    public float spawnInterval = 0.9f;

    // Wave chỉ hoàn tất sau khi sinh đủ và tất cả thiên thạch đã bị hủy.
    public bool IsWaveRunning { get; private set; }
    public bool IsWaveComplete { get; private set; }

    private Camera mainCamera;
    private readonly List<GameObject> spawnedAsteroids = new List<GameObject>();

    // Có thể gọi từ Level1Controller hoặc menu component khi đang Play.
    [ContextMenu("Start Asteroid Wave")]
    public void StartAsteroidWave()
    {
        // Giữ cách gọi cũ và các giá trị mặc định trong Inspector.
        StartAsteroidWave(asteroidCount, spawnInterval);
    }

    public void StartAsteroidWave(int count, float interval)
    {
        if (!Application.isPlaying || !isActiveAndEnabled || IsWaveRunning)
        {
            return;
        }

        IsWaveComplete = false;

        if (count <= 0 || interval <= 0f)
        {
            Debug.LogWarning("Số Asteroid và khoảng cách sinh phải lớn hơn 0.", this);
            return;
        }

        mainCamera = Camera.main;

        if (asteroidPrefab == null || mainCamera == null)
        {
            Debug.LogWarning("Hãy gán Asteroid Prefab và đặt tag MainCamera cho camera.", this);
            return;
        }

        SpriteRenderer prefabRenderer = asteroidPrefab.GetComponent<SpriteRenderer>();

        if (prefabRenderer == null || prefabRenderer.sprite == null)
        {
            Debug.LogWarning("Asteroid Prefab cần SpriteRenderer có sprite trên object gốc.", this);
            return;
        }

        // Đặt cờ trước khi bắt đầu để các lần gọi tiếp theo không tạo wave trùng.
        spawnedAsteroids.Clear();
        IsWaveRunning = true;
        StartCoroutine(SpawnWave(count, interval));
    }

    IEnumerator SpawnWave(int count, float interval)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(Vector3.zero);
            Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));

            Vector3 spawnPosition = new Vector3(
                (bottomLeft.x + topRight.x) / 2f,
                topRight.y,
                asteroidPrefab.transform.position.z
            );

            GameObject asteroid = Instantiate(asteroidPrefab, spawnPosition, Quaternion.identity);
            float scaleMultiplier = Random.Range(0.8f, 1.3f);
            asteroid.transform.localScale = asteroidPrefab.transform.localScale * scaleMultiplier;

            // Tính giới hạn X sau khi áp dụng Scale ngẫu nhiên.
            Bounds asteroidBounds = asteroid.GetComponent<SpriteRenderer>().bounds;
            float minX = bottomLeft.x + asteroidBounds.extents.x;
            float maxX = topRight.x - asteroidBounds.extents.x;

            if (minX > maxX)
            {
                Debug.LogWarning("Asteroid quá rộng để nằm hoàn toàn trong camera. Hãy giảm Scale của prefab.", this);
                Destroy(asteroid);
                IsWaveRunning = false;
                yield break;
            }

            float randomX = Random.Range(minX, maxX);

            // Căn tâm sprite theo X và đặt cạnh dưới ngay phía trên camera.
            asteroid.transform.position += new Vector3(
                randomX - asteroidBounds.center.x,
                topRight.y + 0.1f - asteroidBounds.min.y,
                0f
            );

            spawnedAsteroids.Add(asteroid);

            if (i < count - 1)
            {
                yield return new WaitForSeconds(interval);
            }
        }

        // Sinh xong vẫn phải chờ thiên thạch cuối cùng biến mất.
        yield return new WaitUntil(AllAsteroidsDestroyed);

        IsWaveComplete = true;
        IsWaveRunning = false;
    }

    bool AllAsteroidsDestroyed()
    {
        foreach (GameObject asteroid in spawnedAsteroids)
        {
            if (asteroid != null)
            {
                return false;
            }
        }

        return true;
    }

    void OnDisable()
    {
        // Dừng wave và cho phép gọi lại sau khi bật spawner trở lại.
        StopAllCoroutines();
        IsWaveRunning = false;
    }
}
