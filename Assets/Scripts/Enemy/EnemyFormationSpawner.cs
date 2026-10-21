using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyFormationType
{
    VFormation, TwoColumns, SideAttack, StaggeredSquad, EscortFormation, HoldAndShoot
}

public enum FormationSpawnDirection { BothSides, Left, Right }

[Serializable]
public sealed class EnemyFormationSettings
{
    public EnemyFormationType formationType;
    public GameObject enemyPrefab;
    [Range(1, 128)] public int enemyCount = 5;
    [Min(0f)] public float spawnInterval = 0.15f;
    [Min(0f)] public float horizontalSpacing = 1.4f;
    [Min(0f)] public float verticalSpacing = 1.2f;
    public FormationSpawnDirection spawnDirection;
    public bool overrideMovement;
    public EnemyMovementType movementProfile;
    [Range(0f, 15f)] public float holdDuration = 2f;
    [Range(1, 16)] public int squadSize = 3;
    [Min(0f)] public float groupInterval = 1f;
    [Min(0.1f)] public float entrySpeed = 3f;
    [Min(0.05f)] public float clearance = 0.2f;
    [Range(5f, 90f)] public float memberLifetime = 40f;

    // Snapshot prevents Inspector/caller changes halfway through a sequence.
    public EnemyFormationSettings Snapshot() => (EnemyFormationSettings)MemberwiseClone();
}

// Opt-in API for a future wave controller. Never auto-starts or persists across scenes.
public sealed class EnemyFormationSpawner : MonoBehaviour
{
    public EnemyFormationSettings settings = new EnemyFormationSettings();
    public GameResultManager gameResultManager;
    public bool IsWaveRunning { get; private set; }
    public bool IsWaveComplete { get; private set; }
    public int SpawnedCount { get; private set; }
    public int GroupsStarted { get; private set; }
    public event Action<EnemyFormationMember> EnemySpawned;

    private readonly List<EnemyFormationMember> members = new List<EnemyFormationMember>();
    private Camera view;
    private Vector2 footprintSize;
    private Vector3 footprintOffset;

    public bool StartFormation() => StartFormation(settings);

    public bool StartFormation(EnemyFormationSettings configuration)
    {
        if (!Application.isPlaying || !isActiveAndEnabled || IsWaveRunning) return false;
        IsWaveComplete = false;
        view = Camera.main;
        if (!Validate(configuration))
        {
            Debug.LogWarning("Formation needs a valid 2D camera, enemy prefab and finite settings.", this);
            return false;
        }
        if (gameResultManager == null)
            gameResultManager = FindAnyObjectByType<GameResultManager>();
        if (gameResultManager != null && gameResultManager.IsGameFinished) return false;

        Bounds b = EnemyFormationMember.MeasureFootprint(configuration.enemyPrefab);
        footprintSize = b.size;
        footprintOffset = b.center - configuration.enemyPrefab.transform.position;
        members.Clear();
        SpawnedCount = 0;
        GroupsStarted = 0;
        IsWaveRunning = true;
        StartCoroutine(Run(configuration.Snapshot()));
        return true;
    }

    bool Validate(EnemyFormationSettings c)
    {
        if (c == null || view == null || !view.orthographic
            || Mathf.Abs(Vector3.Dot(view.transform.forward, Vector3.forward)) < 0.999f
            || c.enemyPrefab == null || !c.enemyPrefab.activeSelf
            || c.enemyPrefab.GetComponent<Enemy>() == null
            || c.enemyPrefab.GetComponent<EnemyMovement>() == null
            || !c.enemyPrefab.GetComponent<EnemyMovement>().enabled
            || c.enemyPrefab.GetComponent<SpriteRenderer>() == null
            || c.enemyPrefab.GetComponent<SpriteRenderer>().sprite == null
            || c.enemyPrefab.GetComponent<EnemyFormationMember>() != null
            || !Positive(c.enemyPrefab.GetComponent<EnemyMovement>().moveSpeed)
            || c.enemyCount < 1 || c.enemyCount > 128 || c.squadSize < 1 || c.squadSize > 16
            || !NonNegative(c.spawnInterval) || !NonNegative(c.groupInterval)
            || !NonNegative(c.horizontalSpacing) || !NonNegative(c.verticalSpacing)
            || !NonNegative(c.holdDuration) || c.holdDuration > 15f
            || !Positive(c.entrySpeed) || !Positive(c.clearance)
            || !Positive(c.memberLifetime) || c.memberLifetime < 5f || c.memberLifetime > 90f
            || !Enum.IsDefined(typeof(EnemyFormationType), c.formationType)
            || !Enum.IsDefined(typeof(FormationSpawnDirection), c.spawnDirection)
            || !Enum.IsDefined(typeof(EnemyMovementType), c.movementProfile)) return false;
        Bounds b = EnemyFormationMember.MeasureFootprint(c.enemyPrefab);
        Vector3 bottom = view.ViewportToWorldPoint(Vector3.zero);
        Vector3 top = view.ViewportToWorldPoint(Vector3.one);
        // Reserve a safe upper band. Refuse impossible layouts rather than overlap ships.
        return b.size.x + 2f * c.clearance < top.x - bottom.x
            && b.size.y + 2f * c.clearance < (top.y - bottom.y) * 0.45f;
    }

