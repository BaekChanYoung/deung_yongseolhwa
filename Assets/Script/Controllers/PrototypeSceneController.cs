using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Prototype 씬 컨트롤러
/// 개발/테스트용 빠른 이동 버튼 관리
/// </summary>
public class PrototypeSceneController : MonoBehaviour
{
    [Header("UI Buttons")]
    [Tooltip("StartScene으로 빠른 이동 (개발용)")]
    public Button backToStartButton;

    [Header("Audio")]
    public AudioClip clickSfx;

    private ISceneService sceneService;
    private IAudioService audioService;

    void Start()
    {
        sceneService = ServiceLocator.Resolve<ISceneService>();
        audioService = ServiceLocator.Resolve<IAudioService>();

        if (sceneService == null)
        {
            Debug.LogError("[PrototypeSceneController] ISceneService를 찾을 수 없습니다!");
        }

        // 버튼 이벤트 연결
        if (backToStartButton != null)
        {
            backToStartButton.onClick.AddListener(OnBackToStartClicked);
        }

        Debug.Log("[PrototypeSceneController] 초기화 완료");
    }

    void OnDestroy()
    {
        if (backToStartButton != null)
        {
            backToStartButton.onClick.RemoveListener(OnBackToStartClicked);
        }
    }

    /// <summary>
    /// StartScene으로 빠른 이동 (개발/테스트용)
    /// </summary>
    void OnBackToStartClicked()
    {
        // 중복 클릭 방지
        if (sceneService != null && sceneService.IsLoading)
        {
            Debug.LogWarning("[PrototypeSceneController] 이미 씬 로딩 중입니다!");
            return;
        }

        Debug.Log("[PrototypeSceneController] StartScene으로 빠른 이동!");

        // 효과음 재생
        if (audioService != null && clickSfx != null)
        {
            audioService.PlaySfx(clickSfx, 0.7f);
        }

        // 버튼 비활성화
        if (backToStartButton != null)
        {
            backToStartButton.interactable = false;
        }

        // 시간 복구 (일시정지 상태일 수 있으므로)
        Time.timeScale = 1f;

        // StartScene으로 전환
        if (sceneService != null)
        {
            sceneService.LoadSceneWithLoading(SceneNames.START);
        }
        else
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(SceneNames.START);
        }
    }
}