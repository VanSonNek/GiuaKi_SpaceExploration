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
    public int SpawnedCount { get; private set; }
    public int CorridorGroupsStarted { get; private set; }
    public float CurrentCorridorCenter { get; private set; }
    public event System.Action<GameObject> AsteroidSpawned;

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
        SpawnedCount = 0;
        CorridorGroupsStarted = 0;
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
            SpawnedCount++;
            AsteroidSpawned?.Invoke(asteroid);

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

    // Two asteroids per group, leaving a lane wide enough for the Player even
    // at the largest existing random scale and throughout a complete rotation.
    public bool IsCorridorConfigurationValid(float interval, float laneWidth, float[] centers)
    {
        mainCamera = Camera.main;
        if (mainCamera == null || !mainCamera.orthographic || asteroidPrefab == null
            || asteroidPrefab.GetComponent<SpriteRenderer>() == null
            || asteroidPrefab.GetComponent<SpriteRenderer>().sprite == null
            || asteroidPrefab.GetComponent<AsteroidMovement>() == null
            || asteroidPrefab.GetComponent<AsteroidMovement>().moveSpeed <= 0f
            || !Positive(interval) || !Positive(laneWidth) || centers == null || centers.Length == 0)
            return false;
        foreach (float center in centers)
            if (float.IsNaN(center) || float.IsInfinity(center) || center < 0f || center > 1f
                || CorridorPositions(center, laneWidth) == null) return false;
        return true;
    }

    public bool StartCorridorWave(int count, float interval, float laneWidth, float[] centers)
    {
        if (!Application.isPlaying || !isActiveAndEnabled || IsWaveRunning) return false;
        IsWaveComplete = false;
        if (count < 1 || !IsCorridorConfigurationValid(interval, laneWidth, centers))
        {
            Debug.LogWarning("Asteroid corridor cannot fit this camera or has invalid settings.", this);
            return false;
        }
        spawnedAsteroids.Clear();
        SpawnedCount = 0;
        CorridorGroupsStarted = 0;
        IsWaveRunning = true;
        StartCoroutine(SpawnCorridor(count, interval, laneWidth, (float[])centers.Clone()));
        return true;
    }

    static bool Positive(float value)
    {
        return value > 0f && !float.IsNaN(value) && !float.IsInfinity(value);
    }

    float MaximumRotationRadius()
    {
        Bounds b = asteroidPrefab.GetComponent<SpriteRenderer>().bounds;
        float radius = new Vector2(b.extents.x, b.extents.y).magnitude;
        CircleCollider2D circle = asteroidPrefab.GetComponent<CircleCollider2D>();
        if (circle != null)
            radius = Mathf.Max(radius, (circle.radius + circle.offset.magnitude)
                * Mathf.Max(Mathf.Abs(circle.transform.lossyScale.x), Mathf.Abs(circle.transform.lossyScale.y)));
        return radius * 1.3f;
    }

    float[] CorridorPositions(float normalizedCenter, float laneWidth)
    {
        float left = mainCamera.ViewportToWorldPoint(Vector3.zero).x;
        float right = mainCamera.ViewportToWorldPoint(Vector3.one).x;
        float radius = MaximumRotationRadius();
        float center = Mathf.Lerp(left, right, normalizedCenter);
        float leftMin = left + radius + 0.1f;
        float leftMax = center - laneWidth * 0.5f - radius;
        float rightMin = center + laneWidth * 0.5f + radius;
        float rightMax = right - radius - 0.1f;
        bool fitsLeft = leftMax >= leftMin;
        bool fitsRight = rightMax >= rightMin;
        if (fitsLeft && fitsRight)
            return new[] { (leftMin + leftMax) * 0.5f, (rightMin + rightMax) * 0.5f };
        if (fitsLeft && leftMax - leftMin >= 2f * radius + 0.15f)
            return new[] { leftMin, leftMax };
        if (fitsRight && rightMax - rightMin >= 2f * radius + 0.15f)
            return new[] { rightMin, rightMax };
        return null;
    }

    IEnumerator SpawnCorridor(int count, float interval, float laneWidth, float[] centers)
    {
        int group = 0;
        while (SpawnedCount < count)
        {
            while (Time.timeScale <= 0f) yield return null;
            if (mainCamera == null) { IsWaveRunning = false; yield break; }
            CurrentCorridorCenter = centers[group % centers.Length];
            float[] positions = CorridorPositions(CurrentCorridorCenter, laneWidth);
            if (positions == null)
            {
                Debug.LogWarning("Asteroid corridor no longer fits the camera.", this);
                IsWaveRunning = false;
                yield break;
            }
            CorridorGroupsStarted++;
            for (int i = 0; i < positions.Length && SpawnedCount < count; i++)
            {
                float top = mainCamera.ViewportToWorldPoint(Vector3.one).y;
                GameObject asteroid = Instantiate(asteroidPrefab,
                    new Vector3(positions[i], top + MaximumRotationRadius() + 0.1f,
                        asteroidPrefab.transform.position.z), Quaternion.identity);
                asteroid.transform.localScale = asteroidPrefab.transform.localScale * Random.Range(0.8f, 1.3f);
                spawnedAsteroids.Add(asteroid);
                SpawnedCount++;
                AsteroidSpawned?.Invoke(asteroid);
            }
            group++;
            if (SpawnedCount < count) yield return new WaitForSeconds(interval);
        }
        yield return new WaitUntil(AllAsteroidsDestroyed);
        IsWaveComplete = true;
        IsWaveRunning = false;
    }

    void OnDisable()
    {
        // Dừng wave và cho phép gọi lại sau khi bật spawner trở lại.
        StopAllCoroutines();
        IsWaveRunning = false;
    }
}
