using UnityEngine;

// [BAO VE] Điều khiển máy bay bằng input, đổi Sprite theo hướng và giới hạn vị trí trong Camera.
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    // 3 hình của tàu
    public Sprite leftSprite;
    public Sprite straightSprite;
    public Sprite rightSprite;

    private Camera mainCamera;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        mainCamera = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // [BAO VE] Đọc Horizontal/Vertical; normalized tránh đi chéo nhanh hơn, moveSpeed * deltaTime tính quãng đường mỗi frame.
    void Update()
    {
        // [BAO VE - PLAYER NHAN PHIM] Horizontal/Vertical đang được InputManager ánh xạ A/D, W/S và các phím mũi tên.
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 movement = new Vector3(horizontal, vertical, 0);

        transform.position += movement.normalized
                            * moveSpeed
                            * Time.deltaTime;

        ChangeShipSprite(horizontal);

        LimitPlayerInsideScreen();
    }

    // [BAO VE] Chọn leftSprite, rightSprite hoặc straightSprite theo dấu của input ngang.
    void ChangeShipSprite(float horizontal)
    {
        if (horizontal < 0)
        {
            spriteRenderer.sprite = leftSprite;
        }
        else if (horizontal > 0)
        {
            spriteRenderer.sprite = rightSprite;
        }
        else
        {
            spriteRenderer.sprite = straightSprite;
        }
    }

    // [BAO VE] Đổi hai góc Viewport sang tọa độ thế giới; Mathf.Clamp có tính nửa kích thước Sprite để tàu không vượt màn hình.
    void LimitPlayerInsideScreen()
    {
        Vector3 bottomLeft =
            mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0));

        Vector3 topRight =
            mainCamera.ViewportToWorldPoint(new Vector3(1, 1, 0));

        // [BAO VE - PLAYER SPRITE BOUNDS] bounds.extents là nửa kích thước Sprite trong thế giới, đã tính Scale để chừa mép thân tàu.
        float halfWidth = spriteRenderer.bounds.extents.x;
        float halfHeight = spriteRenderer.bounds.extents.y;

        Vector3 position = transform.position;

        // [BAO VE - PLAYER CLAMP CAMERA] Clamp giới hạn tọa độ vào khoảng min/max; X và Y đều chừa nửa kích thước Sprite ở hai mép.
        position.x = Mathf.Clamp(
            position.x,
            bottomLeft.x + halfWidth,
            topRight.x - halfWidth
        );

        position.y = Mathf.Clamp(
            position.y,
            bottomLeft.y + halfHeight,
            topRight.y - halfHeight
        );

        transform.position = position;
    }
}