using UnityEngine;

public class BackgroundScroller : MonoBehaviour
{
    public Transform background1;
    public Transform background2;

    [Min(0f)]
    public float scrollSpeed = 2f;

    private float backgroundHeight;
    private SpriteRenderer renderer1;
    private SpriteRenderer renderer2;
    private Camera mainCamera;

    // [BAO VE] Xếp hai nền nối nhau theo chiều cao Sprite trong thế giới để tạo vòng cuộn liên tục.
    void Start()
    {
        if (background1 == null || background2 == null || background1 == background2)
        {
            Debug.LogWarning("Hãy gán hai Background khác nhau cho BackgroundScroller.", this);
            enabled = false;
            return;
        }

        renderer1 = background1.GetComponent<SpriteRenderer>();
        renderer2 = background2.GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;

        if (renderer1 == null || renderer2 == null || mainCamera == null)
        {
            Debug.LogWarning("Hai Background cần SpriteRenderer và camera cần tag MainCamera.", this);
            enabled = false;
            return;
        }

        // Bounds cho biết chiều cao trong thế giới, đã tính cả Scale.
        backgroundHeight = renderer1.bounds.size.y;

        if (backgroundHeight <= 0f || renderer2.bounds.size.y <= 0f)
        {
            Debug.LogWarning("Hãy gán sprite có chiều cao lớn hơn 0 cho hai Background.", this);
            enabled = false;
            return;
        }

        // Hai background dùng cùng sprite và Scale nên có cùng chiều cao.
        background2.position = background1.position + Vector3.up * backgroundHeight;
    }

    // [BAO VE] Cuộn hai nền xuống theo deltaTime; nền ra hẳn dưới Camera được chuyển lên trên nền còn lại để tái sử dụng.
    void Update()
    {
        Vector3 movement = Vector3.down * scrollSpeed * Time.deltaTime;
        background1.position += movement;
        background2.position += movement;

        float screenBottom = mainCamera.ViewportToWorldPoint(Vector3.zero).y;

        // Cạnh trên xuống dưới màn hình nghĩa là toàn bộ background đã ra ngoài.
        // Vòng lặp cũng xử lý được khi một frame di chuyển quá xa.
        while (renderer1.bounds.max.y <= screenBottom || renderer2.bounds.max.y <= screenBottom)
        {
            if (renderer1.bounds.max.y <= screenBottom)
            {
                // [BAO VE - TAI SU DUNG NEN] Đặt nền đã ra khỏi màn hình lên trên nền còn lại; không Instantiate nền mới mỗi frame.
                background1.position = background2.position + Vector3.up * backgroundHeight;
            }
            else
            {
                background2.position = background1.position + Vector3.up * backgroundHeight;
            }
        }
    }
}
