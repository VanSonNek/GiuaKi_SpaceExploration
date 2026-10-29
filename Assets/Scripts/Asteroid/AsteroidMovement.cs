using UnityEngine;

public class AsteroidMovement : MonoBehaviour
{
    [Min(0f)]
    public float moveSpeed = 2.5f;

    // Tốc độ xoay tính bằng độ mỗi giây. Giá trị âm đổi chiều xoay.
    public float rotationSpeed = 60f;

    private Camera mainCamera;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        mainCamera = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (mainCamera == null || spriteRenderer == null)
        {
            Debug.LogWarning("Asteroid cần SpriteRenderer và camera cần tag MainCamera.", this);
            enabled = false;
            return;
        }
    }

    // [BAO VE] Cho thiên thạch vừa rơi vừa xoay theo deltaTime; chỉ Destroy khi cạnh trên Sprite đã xuống dưới Camera.
    void Update()
    {
        transform.position += Vector3.down * moveSpeed * Time.deltaTime;
        // [BAO VE - ASTEROID XOAY] Xoay quanh trục Z với số độ mỗi giây; chuyển động rơi nằm ở dòng phía trên và độc lập với góc xoay.
        transform.Rotate(0f, 0f, rotationSpeed * Time.deltaTime);

        float screenBottom = mainCamera.ViewportToWorldPoint(Vector3.zero).y;

        // Bounds được cập nhật theo góc xoay và Scale hiện tại của sprite.
        if (spriteRenderer.bounds.max.y <= screenBottom)
        {
            Destroy(gameObject);
        }
    }
}
