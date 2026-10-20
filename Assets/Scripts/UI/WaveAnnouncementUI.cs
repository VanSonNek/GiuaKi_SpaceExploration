using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform), typeof(CanvasGroup))]
public class WaveAnnouncementUI : MonoBehaviour
{
    [SerializeField] private Level1Controller levelController;
    [SerializeField] private TMP_Text headingText;
    [SerializeField] private TMP_Text subtitleText;
    [SerializeField] private Image background;
    [SerializeField] private Image accent;
    [SerializeField] private Outline border;
    [SerializeField, Min(0.1f)] private float waveDuration = 1.4f;
    [SerializeField, Min(0f)] private float fadeDuration = 0.18f;
    [SerializeField, Min(0f)] private float slideDistance = 12f;

    private Level1Controller subscribedController;
    private GameResultManager resultManager;
    private PlayerHealth subscribedPlayer;
    private CanvasGroup canvasGroup;
    private RectTransform banner;
    private Vector2 restingPosition;
    private Coroutine animation;

    void OnEnable()
    {
        banner = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        restingPosition = banner.anchoredPosition;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        HideImmediate();

        if (levelController == null || headingText == null || subtitleText == null
            || background == null || accent == null || border == null)
        {
            Debug.LogWarning("WaveAnnouncementUI needs its controller and banner references.", this);
            enabled = false;
            return;
        }

        subscribedController = levelController;
        resultManager = levelController.gameResultManager;
        subscribedController.WaveStarted += ShowWave;
        subscribedController.BossWarningStarted += ShowBossWarning;

        subscribedPlayer = resultManager == null ? null : resultManager.playerHealth;
        if (subscribedPlayer != null)
        {
            subscribedPlayer.Died += HideImmediate;
        }
    }

    void ShowWave(int number, int total, string waveName)
    {
        Show("WAVE " + number + " / " + total, waveName, waveDuration, false);
    }

    void ShowBossWarning(float existingDelay)
    {
        // Use the controller's existing rest; this UI never delays Boss spawning.
        Show("WARNING", "BOSS INCOMING", existingDelay, true);
    }

    void Show(string heading, string subtitle, float duration, bool warning)
    {
        if (!isActiveAndEnabled || duration <= 0f
            || (resultManager != null && resultManager.IsGameFinished))
        {
            return;
        }

        HideImmediate();
        Color tone = warning ? new Color(1f, 0.36f, 0.16f) : new Color(0.25f, 0.85f, 1f);
        headingText.text = heading;
        subtitleText.text = string.IsNullOrEmpty(subtitle) ? "INCOMING" : subtitle;
        headingText.color = tone;
        accent.color = tone;
        border.effectColor = new Color(tone.r, tone.g, tone.b, 0.85f);
        background.color = warning
            ? new Color(0.18f, 0.035f, 0.045f, 0.96f)
            : new Color(0.025f, 0.09f, 0.15f, 0.96f);
        animation = StartCoroutine(Animate(duration));
    }

    IEnumerator Animate(float duration)
    {
        float elapsed = 0f;
        float fade = Mathf.Min(fadeDuration, duration * 0.5f);

        while (elapsed < duration)
        {
            if (resultManager != null && resultManager.IsGameFinished)
            {
                break;
            }

            float fadeIn = fade > 0f ? Mathf.Clamp01(elapsed / fade) : 1f;
            float fadeOut = fade > 0f ? Mathf.Clamp01((duration - elapsed) / fade) : 1f;
            canvasGroup.alpha = Mathf.Min(fadeIn, fadeOut);
            banner.anchoredPosition = restingPosition + Vector2.up * (slideDistance * (1f - fadeIn));
            elapsed += Time.deltaTime;
            yield return null;
        }

        ResetVisuals();
        animation = null;
    }

    void HideImmediate()
    {
        if (animation != null)
        {
            StopCoroutine(animation);
            animation = null;
        }

        ResetVisuals();
    }

    void ResetVisuals()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        if (banner != null)
        {
            banner.anchoredPosition = restingPosition;
        }
    }

    void OnDisable()
    {
        if (subscribedController != null)
        {
            subscribedController.WaveStarted -= ShowWave;
            subscribedController.BossWarningStarted -= ShowBossWarning;
        }

        if (subscribedPlayer != null)
        {
            subscribedPlayer.Died -= HideImmediate;
        }

        subscribedController = null;
        subscribedPlayer = null;
        resultManager = null;
        HideImmediate();
    }
}
