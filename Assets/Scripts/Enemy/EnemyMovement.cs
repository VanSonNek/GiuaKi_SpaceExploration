using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Min(0f)]
    public float moveSpeed = 2f;

    private Camera mainCamera;
    private SpriteRenderer spriteRenderer;

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
    }

    void Update()
    {
        // Bay thẳng xuống theo trục Y của thế giới.
        transform.position += Vector3.down * moveSpeed * Time.deltaTime;

        float screenBottom = mainCamera.ViewportToWorldPoint(Vector3.zero).y;

        // Chỉ hủy khi cạnh trên của sprite đã đi qua cạnh dưới màn hình.
        if (spriteRenderer.bounds.max.y <= screenBottom)
        {
            Destroy(gameObject);
        }
    }
}
