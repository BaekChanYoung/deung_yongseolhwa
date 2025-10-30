using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 씬 전환 시 페이드 효과를 담당하는 UI
/// Canvas에 배치되어 화면 전체를 덮음
/// </summary>
public class SceneTransitionUI : MonoBehaviour
{
    [Header("Fade")]
    [Tooltip("페이드용 검은 이미지")]
    public Image fadeImage;

    [Tooltip("페이드 색상")]
    public Color fadeColor = Color.black;

    [Header("Loading UI")]
    [Tooltip("로딩 텍스트 (Loading...)")]
    public Text loadingText;

    [Tooltip("로딩 진행률 슬라이더")]
    public Slider loadingSlider;

    [Tooltip("로딩 퍼센트 텍스트 (0%)")]
    public Text percentText;

    [Header("Animation")]
    [Tooltip("로딩 텍스트 애니메이션 (점 깜빡임)")]
    public bool animateLoadingText = true;

    private Coroutine textAnimationCoroutine;

    void Awake()
    {
        // 중복 방지
        var existing = FindObjectsOfType<SceneTransitionUI>();
        if (existing.Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        // 초기 상태: 완전 투명
        if (fadeImage != null)
        {
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
            fadeImage.raycastTarget = false; // 클릭 차단 안 함
        }

        // 로딩 UI 숨김
        HideLoadingUI();

        Debug.Log("[SceneTransitionUI] 초기화 완료");
    }

    /// <summary>
    /// Fade Out (화면이 어두워짐)
    /// </summary>
    public IEnumerator FadeOut(float duration)
    {
        if (fadeImage == null) yield break;

        Debug.Log($"[SceneTransitionUI] Fade Out 시작 ({duration}초)");

        fadeImage.raycastTarget = true; // 페이드 중 클릭 차단

        float elapsed = 0f;
        Color startColor = fadeImage.color;
        Color targetColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 1f);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime; // Time.timeScale 영향 안 받음
            float t = elapsed / duration;

            fadeImage.color = Color.Lerp(startColor, targetColor, t);

            yield return null;
        }

        fadeImage.color = targetColor;
    }

    /// <summary>
    /// Fade In (화면이 밝아짐)
    /// </summary>
    public IEnumerator FadeIn(float duration)
    {
        if (fadeImage == null) yield break;

        Debug.Log($"[SceneTransitionUI] Fade In 시작 ({duration}초)");

        float elapsed = 0f;
        Color startColor = fadeImage.color;
        Color targetColor = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;

            fadeImage.color = Color.Lerp(startColor, targetColor, t);

            yield return null;
        }

        fadeImage.color = targetColor;
        fadeImage.raycastTarget = false; // 페이드 완료 후 클릭 허용

        // 로딩 UI 숨김
        HideLoadingUI();
    }

    /// <summary>
    /// 로딩 진행률 설정 (0.0 ~ 1.0)
    /// </summary>
    public void SetLoadingProgress(float progress)
    {
        ShowLoadingUI();

        if (loadingSlider != null)
        {
            loadingSlider.value = progress;
        }

        if (percentText != null)
        {
            percentText.text = $"{progress * 100:F0}%";
        }
    }

    /// <summary>
    /// 로딩 UI 표시
    /// </summary>
    void ShowLoadingUI()
    {
        if (loadingText != null)
        {
            loadingText.gameObject.SetActive(true);

            // 애니메이션 시작
            if (animateLoadingText && textAnimationCoroutine == null)
            {
                textAnimationCoroutine = StartCoroutine(AnimateLoadingText());
            }
        }

        if (loadingSlider != null)
            loadingSlider.gameObject.SetActive(true);

        if (percentText != null)
            percentText.gameObject.SetActive(true);
    }

    /// <summary>
    /// 로딩 UI 숨김
    /// </summary>
    void HideLoadingUI()
    {
        if (loadingText != null)
            loadingText.gameObject.SetActive(false);

        if (loadingSlider != null)
            loadingSlider.gameObject.SetActive(false);
        
        if (percentText != null)
            percentText.gameObject.SetActive(false);

        // 애니메이션 중지
        if (textAnimationCoroutine != null)
        {
            StopCoroutine(textAnimationCoroutine);
            textAnimationCoroutine = null;
        }
    }

    /// <summary>
    /// 로딩 텍스트 애니메이션 (Loading...)
    /// </summary>
    IEnumerator AnimateLoadingText()
    {
        string baseText = "Loading";
        int dotCount = 0;

        while (true)
        {
            if (loadingText != null)
            {
                loadingText.text = baseText + new string('.', dotCount);

                dotCount = (dotCount + 1) % 4; // 0, 1, 2, 3 반복
            }

            yield return new WaitForSecondsRealtime(0.3f);
        }
    }
}