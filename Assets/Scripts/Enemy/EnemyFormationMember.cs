using UnityEngine;

// Runtime-only steering. The prefab's movement and weapon settings stay intact.
[RequireComponent(typeof(EnemyMovement), typeof(Enemy), typeof(SpriteRenderer))]
// [BAO VE] Điều khiển từng thành viên qua Entering, Holding, Released hoặc Exiting; không viết lại EnemyShooting.
public sealed class EnemyFormationMember : MonoBehaviour
{
    public enum FlightState { Entering, Holding, Released, Exiting }

    public FlightState State { get; private set; }
    public bool HasEntered => State != FlightState.Entering;
    public Bounds Footprint => MeasureFootprint(gameObject);

    private EnemyMovement movement;
    private Camera view;
    private Vector3 target;
    private float entrySpeed;
    private float remainingLifetime;
    private float exitDirection;
    private bool initialized;

    // [BAO VE] Tạm tắt EnemyMovement để MoveTowards đưa tàu vào đích; lưu thời gian sống và hướng thoát từ Spawner.
    public void Initialize(Camera camera, Vector3 destination, float speed, float lifetime,
        float horizontalExit, bool overrideMovement, EnemyMovementType profile)
    {
        movement = GetComponent<EnemyMovement>();
        // Called by the spawner before the first Start: movement initializes at release.
        movement.enabled = false;
        // [BAO VE - FORMATION MOVEMENT PROFILE] Nếu bật overrideMovement thì gán kiểu bay được cấu hình và tắt chọn ngẫu nhiên; vẫn giữ các thông số bắn của Enemy.
        if (overrideMovement)
        {
            movement.randomizeMovement = false;
            movement.movementType = profile;
        }
        view = camera;
        target = destination;
        entrySpeed = speed;
        remainingLifetime = lifetime;
        exitDirection = horizontalExit;
        State = FlightState.Entering;
        initialized = true;
    }

    // [BAO VE] Sau thời gian giữ vị trí, bật lại EnemyMovement hoặc chuyển sang bay ngang ra ngoài theo exitDirection.
    public void Release()
    {
        if (!initialized || State != FlightState.Holding) return;
        // [BAO VE - FORMATION THOAT MAN HINH] Không có hướng thoát ngang thì bật EnemyMovement; có hướng thì Member tự đưa tàu ra cạnh bên.
        State = exitDirection == 0f ? FlightState.Released : FlightState.Exiting;
        movement.enabled = State == FlightState.Released;
    }

    // [BAO VE] Dùng deltaTime nên Pause dừng bay và tuổi thọ; hủy khi hết lifetime hoặc thành viên đã rời màn hình.
    void Update()
    {
        if (!initialized || Time.deltaTime <= 0f) return;
        remainingLifetime -= Time.deltaTime;
        if (view == null || remainingLifetime <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        if (State == FlightState.Entering)
        {
            transform.position = Vector3.MoveTowards(transform.position, target,
                entrySpeed * Time.deltaTime);
            if ((transform.position - target).sqrMagnitude < 0.000001f)
                State = FlightState.Holding;
        }
        else if (State == FlightState.Exiting)
        {
            transform.position += Vector3.right * exitDirection
                * movement.moveSpeed * Time.deltaTime;
        }

        // Includes horizontal exits, unlike the legacy downward-only cleanup.
        if (State == FlightState.Released || State == FlightState.Exiting)
        {
            Bounds b = Footprint;
            Vector3 bottom = view.ViewportToWorldPoint(Vector3.zero);
            Vector3 top = view.ViewportToWorldPoint(Vector3.one);
            if (b.max.x < bottom.x || b.min.x > top.x
                || b.max.y < bottom.y || b.min.y > top.y)
                Destroy(gameObject);
        }
    }

    // [BAO VE] Gộp Bounds Sprite và Collider, kể cả offset/scale, để Spawner tính khoảng cách tránh chồng tàu.
    public static Bounds MeasureFootprint(GameObject root)
    {
        Bounds result = root.GetComponent<SpriteRenderer>().bounds;
        // The project uses root BoxCollider2D. Include its offset/scale, even on assets
        // whose Collider2D.bounds are empty outside the physics world.
        foreach (BoxCollider2D box in root.GetComponentsInChildren<BoxCollider2D>(true))
        {
            Vector2 half = box.size * 0.5f;
            for (int x = -1; x <= 1; x += 2)
                for (int y = -1; y <= 1; y += 2)
                    result.Encapsulate(box.transform.TransformPoint(box.offset
                        + new Vector2(half.x * x, half.y * y)));
        }
        foreach (Collider2D collider in root.GetComponentsInChildren<Collider2D>(true))
            if (collider.bounds.size.sqrMagnitude > 0f) result.Encapsulate(collider.bounds);
        return result;
    }
}
