using UnityEngine;

public enum EnemyMovementType
{
    Straight,
    Zigzag,
    Diagonal
}

public class EnemyMovement : MonoBehaviour
{
    [Min(0f)]
    public float moveSpeed = 2f;

    public EnemyMovementType movementType = EnemyMovementType.Straight;
    public bool randomizeMovement;
    [Min(0f)] public float zigzagAmplitude = 0.7f;
    [Min(0f)] public float zigzagFrequency = 0.65f;
    [Min(0f)] public float driftSpeed = 0.65f;

    public EnemyMovementType CurrentMovementType { get; private set; }

    private Camera mainCamera;
    private SpriteRenderer spriteRenderer;
    private float originX;
    private float elapsed;
    private float horizontalDirection;

    void Start()
    {
        mainCamera = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (mainCamera == null || spriteRenderer == null)
        {
            Debug.LogWarning("Enemy cần SpriteRenderer và camera cần tag MainCamera.", this);
            enabled = false;
            return;
        }

        CurrentMovementType = randomizeMovement
            ? (EnemyMovementType)Random.Range(0, 3)
            : movementType;
        originX = transform.position.x;
        horizontalDirection = CurrentMovementType == EnemyMovementType.Diagonal
            && Random.value < 0.5f ? -1f : 1f;
    }

    void Update()
    {
        // Bay thẳng xuống theo trục Y của thế giới.
        transform.position += Vector3.down * moveSpeed * Time.deltaTime;

        if (CurrentMovementType != EnemyMovementType.Straight && Time.deltaTime > 0f)
        {
            MoveHorizontally();
        }

        float screenBottom = mainCamera.ViewportToWorldPoint(Vector3.zero).y;

        // Chỉ hủy khi cạnh trên của sprite đã đi qua cạnh dưới màn hình.
        if (spriteRenderer.bounds.max.y <= screenBottom)
        {
            Destroy(gameObject);
        }
    }

    void MoveHorizontally()
    {
        elapsed += Time.deltaTime;
        Bounds bounds = spriteRenderer.bounds;
        float offsetX = bounds.center.x - transform.position.x;
        float minX = mainCamera.ViewportToWorldPoint(Vector3.zero).x
            + bounds.extents.x - offsetX;
        float maxX = mainCamera.ViewportToWorldPoint(Vector3.one).x
            - bounds.extents.x - offsetX;

        if (minX > maxX)
        {
            return;
        }

        Vector3 position = transform.position;
        if (CurrentMovementType == EnemyMovementType.Zigzag)
        {
            position.x = originX + Mathf.Sin(elapsed * zigzagFrequency * Mathf.PI * 2f)
                * zigzagAmplitude;
        }
        else
        {
            position.x += horizontalDirection * driftSpeed * Time.deltaTime;
            if (position.x <= minX || position.x >= maxX)
            {
                horizontalDirection = -horizontalDirection;
            }
        }

        position.x = Mathf.Clamp(position.x, minX, maxX);
        transform.position = position;
    }
}
