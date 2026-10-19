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

        spriteRenderer.color = flashColor;
        flashCoroutine = StartCoroutine(RestoreColor());
    }

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
