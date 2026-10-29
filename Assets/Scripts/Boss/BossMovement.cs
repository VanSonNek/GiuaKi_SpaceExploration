using UnityEngine;

// [BAO VE] Cho Boss bay vào vùng chiến đấu rồi di chuyển ngang; IsInPosition cho BossShooting biết khi nào được bắn.
public class BossMovement : MonoBehaviour
{
    [Min(0f)]
    public float entrySpeed = 2f;

    [Min(0f)]
    public float moveSpeed = 2f;

    // 0.78 nghĩa là tâm sprite ở mức 78% chiều cao màn hình, tính từ dưới lên.
    [Range(0.75f, 0.8f)]
    public float battleHeight = 0.78f;

    public bool IsInPosition { get; private set; }

    private Camera mainCamera;
    private SpriteRenderer spriteRenderer;
    private float horizontalDirection = 1f;

    // [BAO VE] Lấy Camera/Sprite và đưa toàn bộ thân Boss lên ngoài cạnh trên để bắt đầu bay vào màn hình.
    void Start()
    {
        mainCamera = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (mainCamera == null || spriteRenderer == null || spriteRenderer.sprite == null)
        {
            Debug.LogWarning("Boss cần SpriteRenderer có sprite và camera cần tag MainCamera.", this);
            enabled = false;
            return;
        }

        // Tự đặt toàn bộ Boss ngay phía trên camera để test prefab độc lập.
        float screenTop = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, 1f, 0f)).y;
        transform.position += Vector3.up * (screenTop + 0.1f - spriteRenderer.bounds.min.y);
    }

    // [BAO VE] MoveTowards đưa Boss tới battleHeight; sau đó bay ngang, Clamp và đổi hướng tại biên theo Bounds gồm cả pivot.
    void Update()
    {
        Vector3 bottomLeft = mainCamera.ViewportToWorldPoint(Vector3.zero);
        Vector3 topRight = mainCamera.ViewportToWorldPoint(new Vector3(1f, 1f, 0f));
        Bounds bounds = spriteRenderer.bounds;
        Vector3 centerOffset = bounds.center - transform.position;

        // Tính cả kích thước và pivot để toàn bộ sprite nằm trong chiều rộng camera.
        float minX = bottomLeft.x + bounds.extents.x - centerOffset.x;
        float maxX = topRight.x - bounds.extents.x - centerOffset.x;

        if (minX > maxX)
        {
            Debug.LogWarning("Boss quá rộng so với camera. Hãy giảm Scale của Boss.", this);
            enabled = false;
            return;
        }

        Vector3 position = transform.position;
        position.x = Mathf.Clamp(position.x, minX, maxX);

        if (!IsInPosition)
        {
            float targetY = mainCamera.ViewportToWorldPoint(new Vector3(0.5f, battleHeight, 0f)).y
                            - centerOffset.y;

            // MoveTowards giúp Boss dừng đúng vị trí, không đi quá đích.
            // [BAO VE - BOSS BAY VAO] Tiến dần tới độ cao chiến đấu theo entrySpeed; IsInPosition chỉ true khi đã tới đích.
            position.y = Mathf.MoveTowards(position.y, targetY, entrySpeed * Time.deltaTime);
            IsInPosition = Mathf.Approximately(position.y, targetY);
        }
        else
        {
            position.x += horizontalDirection * moveSpeed * Time.deltaTime;

            // [BAO VE - BOSS DOI HUONG] Chạm biên phải thì bay trái, chạm biên trái thì bay phải; minX/maxX đã tính toàn bộ kích thước Sprite.
            if (position.x >= maxX)
            {
                position.x = maxX;
                horizontalDirection = -1f;
            }
            else if (position.x <= minX)
            {
                position.x = minX;
                horizontalDirection = 1f;
            }
        }

        transform.position = position;
    }
}
