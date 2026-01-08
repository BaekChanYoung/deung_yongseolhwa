using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 설정 패널 열기/닫기 공통 로직
/// 모든 씬에서 재사용 가능
/// </summary>
public class SettingsPanelController : MonoBehaviour
{
    [Header("Panel References")]
    [Tooltip("설정 패널 GameObject")]
    public GameObject settingsPanel;

    [Header("Button References")]
    [Tooltip("설정 열기 버튼")]
    public Button settingsButton;

    [Tooltip("설정 닫기 버튼")]
    public Button closeSettingsButton;

    [Header("Game Control Buttons")]
    [Tooltip("메인 메뉴(로비)로 돌아가기 버튼")]
    public Button lobbyButton;

    [Tooltip("게임 재시작 버튼")]
    public Button restartButton;

    [Header("Dialogue Replay")]
    [Tooltip("다이얼로그 다시보기 버튼 (선택사항)")]
    public Button replayDialogueButton;

    [Header("Panel Type")]
    [Tooltip("패널 타입 (씬에 따라 다른 버튼 표시)")]
    public PanelType panelType = PanelType.MenuOnly;

    public enum PanelType
    {
        MenuOnly,    // 메뉴 씬 (닫기만)
        Gameplay     // 게임 씬 (닫기 + 로비 + 재시작)
    }

    [Header("Animator References")]
    [Tooltip("설정 패널 Animator")]
    public Animator settingsPanelAnimator;

    [Header("Content References")]
    [Tooltip("볼륨 슬라이더 UI")]
    public VolumeSlidersUI volumeSlidersUI;

    private ISceneService sceneService;

    void Awake()
    {
        sceneService = ServiceLocator.Resolve<ISceneService>();

        // 버튼 이벤트 연결
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);

        if (closeSettingsButton != null)
            closeSettingsButton.onClick.AddListener(CloseSettings);

        if (lobbyButton != null)
            lobbyButton.onClick.AddListener(GoToLobby);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        if (replayDialogueButton != null)
            replayDialogueButton.onClick.AddListener(ReplayDialogue);

        // Animator 자동 찾기
        if (settingsPanelAnimator == null && settingsPanel != null)
        {
            settingsPanelAnimator = settingsPanel.GetComponent<Animator>();
        }

        // Animator를 Unscaled Time으로 설정 (일시정지 중에도 작동)
        if (settingsPanelAnimator != null)
        {
            settingsPanelAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
        }

        SetupButtonVisibility();

        Debug.Log("[SettingsPanelController] 초기화 완료 - 타입: {panelType}");
    }

    void OnDestroy()
    {
        // 이벤트 해제
        if (settingsButton != null)
            settingsButton.onClick.RemoveListener(OpenSettings);

        if (closeSettingsButton != null)
            closeSettingsButton.onClick.RemoveListener(CloseSettings);

        if (lobbyButton != null)
            lobbyButton.onClick.RemoveListener(GoToLobby);

        if (restartButton != null)
            restartButton.onClick.RemoveListener(RestartGame);

        if (replayDialogueButton != null)
            replayDialogueButton.onClick.RemoveListener(ReplayDialogue);
    }

    /// <summary>
    /// 패널 타입에 따라 버튼 표시 설정
    /// </summary>
    void SetupButtonVisibility()
    {
        switch (panelType)
        {
            case PanelType.MenuOnly:
                // 메뉴 씬: 로비/재시작 버튼 숨김
                if (lobbyButton != null)
                    lobbyButton.gameObject.SetActive(false);

                if (restartButton != null)
                    restartButton.gameObject.SetActive(false);

                if (replayDialogueButton != null)
                    replayDialogueButton.gameObject.SetActive(true);

                Debug.Log("[SettingsPanelController] 메뉴 전용 모드 (닫기만)");
                break;

            case PanelType.Gameplay:
                // 게임 씬: 로비/재시작 버튼 표시
                if (lobbyButton != null)
                    lobbyButton.gameObject.SetActive(true);

                if (restartButton != null)
                    restartButton.gameObject.SetActive(true);

                if (replayDialogueButton != null)
                    replayDialogueButton.gameObject.SetActive(false);

                Debug.Log("[SettingsPanelController] 게임플레이 모드 (닫기 + 로비 + 재시작)");
                break;
        }
    }
    /// <summary>
    /// 설정 패널 열기
    /// </summary>
    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            // 게임플레이 중이면 일시정지
            if (panelType == PanelType.Gameplay)
            {
                Time.timeScale = 0f;
                Debug.Log("[SettingsPanelController] 게임 일시정지");
            }

            settingsPanel.SetActive(true);

            if (settingsPanelAnimator != null)
            {
                settingsPanelAnimator.SetBool("IsOpen", true);
            }

            Debug.Log("[SettingsPanelController] 설정 패널 열림");
        }
    }

    /// <summary>
    /// 설정 패널 닫기
    /// </summary>
    public void CloseSettings()
    {
        if (settingsPanel != null && settingsPanelAnimator != null)
        {
            settingsPanelAnimator.SetBool("IsOpen", false);

            // 내용물 페이드 아웃
            if (volumeSlidersUI != null)
            {
                volumeSlidersUI.StartContentFadeOut();
            }

            // 대기 시간 계산
            float waitTime = Mathf.Max(
                settingsPanelAnimator.GetCurrentAnimatorStateInfo(0).length,
                volumeSlidersUI != null ? volumeSlidersUI.fadeDuration : 0f
            );

            StartCoroutine(DeactivatePanelAfterAnimation(waitTime));

            if (panelType == PanelType.Gameplay)
            {
                Time.timeScale = 1f;
                Debug.Log("[SettingsPanelController] 게임 재개");
            }

            Debug.Log("[SettingsPanelController] 설정 패널 닫힘");
        }
    }

    IEnumerator DeactivatePanelAfterAnimation(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);

        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 로비(메인 메뉴)로 돌아가기
    /// </summary>
    void GoToLobby()
    {
        Debug.Log("[SettingsPanelController] 로비로 이동");

        // 시간 복구
        Time.timeScale = 1f;

        // 씬 전환
        if (sceneService != null)
        {
            sceneService.LoadSceneWithLoading(SceneNames.START);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.START);
        }
    }

    /// <summary>
    /// 게임 재시작
    /// </summary>
    void RestartGame()
    {
        Debug.Log("[SettingsPanelController] 게임 재시작");

        // 시간 복구
        Time.timeScale = 1f;

        // 현재 씬 다시 로드
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        if (sceneService != null)
        {
            sceneService.LoadSceneWithLoading(currentScene);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(currentScene);
        }
    }

    /// <summary>
    /// 다이얼로그 다시보기 실행
    /// </summary>
    void ReplayDialogue()
    {
        Debug.Log("========================================");
        Debug.Log("[SettingsPanelController] 다이얼로그 다시보기 시작");
        Debug.Log("========================================");

        // 서비스 확인
        if (sceneService == null)
        {
            Debug.LogError("[SettingsPanelController] ISceneService를 찾을 수 없습니다!");
            return;
        }

        // 최초 실행 플래그 초기화
        FirstTimeManager.ResetFirstTimeFlag();
        Debug.Log("[SettingsPanelController] 최초 실행 플래그 초기화 완료");

        // DialogueScene으로 이동
        Debug.Log("[SettingsPanelController] DialogueScene으로 이동");
        sceneService.LoadSceneWithLoading(SceneNames.DIALOGUE);
    }
}