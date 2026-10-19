using UnityEngine;

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

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 movement = new Vector3(horizontal, vertical, 0);

        transform.position += movement.normalized
                            * moveSpeed
                            * Time.deltaTime;

        ChangeShipSprite(horizontal);

        LimitPlayerInsideScreen();
    }

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

    void LimitPlayerInsideScreen()
    {
        Vector3 bottomLeft =
            mainCamera.ViewportToWorldPoint(new Vector3(0, 0, 0));

        Vector3 topRight =
            mainCamera.ViewportToWorldPoint(new Vector3(1, 1, 0));

        float halfWidth = spriteRenderer.bounds.extents.x;
        float halfHeight = spriteRenderer.bounds.extents.y;

        Vector3 position = transform.position;

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