    static bool Positive(float v) => v > 0f && !float.IsNaN(v) && !float.IsInfinity(v);
    static bool NonNegative(float v) => v >= 0f && !float.IsNaN(v) && !float.IsInfinity(v);

    IEnumerator Run(EnemyFormationSettings c)
    {
        while (SpawnedCount < c.enemyCount)
        {
            // TimeScale=0 can still execute a freshly started coroutine: wait before spawn.
            while (Time.timeScale <= 0f) yield return null;
            if (view == null) { CancelFormation(); yield break; }
            List<Vector2> slots = BuildSlots(c, c.enemyCount - SpawnedCount);
            if (slots.Count == 0)
            {
                Debug.LogWarning("Formation cannot fit this camera without overlapping ships.", this);
                CancelFormation();
                yield break;
            }
            var group = new List<EnemyFormationMember>();
            float maxY = float.MinValue;
            float minY = float.MaxValue;
            foreach (Vector2 slot in slots)
            {
                maxY = Mathf.Max(maxY, slot.y);
                minY = Mathf.Min(minY, slot.y);
            }
            Vector3 top = view.ViewportToWorldPoint(Vector3.one);
            Vector3 bottom = view.ViewportToWorldPoint(Vector3.zero);
            // GroupInterval is a minimum gap. Even with zero delay/very fast entry,
            // let the previous group clear the whole approach band before overtaking it.
            float entryBandBottom = top.y - c.clearance - footprintSize.y + minY - maxY;
            while (members.Exists(m => m != null && m.Footprint.max.y >= entryBandBottom))
                yield return null;
            GroupsStarted++;
            float centerX = (top.x + bottom.x) * 0.5f;
            for (int i = 0; i < slots.Count; i++)
            {
                while (Time.timeScale <= 0f) yield return null;
                Vector3 destination = new Vector3(centerX + slots[i].x,
                    top.y - c.clearance - footprintSize.y * 0.5f + slots[i].y - maxY,
                    c.enemyPrefab.transform.position.z) - footprintOffset;
                Vector3 spawn = destination;
                float exit = 0f;
                if (c.formationType == EnemyFormationType.SideAttack)
                {
                    bool fromLeft = c.spawnDirection == FormationSpawnDirection.Left
                        || (c.spawnDirection == FormationSpawnDirection.BothSides && i % 2 == 0);
                    spawn.x = (fromLeft ? bottom.x - footprintSize.x * 0.5f - c.clearance
                        : top.x + footprintSize.x * 0.5f + c.clearance) - footprintOffset.x;
                    exit = fromLeft ? 1f : -1f;
                }
                else
                    spawn.y = top.y + c.clearance + footprintSize.y * 0.5f
                        + slots[i].y - footprintOffset.y;

                float blockedTime = 0f;
                while (!SpawnIsClear(spawn))
                {
                    blockedTime += Time.deltaTime;
                    if (blockedTime >= 5f) { CancelFormation(); yield break; }
                    yield return null;
                }
                while (Time.timeScale <= 0f) yield return null;
                GameObject enemy = Instantiate(c.enemyPrefab, spawn, Quaternion.identity);
                EnemyFormationMember member = enemy.AddComponent<EnemyFormationMember>();
                member.Initialize(view, destination, c.entrySpeed, c.memberLifetime,
                    exit, c.overrideMovement, c.movementProfile);
                members.Add(member);
                group.Add(member);
                SpawnedCount++;
                EnemySpawned?.Invoke(member);
                if (i < slots.Count - 1 && c.spawnInterval > 0f)
                    yield return new WaitForSeconds(c.spawnInterval);
            }
            // Each entry has a finite scaled-time lifetime, even if obstructed/destroyed.
            while (!AllEntered(group)) yield return null;
            if (c.holdDuration > 0f) yield return new WaitForSeconds(c.holdDuration);
            while (Time.timeScale <= 0f) yield return null;
            foreach (EnemyFormationMember member in group)
                if (member != null) member.Release();
            if (SpawnedCount < c.enemyCount && c.groupInterval > 0f)
                yield return new WaitForSeconds(c.groupInterval);
        }
        while (members.Exists(m => m != null)) yield return null;
        IsWaveComplete = true;
        IsWaveRunning = false;
    }

