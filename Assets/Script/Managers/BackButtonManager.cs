using UnityEngine;

/// <summary>
/// Android 뒤로가기 버튼 관리 (씬별 동작)
/// </summary>
public class BackButtonManager : MonoBehaviour
{
    [Header("Scene Type")]
    public SceneType sceneType = SceneType.Menu;

    public enum SceneType
    {
        Menu,       // TitleScene, StartScene - 종료 확인
        Loading,    // LoadingScene - 무시
        Gameplay    // Prototype - 일시정지
    }

    [Header("References")]
    public UniversalModalController modalController;

    [Header("Debug")]
    [ReadOnly]
    [SerializeField]
    private bool isModalOpen = false;

    [ReadOnly]
    [SerializeField]
    private bool isProcessing = false;

    private ISceneService sceneService;
    private SettingsPanelController settingsPanelController;

    void Start()
    {
        sceneService = ServiceLocator.Resolve<ISceneService>();

        // Gameplay 씬이면 SettingsPanelController 미리 찾기
        if (sceneType == SceneType.Gameplay)
        {
            settingsPanelController = FindObjectOfType<SettingsPanelController>();
            if (settingsPanelController == null)
            {
                Debug.LogWarning("[BackButton] SettingsPanelController를 찾을 수 없습니다!");
            }
        }
    }

    void Update()
    {
        // Android 뒤로가기 버튼 (PC에서는 ESC)
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleBackButton();
        }
    }

    void HandleBackButton()
    {
        if (isProcessing)
        {
            Debug.Log("[BackButton] 이미 처리 중입니다.");
            return;
        }

        if (sceneService != null && sceneService.IsLoading)
        {
            Debug.Log("[BackButton] 씬 전환 중 - 무시");
            return;
        }

        switch (sceneType)
        {
            case SceneType.Menu:
                HandleMenuScene();
                break;

            case SceneType.Loading:
                Debug.Log("[BackButton] 로딩 중 - 무시");
                break;

            case SceneType.Gameplay:
                HandleGameplayScene();
                break;
        }
    }

    void HandleMenuScene()
    {
        if (isModalOpen)
        {
            Debug.Log("[BackButton] 모달 닫기");
            if (modalController != null)
            {
                modalController.Hide();
                isModalOpen = false;
            }
            return;
        }

        // 모달 열기
        ShowExitConfirmModal();
    }

    void HandleGameplayScene()
    {
        if (GameManager.instance != null && GameManager.instance.player.isDead)
        {
            Debug.Log("[BackButton] 플레이어 사망 - 설정 열기 불가");
            return;
        }

        if (settingsPanelController == null)
        {
            Debug.LogWarning("[BackButton] SettingsPanelController를 찾을 수 없습니다!");
            return;
        }

        bool isPanelOpen = settingsPanelController.settingsPanel != null && settingsPanelController.settingsPanel.activeSelf;

        if (isPanelOpen)
        {
            Debug.Log("[BackButton] 설정 패널 닫기");
            settingsPanelController.CloseSettings();
        }
        else
        {
            Debug.Log("[BackButton] 설정 패널 열기");
            settingsPanelController.OpenSettings();
        }
    }

    /// <summary>
    /// 게임 종료 확인 모달 (TitleScene, StartScene)
    /// </summary>
    void ShowExitConfirmModal()
    {
        if (modalController != null)
        {
            isProcessing = true;
            isModalOpen = true;

            modalController.Show(
                title: "경고!",
                message: "게임을 종료 하시겠습니까?",
                leftText: "예",
                rightText: "아니요",
                onLeft: QuitGame,
                onRight: OnModalClosed // 아무것도 안 함 (모달만 닫힘)
            );
        }
    }

    void OnModalClosed()
    {
        isModalOpen = false;
        isProcessing = false;
        Debug.Log("[BackButton] 모달 닫힘");
    }

    void QuitGame()
    {
        Debug.Log("[BackButton] 게임 종료");

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}