using System.Collections;
using UnityEngine;

public class DamageFlash : MonoBehaviour
{
    public Color flashColor = new Color(1f, 0.5f, 0.5f, 1f);

    [Min(0f)]
    public float flashDuration = 0.08f;

    private SpriteRenderer spriteRenderer;
    private Color normalColor;
    private Coroutine flashCoroutine;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogWarning("DamageFlash cần SpriteRenderer trên cùng GameObject.", this);
            enabled = false;
        }
    }

    // [BAO VE] Nháy màu Sprite khi nhận sát thương; dừng coroutine cũ nhưng giữ normalColor gốc để trúng liên tiếp vẫn khôi phục đúng màu.
    public void Flash()
    {
        if (spriteRenderer == null || !isActiveAndEnabled)
        {
            return;
        }

        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
        }
        else
        {
            // Chỉ lưu màu khi bắt đầu, tránh lưu nhầm flashColor khi bị trúng liên tiếp.
            normalColor = spriteRenderer.color;
        }

        // [BAO VE - HIEU UNG TRUNG DAN] Chỉ đổi màu Sprite rồi RestoreColor sau flashDuration; hiệu ứng không trừ HP, HP do TakeDamage xử lý.
        spriteRenderer.color = flashColor;
        flashCoroutine = StartCoroutine(RestoreColor());
    }

    // [BAO VE] Chờ flashDuration theo thời gian game rồi trả Sprite về màu ban đầu; OnDisable cũng dọn hiệu ứng còn dở.
    IEnumerator RestoreColor()
    {
        yield return new WaitForSeconds(flashDuration);

        if (spriteRenderer != null)
        {
            spriteRenderer.color = normalColor;
        }

        flashCoroutine = null;
    }

    void OnDisable()
    {
        if (flashCoroutine != null)
        {
            StopCoroutine(flashCoroutine);
            flashCoroutine = null;

            if (spriteRenderer != null)
            {
                spriteRenderer.color = normalColor;
            }
        }
    }
}
