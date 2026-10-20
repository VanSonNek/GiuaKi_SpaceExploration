using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

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

    public void ReturnToMainMenu()
    {
        LoadScene("MainMenuScene");
    }

    public void ShowLevelSelect()
    {
        SetMenuPanels(false);
    }

    public void ShowMainMenu()
    {
        SetMenuPanels(true);
    }

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
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

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
