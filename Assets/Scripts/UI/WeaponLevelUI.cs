using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(TextMeshProUGUI))]
public class WeaponLevelUI : MonoBehaviour
{
    [SerializeField] private PlayerShooting playerShooting;

    private PlayerShooting subscribedPlayer;
    private TMP_Text label;

    void OnEnable()
    {
        label = GetComponent<TMP_Text>();
        label.raycastTarget = false;
        if (!Application.isPlaying)
        {
            ShowLevel(1);
            return;
        }

        if (playerShooting == null)
        {
            Debug.LogWarning("WeaponLevelUI needs this Scene's PlayerShooting.", this);
            enabled = false;
            return;
        }

        subscribedPlayer = playerShooting;
        subscribedPlayer.WeaponLevelChanged += ShowLevel;
        ShowLevel(subscribedPlayer.WeaponLevel);
    }

    void ShowLevel(int level)
    {
        if (label != null) label.text = "WEAPON LV." + level;
    }

    void OnDisable()
    {
        if (subscribedPlayer != null) subscribedPlayer.WeaponLevelChanged -= ShowLevel;
        subscribedPlayer = null;
    }
}
