using UnityEngine;
using UnityEngine.UI;

public class StartSceneUIManager : MonoBehaviour
{
    [Header("Button References")]
    [Tooltip("게임 시작 버튼")]
    public Button nextSceneButton;

    [Header("Settings Panel")]
    [Tooltip("설정 패널 컨트롤러 (공통)")]
    public SettingsPanelController settingsPanelController;

    [Header("Scene Transition Settings")]
    public string nextSceneName = SceneNames.PROTOTYPE;

    [Header("Events")]
    [Tooltip("게임 시작 이벤트 (선택적)")]
    public GameEvent startGameEvent;

    private ISceneService sceneService;

    void Awake()
    {
        sceneService = ServiceLocator.Resolve<ISceneService>();

        if (sceneService == null)
        {
            Debug.LogWarning("[StartSceneUIManager] ISceneService를 찾을 수 없습니다. 씬 전환이 정상 작동하지 않을 수 있습니다.");
        }

        // 게임 시작 이벤트 연결
        if (nextSceneButton != null)
        {
            nextSceneButton.onClick.AddListener(StartGame);
        }

        if (string.IsNullOrEmpty(nextSceneName))
        {
            nextSceneName = SceneNames.PROTOTYPE; // 기본값
            Debug.LogWarning("[StartSceneUIManager] Next Scene Name이 설정되지 않아 'Prototype'을 기본값으로 사용합니다.");
        }

        Debug.Log($"[StartSceneUIManager] 초기화 완료 - 다음 씬: {nextSceneName}");
    }

    void OnDestroy()
    {
        if (nextSceneButton != null)
        {
            nextSceneButton.onClick.RemoveListener(StartGame);
        }
    }

    public void StartGame()
    {
        // 중복 클릭 방지
        if (sceneService != null && sceneService.IsLoading)
        {
            Debug.LogWarning("[StartSceneUIManager] 이미 씬 로딩 중입니다!");
            return;
        }

        // 버튼 비활성화
        if (nextSceneButton != null)
        {
            nextSceneButton.interactable = false;
        }

        // 설정 버튼도 비활성화
        if (settingsPanelController != null && settingsPanelController.settingsButton != null)
        {
            settingsPanelController.settingsButton.interactable = false;
        }

        // 1. GameEvent 발생 (선택적: 예: BGM FADE OUT 등)
        if (startGameEvent != null)
        {
            startGameEvent.Raise();
        }

        if (sceneService != null)
        {
            Debug.Log($"[StartSceneUIManager] SceneService로 씬 전환 시작: {GetCurrentSceneName()} → {nextSceneName}");
            sceneService.LoadSceneWithLoading(nextSceneName);
        }
        else
        {
            // SceneService가 없으면 기존 방식 사용 (폴백)
            Debug.LogWarning("[StartSceneUIManager] SceneService가 없어서 직접 씬 로드합니다.");
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        }
    }

    string GetCurrentSceneName()
    {
        return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
    }
}