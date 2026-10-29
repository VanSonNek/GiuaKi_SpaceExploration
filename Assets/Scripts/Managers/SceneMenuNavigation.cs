using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

// [BAO VE] Điều hướng Main Menu, chọn Level và quay về Menu bằng tên Scene; không lưu HP hoặc Score giữa các Scene.
public class SceneMenuNavigation : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject levelSelectPanel;

    private bool isLoading;

    void Start()
    {
        // Only the menu owns these panels. Gameplay scenes keep their existing managers.
        if (mainMenuPanel != null && levelSelectPanel != null)
        {
            Time.timeScale = 1f;
            ShowMainMenu();
        }
    }

    // [BAO VE] PLAY/LEVEL 1 tải GameScene; LoadLevel2 và LoadLevel3 tải các Scene tương ứng.
    public void Play()
    {
        LoadScene("GameScene");
    }

    public void LoadLevel2()
    {
        LoadScene("Level2Scene");
    }

    public void LoadLevel3()
    {
        LoadScene("Level3Scene");
    }

    // [BAO VE] Callback MAIN MENU trên màn kết quả, dùng LoadScene chung để khôi phục thời gian trước khi chuyển Scene.
    public void ReturnToMainMenu()
    {
        LoadScene("MainMenuScene");
    }

    // [BAO VE - MO CHON LEVEL] Nút SELECT LEVEL gọi hàm này để ẩn menu chính và hiện danh sách ba màn.
    public void ShowLevelSelect()
    {
        SetMenuPanels(false);
    }

    public void ShowMainMenu()
    {
        SetMenuPanels(true);
    }

    // [BAO VE] Bật Main Menu hoặc khu vực chọn Level theo showMainMenu, đồng thời xóa lựa chọn cũ trong EventSystem.
    void SetMenuPanels(bool showMainMenu)
    {
        if (mainMenuPanel == null || levelSelectPanel == null || isLoading)
        {
            return;
        }

        mainMenuPanel.SetActive(showMainMenu);
        levelSelectPanel.SetActive(!showMainMenu);

        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }

    // [BAO VE] Kiểm tra Scene đã bật trong Build Settings, chặn click lặp, đặt timeScale = 1 rồi thay Scene bằng chế độ Single.
    void LoadScene(string sceneName)
    {
        if (!Application.isPlaying || isLoading)
        {
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogWarning("Scene is missing or disabled in Build Settings: " + sceneName, this);
            return;
        }

        isLoading = true;
        Time.timeScale = 1f;
        // [BAO VE - MENU LOAD SCENE] PLAY, chọn Level và MAIN MENU cùng đi qua lệnh tải Scene này sau kiểm tra và reset thời gian.
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    // [BAO VE] Application.Quit chỉ được biên dịch ngoài Unity Editor; bấm QUIT trong Editor không ép đóng Editor.
    public void QuitGame()
    {
        if (!Application.isPlaying || isLoading)
        {
            return;
        }

#if !UNITY_EDITOR
        Application.Quit();
#endif
    }
}