    List<Vector2> BuildSlots(EnemyFormationSettings c, int remaining)
    {
        Vector3 bottom = view.ViewportToWorldPoint(Vector3.zero);
        Vector3 top = view.ViewportToWorldPoint(Vector3.one);
        float width = top.x - bottom.x - 2f * c.clearance - footprintSize.x;
        float height = (top.y - bottom.y) * 0.45f - 2f * c.clearance - footprintSize.y;
        float dx = Mathf.Max(c.horizontalSpacing, footprintSize.x + c.clearance);
        float dy = Mathf.Max(c.verticalSpacing, footprintSize.y + c.clearance);
        int rows = Mathf.Max(1, Mathf.FloorToInt(height / dy) + 1);
        int columns = Mathf.Max(1, Mathf.FloorToInt(width / dx) + 1);
        var slots = new List<Vector2>();
        int count;
        switch (c.formationType)
        {
            case EnemyFormationType.VFormation:
                int wings = Mathf.Min(Mathf.FloorToInt(width / (2f * dx)), rows - 1);
                if (wings < 1 && remaining > 1) return slots;
                count = Mathf.Min(remaining, 1 + 2 * wings);
                slots.Add(Vector2.zero);
                for (int i = 1; i < count; i++)
                    slots.Add(new Vector2((i % 2 == 1 ? -1f : 1f) * ((i + 1) / 2) * dx,
                        ((i + 1) / 2) * dy));
                break;
            case EnemyFormationType.EscortFormation:
                if (columns < 3 && remaining > 1) return slots;
                count = Mathf.Min(remaining, 1 + 2 * rows);
                int guardRows = Mathf.Max(1, Mathf.CeilToInt((count - 1) / 2f));
                slots.Add(new Vector2(0f, (guardRows - 1) * dy * 0.5f));
                for (int i = 1; i < count; i++)
                    slots.Add(new Vector2(i % 2 == 1 ? -dx : dx, ((i - 1) / 2) * dy));
                break;
            case EnemyFormationType.SideAttack:
                // Each horizontal lane is unique, including opposite-side entries.
                count = Mathf.Min(remaining, rows);
                for (int i = 0; i < count; i++)
                    slots.Add(new Vector2((i % 2 == 0 ? -1f : 1f) * width * 0.25f, i * dy));
                break;
            case EnemyFormationType.TwoColumns:
                if (columns < 2) return slots;
                count = Mathf.Min(remaining, 2 * rows);
                for (int i = 0; i < count; i++)
                    slots.Add(new Vector2((i % 2 == 0 ? -0.5f : 0.5f) * dx, (i / 2) * dy));
                break;
            default:
                // HoldAndShoot: upper grid. StaggeredSquad: smaller successive grids.
                count = Mathf.Min(remaining, columns * rows);
                if (c.formationType == EnemyFormationType.StaggeredSquad)
                    count = Mathf.Min(count, c.squadSize);
                int usedColumns = Mathf.Min(count, columns);
                for (int i = 0; i < count; i++)
                    slots.Add(new Vector2((i % usedColumns - (usedColumns - 1) * 0.5f) * dx,
                        (i / usedColumns) * dy));
                break;
        }
        return slots;
    }

    bool SpawnIsClear(Vector3 position)
    {
        Bounds candidate = new Bounds(position + footprintOffset, footprintSize);
        foreach (EnemyFormationMember member in members)
            if (member != null && candidate.Intersects(member.Footprint)) return false;
        foreach (Collider2D collider in Physics2D.OverlapBoxAll(candidate.center, footprintSize, 0f))
            if (collider.GetComponentInParent<PlayerHealth>() != null
                || collider.GetComponentInParent<Enemy>() != null) return false;
        return true;
    }

    static bool AllEntered(List<EnemyFormationMember> group)
    {
        foreach (EnemyFormationMember member in group)
            if (member != null && !member.HasEntered) return false;
        return true;
    }

    void Update()
    {
        if (IsWaveRunning && (view == null
            || (gameResultManager != null && gameResultManager.IsGameFinished)))
            CancelFormation();
    }

    public void CancelFormation()
    {
        StopAllCoroutines();
        IsWaveRunning = false;
        IsWaveComplete = false;
        foreach (EnemyFormationMember member in members)
            if (member != null) Destroy(member.gameObject);
        members.Clear();
    }

    void OnDisable() => CancelFormation();
}
