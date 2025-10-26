using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // 씬 로딩을 위해 필요

public class StartSceneUIManager : MonoBehaviour
{
    [Header("Panel References")]
    public GameObject settingsPanel; // 설정 창 GameObject

    [Header("Button References")]
    public Button settingsButton;    // 설정 UI 열기 버튼
    public Button closeSettingsButton; // 설정 창 닫기 버튼
    public Button startButton;       // 게임 시작 버튼

    [Header("Animator References")]
    public Animator settingsPanelAnimator;

    public GameObject playTransitionEffectPanel; // 이펙트 Image를 포함하는 GameObject
    public Animator playTransitionAnimator;      // 이펙트 Animator

    [Header("Scene Transition Settings")]
    public string nextSceneName = "Timer";

    [Header("Events")]
    public GameEvent startGameEvent; // 게임 시작 시 발생시킬 GameEvent (선택적)

    public VolumeSlidersUI volumeSlidersUI;

    void Awake()
    {
        // 팝업 관리 이벤트 연결
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OpenSettings);

        if (closeSettingsButton != null)
            closeSettingsButton.onClick.AddListener(CloseSettings);

        // 게임 시작 이벤트 연결
        if (startButton != null)
            startButton.onClick.AddListener(StartGame);

        if (settingsPanelAnimator == null && settingsPanel != null)
        {
            settingsPanelAnimator = settingsPanel.GetComponent<Animator>();
        }

        // 트랜지션 이펙트 Animator 연결
        if (playTransitionEffectPanel != null && playTransitionAnimator == null)
        {
            playTransitionAnimator = playTransitionEffectPanel.GetComponent<Animator>();
        }
        // 초기에는 이펙트 패널 비활성화
        if (playTransitionEffectPanel != null)
        {
            playTransitionEffectPanel.SetActive(false);
        }

        if (string.IsNullOrEmpty(nextSceneName))
        {
            nextSceneName = "Timer"; // 기본값
            Debug.LogWarning("[StartSceneUIManager] Next Scene Name이 설정되지 않아 'Timer'을 기본값으로 사용합니다.");
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
        // 게임 시작 버튼을 누르면 다른 버튼들을 비활성화하여 중복 클릭 방지
        if (settingsButton != null) settingsButton.interactable = false;
        if (startButton != null) startButton.interactable = false;

        // 1. GameEvent 발생 (선택적: 예: BGM FADE OUT 등)
        if (startGameEvent != null)
            startGameEvent.Raise();

        // 트랜지션 애니메이션 시작
        if (playTransitionEffectPanel != null && playTransitionAnimator != null)
        {
            playTransitionEffectPanel.SetActive(true); // 이펙트 패널 활성화
            // "PlayTransition"은 Animator Controller에 설정한 애니메이션 트리거/Bool/State 이름
            playTransitionAnimator.SetTrigger("StartSlash"); // 예시: Trigger 사용

            // 애니메이션 길이를 기다린 후 씬 로드
            StartCoroutine(LoadSceneAfterAnimation(nextSceneName, playTransitionAnimator.GetCurrentAnimatorStateInfo(0).length));
        }
        else
        {
            // 애니메이션이 없으면 즉시 씬 로드
            Debug.LogWarning("[StartSceneUIManager] Transition Effect Panel 또는 Animator가 없어 즉시 씬을 로드합니다.");
            SceneManager.LoadScene(nextSceneName);
        }
    }

    IEnumerator LoadSceneAfterAnimation(string sceneName, float animationLength)
    {
        yield return new WaitForSeconds(animationLength + 0.1f); // 애니메이션 시간만큼 대기
        SceneManager.LoadScene(sceneName);
    }
}