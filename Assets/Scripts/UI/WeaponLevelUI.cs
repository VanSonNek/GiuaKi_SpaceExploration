using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(TextMeshProUGUI))]
// [BAO VE] Hiển thị cấp súng bằng TMP_Text; nội dung lấy từ WeaponLevel của PlayerShooting.
public class WeaponLevelUI : MonoBehaviour
{
    [SerializeField] private PlayerShooting playerShooting;

    private PlayerShooting subscribedPlayer;
    private TMP_Text label;

    // [BAO VE] Hiển thị cấp hiện tại và nghe WeaponLevelChanged; tắt raycastTarget để chữ HUD không chặn thao tác chuột.
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

    // [BAO VE - CAP NHAT WEAPON UI] Callback của WeaponLevelChanged ghi WEAPON LV. kèm cấp súng mới vào TMP_Text.
    void ShowLevel(int level)
    {
        if (label != null) label.text = "WEAPON LV." + level;
    }

    // [BAO VE] Gỡ listener ShowLevel khi UI tắt để không cập nhật lặp sau Retry hoặc chuyển Scene.
    void OnDisable()
    {
        if (subscribedPlayer != null) subscribedPlayer.WeaponLevelChanged -= ShowLevel;
        subscribedPlayer = null;
    }
}
