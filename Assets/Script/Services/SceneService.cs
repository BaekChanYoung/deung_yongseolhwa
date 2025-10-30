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
    private string targetSceneName;
    private float savedMusicVolume = 0.75f;

    public bool IsLoading => isLoading;

    void Awake()
    {
        if (ServiceLocator.Resolve<ISceneService>() != null)
        {
            Destroy(gameObject);
            return;
        }

        ServiceLocator.Register<ISceneService>(this);
        Debug.Log("[SceneService] ServiceLocator에 등록되었습니다.");
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

        Debug.Log("========================================");
        Debug.Log($"[SceneService] 씬 전환 요청!");
        Debug.Log($"  현재 씬: {GetCurrentSceneName()}");
        Debug.Log($"  목표 씬: {targetScene}");
        Debug.Log("========================================");

        targetSceneName = targetScene;
        StartCoroutine(LoadSceneWithLoadingCoroutine(onComplete));
    }

    IEnumerator LoadSceneWithLoadingCoroutine(Action onComplete)
    {
        isLoading = true;

        string startSceneName = GetCurrentSceneName();
        Debug.Log($"[SceneService] 씬 전환 시작: {startSceneName} → {targetSceneName}");

        // ========== 1단계: 현재 씬에서 Fade Out + BGM 페이드 아웃 ==========

        // 현재 BGM 볼륨 저장
        savedMusicVolume = PlayerPrefs.GetFloat("MusicVol", 0.75f);
        Debug.Log($"[SceneService] BGM 볼륨 저장: {savedMusicVolume:F2}");

        PlayTransitionSfx();

        // BGM 볼륨을 0으로 페이드 아웃
        if (audioService != null)
        {
            Debug.Log("[SceneService] BGM 페이드 아웃 시작 (0.0)");
            yield return StartCoroutine(FadeMusicOut(fadeDuration));
        }

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

        float startTime = Time.time;

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

            float elapsedTime = Time.time - startTime;
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

        // ========== BGM 볼륨 복구 (최종 씬에서만!) ==========
        if (audioService != null)
        {
            Debug.Log($"[SceneService] 최종 씬({targetSceneName}) - BGM 볼륨 복구 시작: 0.0 → {savedMusicVolume:F2}");
            yield return StartCoroutine(RestoreMusicVolumeCoroutine(savedMusicVolume, fadeDuration));
        }
        // ================================================

        // ========== 완료 ==========

        isLoading = false;

        if (onComplete != null)
            onComplete.Invoke();

        Debug.Log($"[SceneService] ========================================");
        Debug.Log($"[SceneService] 씬 전환 완전 완료: {targetSceneName}");
        Debug.Log($"[SceneService] ========================================");
    }

    IEnumerator FadeMusicOut(float duration)
    {
        if (audioService == null) yield break;

        Debug.Log($"[SceneService] FadeMusicOut 시작: {savedMusicVolume:F2} → 0.0");

        float elapsed = 0f;
        float startVolume = savedMusicVolume;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            float currentVolume = Mathf.Lerp(startVolume, 0f, t);

            // Internal 메서드 사용 (PlayerPrefs 저장 안 함)
            audioService.SetMusicVolumeInternal(currentVolume);

            yield return null;
        }

        // 최종적으로 0으로 설정
        audioService.SetMusicVolumeInternal(0f);
        Debug.Log($"[SceneService] FadeMusicOut 완료: 0.0");
    }

    /// <summary>
    /// BGM 볼륨을 부드럽게 복구
    /// </summary>
    IEnumerator RestoreMusicVolumeCoroutine(float targetVolume, float duration)
    {
        if (audioService == null) yield break;

        Debug.Log($"[SceneService] BGM 볼륨 복구 코루틴 시작: 0.0 → {targetVolume:F2}");

        float elapsed = 0f;
        float startVolume = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            float currentVolume = Mathf.Lerp(startVolume, targetVolume, t);

            // Internal 메서드 사용 (PlayerPrefs 저장 안 함)
            audioService.SetMusicVolumeInternal(currentVolume);

            yield return null;
        }

        // 최종 볼륨 설정 (PlayerPrefs에 저장)
        audioService.SetMusicVolume(targetVolume);
        Debug.Log($"[SceneService] BGM 볼륨 복구 완료: {targetVolume:F2}");
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
        if ((UnityEngine.Object)ServiceLocator.Resolve<ISceneService>() == this)
        {
            ServiceLocator.Unregister<ISceneService>();
        }
    }
}