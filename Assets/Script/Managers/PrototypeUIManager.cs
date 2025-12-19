using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Prototype 씬 전용 UI 관리
/// (일시정지, 설정만 담당)
/// </summary>
public class PrototypeUIManager : MonoBehaviour
{
    [Header("Settings Panel")]
    [Tooltip("설정 패널 컨트롤러 (공통)")]
    public SettingsPanelController settingsPanelController;

    [Header("Pause Menu")]
    [Tooltip("일시정지 메뉴 (선택적)")]
    public GameObject pauseMenu;

    [Tooltip("일시정지 버튼")]
    public Button pauseButton;

    [Tooltip("재개 버튼")]
    public Button resumeButton;

    void Awake()
    {
        // 일시정지 버튼 연결
        if (pauseButton != null)
        {
            pauseButton.onClick.AddListener(PauseGame);
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.AddListener(ResumeGame);
        }

        // 일시정지 메뉴 초기 상태
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }

        Debug.Log("[PrototypeUIManager] 초기화 완료");
    }

    void OnDestroy()
    {
        if (pauseButton != null)
        {
            pauseButton.onClick.RemoveListener(PauseGame);
        }

        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveListener(ResumeGame);
        }
    }

    /// <summary>
    /// 게임 일시정지
    /// </summary>
    public void PauseGame()
    {
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(true);
            Time.timeScale = 0f;
            Debug.Log("[PrototypeUIManager] 게임 일시정지");
        }
    }

    /// <summary>
    /// 게임 재개
    /// </summary>
    public void ResumeGame()
    {
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
            Time.timeScale = 1f;
            Debug.Log("[PrototypeUIManager] 게임 재개");
        }
    }
}