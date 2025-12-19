using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 씬 전환을 담당하는 서비스
/// AudioManager와 동일한 Self-registration 패턴 사용
/// </summary>
public class SceneService : MonoBehaviour, ISceneService
{
    [Header("References")]
    [Tooltip("페이드 효과 UI")]
    public SceneTransitionUI transitionUI;

    [Header("Settings")]
    [Tooltip("페이드 지속 시간")]
    public float fadeDuration = 0.5f;

    [Tooltip("최소 로딩 시간 (너무 빠른 전환 방지)")]
    public float minLoadingTime = 1.5f;

    [Header("Audio")]
    [Tooltip("씬 전환 효과음")]
    public AudioClip transitionSfx;

    private bool isLoading = false;
    private IAudioService audioService;
    //private string targetSceneName;
    //private float savedMusicVolume = 0.5f;

    public bool IsLoading => isLoading;

    void Awake()
    {
        // 등록 전 확인
        if (ServiceLocator.IsRegistered<ISceneService>())
        {
            Debug.Log("SceneService already exists. Destroying duplicate.");
            Destroy(gameObject);
            return;
        }

        // 안전한 등록
        if (!ServiceLocator.Register<ISceneService>(this))
        {
            Debug.LogError("Failed to register ISceneService!");
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        audioService = ServiceLocator.Resolve<IAudioService>();

        if (transitionUI == null)
        {
            transitionUI = FindObjectOfType<SceneTransitionUI>();
        }

        if (transitionUI == null)
        {
            Debug.LogError("[SceneService] SceneTransitionUI를 찾을 수 없습니다!");
        }
    }

    public void LoadSceneWithLoading(string targetScene, Action onComplete = null)
    {
        if (isLoading)
        {
            Debug.LogWarning($"[SceneService] 이미 로딩 중입니다!");
            return;
        }

        if (Time.timeScale != 1f)
        {
            Debug.LogWarning($"[SceneService] Time.timeScale이 {Time.timeScale}였습니다. 1로 복구합니다.");
            Time.timeScale = 1f;
        }

        Debug.Log("========================================");
        Debug.Log($"[SceneService] 씬 전환 요청!");
        Debug.Log($"  현재 씬: {GetCurrentSceneName()}");
        Debug.Log($"  목표 씬: {targetScene}");
        Debug.Log("========================================");

        //targetSceneName = targetScene;
        StartCoroutine(LoadSceneWithLoadingCoroutine(targetScene, onComplete));
    }

    IEnumerator LoadSceneWithLoadingCoroutine(string targetSceneName, Action onComplete)
    {
        isLoading = true;

        string startSceneName = GetCurrentSceneName();
        Debug.Log($"[SceneService] 씬 전환 시작: {startSceneName} → {targetSceneName}");

        // ========== 1단계: 현재 씬에서 Fade Out + BGM 페이드 아웃 ==========

        PlayTransitionSfx();

        // 화면 페이드 아웃
        if (transitionUI != null)
        {
            yield return StartCoroutine(transitionUI.FadeOut(fadeDuration));
        }
        else
        {
            yield return new WaitForSeconds(fadeDuration);
        }

        Debug.Log("[SceneService] Fade Out 완료");

        // ========== 2단계: LoadingScene 로드 ==========

        SceneManager.LoadScene(SceneNames.LOADING);
        yield return null; // 씬 로드 대기

        Debug.Log("[SceneService] LoadingScene 진입");
        Debug.Log("[SceneService] ⚠️ LoadingScene에서는 BGM 0 유지 (복구 안 함!)");

        // ========== 3단계: 목표 씬 비동기 로드 ==========

        float startTime = Time.realtimeSinceStartup;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetSceneName);
        asyncLoad.allowSceneActivation = false;

        // 로딩 진행
        while (!asyncLoad.isDone)
        {
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

            if (transitionUI != null)
            {
                transitionUI.SetLoadingProgress(progress);
            }

            float elapsedTime = Time.realtimeSinceStartup - startTime;
            bool minTimeReached = elapsedTime >= minLoadingTime;

            if (asyncLoad.progress >= 0.9f && minTimeReached)
            {
                Debug.Log($"[SceneService] 로딩 완료! (진행률: {progress * 100:F0}%, 경과: {elapsedTime:F1}초)");
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }

        yield return null; // 씬 전환 완료 대기

        Debug.Log($"[SceneService] {targetSceneName} 로드 완료");

        // ========== 4단계: 최종 씬에서 Fade In + BGM 복구 ==========

        Debug.Log($"[SceneService] 최종 씬({targetSceneName}) 진입 - Fade In 시작");

        // Fade In
        if (transitionUI != null)
        {
            yield return StartCoroutine(transitionUI.FadeIn(fadeDuration));
        }
        else
        {
            yield return new WaitForSeconds(fadeDuration);
        }

        Debug.Log("[SceneService] Fade In 완료");

        isLoading = false;

        if (onComplete != null)
            onComplete.Invoke();

        Debug.Log($"[SceneService] ========================================");
        Debug.Log($"[SceneService] 씬 전환 완전 완료: {targetSceneName}");
        Debug.Log($"[SceneService] ========================================");
    }

    public string GetCurrentSceneName()
    {
        return SceneManager.GetActiveScene().name;
    }

    void PlayTransitionSfx()
    {
        if (audioService != null && transitionSfx != null)
        {
            audioService.PlaySfx(transitionSfx, 0.7f);
        }
    }

    void OnDestroy()
    {
        ServiceLocator.Unregister<ISceneService>();
    }
}