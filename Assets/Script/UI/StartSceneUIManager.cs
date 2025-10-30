using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StartSceneUIManager : MonoBehaviour
{
    [Header("Panel References")]
    public GameObject settingsPanel; // 설정 창 GameObject

    [Header("Button References")]
    public Button settingsButton;    // 설정 UI 열기 버튼
    public Button closeSettingsButton; // 설정 창 닫기 버튼
    public Button NextSceneButton;       // 게임 시작 버튼

    [Header("Animator References")]
    public Animator settingsPanelAnimator;

    [Header("Scene Transition Settings")]
    public string nextSceneName = SceneNames.PROTOTYPE;

    [Header("Events")]
    public GameEvent startGameEvent; // 게임 시작 시 발생시킬 GameEvent (선택적)

    [Header("References")]
    public VolumeSlidersUI volumeSlidersUI;

    private ISceneService sceneService;

    void Awake()
    {
        sceneService = ServiceLocator.Resolve<ISceneService>();

        if (sceneService == null)
        {
            Debug.LogWarning("[StartSceneUIManager] ISceneService를 찾을 수 없습니다. 씬 전환이 정상 작동하지 않을 수 있습니다.");
        }

        // 팝업 관리 이벤트 연결
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);

        if (closeSettingsButton != null)
            closeSettingsButton.onClick.AddListener(CloseSettings);

        // 게임 시작 이벤트 연결
        if (NextSceneButton != null)
            NextSceneButton.onClick.AddListener(StartGame);

        if (settingsPanelAnimator == null && settingsPanel != null)
        {
            settingsPanelAnimator = settingsPanel.GetComponent<Animator>();
        }

        if (string.IsNullOrEmpty(nextSceneName))
        {
            nextSceneName = SceneNames.PROTOTYPE; // 기본값
            Debug.LogWarning("[StartSceneUIManager] Next Scene Name이 설정되지 않아 'Prototype'을 기본값으로 사용합니다.");
        }
    }

    public void OpenSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);

            if (settingsPanelAnimator != null)
            {
                settingsPanelAnimator.SetBool("IsOpen", true); // 애니메이션 재생
            }
        }
    }

    public void CloseSettings()
    {
        if (settingsPanel != null && settingsPanelAnimator != null)
        {
            settingsPanelAnimator.SetBool("IsOpen", false); // 닫기 애니메이션 재생

            // 1. 내용물 페이드 아웃 시작
            if (volumeSlidersUI != null)
            {
                volumeSlidersUI.StartContentFadeOut(); // 페이드 아웃 시작 (Alpha 0f로)
            }

            // 2. 족자봉 닫기 애니메이션과 페이드 아웃 시간 중 긴 쪽을 기다립니다.
            float waitTime = Mathf.Max(
                settingsPanelAnimator.GetCurrentAnimatorStateInfo(0).length,
                volumeSlidersUI.fadeDuration
            );

            StartCoroutine(DeactivatePanelAfterAnimation(waitTime));
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

    public void StartGame()
    {
        if (sceneService != null && sceneService.IsLoading)
        {
            Debug.LogWarning("[StartSceneUIManager] 이미 씬 로딩 중입니다!");
            return;
        }

        // 게임 시작 버튼을 누르면 다른 버튼들을 비활성화하여 중복 클릭 방지
        if (settingsButton != null)
            settingsButton.interactable = false;

        if (NextSceneButton != null) 
            NextSceneButton.interactable = false;

        // 1. GameEvent 발생 (선택적: 예: BGM FADE OUT 등)
        if (startGameEvent != null)
            startGameEvent.Raise();

        if (sceneService != null)
        {
            Debug.Log($"[StartSceneUIManager] SceneService로 씬 전환: {nextSceneName}");
            sceneService.LoadSceneWithLoading(nextSceneName);
        }

        else
        {
            // SceneService가 없으면 기존 방식 사용 (폴백)
            Debug.LogWarning("[StartSceneUIManager] SceneService가 없어서 직접 씬 로드합니다.");
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
        }
    }

}