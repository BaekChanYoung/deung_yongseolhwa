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

    [Header("Loading Icon")]
    [Tooltip("로딩 아이콘 (회전 애니메이션)")]
    public RectTransform loadingIcon;

    [Tooltip("회전 속도")]
    public float rotationSpeed = 180f;

    [Header("Loading Tips")]
    [Tooltip("팁 텍스트 UI")]
    public Text tipText;

    [Tooltip("팁 텍스트 배열")]
    public string[] loadingTips = new string[]
    {
        "Tip: 마스터 볼륨으로 전체 소리를 조절할 수 있습니다.",
        "Tip: 옵션에서 배경음과 효과음을 따로 조절하세요.",
        "Tip: ESC 키로 옵션 창을 열고 닫을 수 있습니다.",
        "Tip: 설정은 자동으로 저장됩니다."
    };

    [Header("Animation")]
    [Tooltip("로딩 텍스트 애니메이션 (점 깜빡임)")]
    public bool animateLoadingText = true;

    private Coroutine textAnimationCoroutine;
    private bool isLoadingIconRotating = false;
    private bool isLoadingUIVisible = false;

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

    void Update()
    {
        if (isLoadingIconRotating && loadingIcon != null)
        {
            loadingIcon.Rotate(0f, 0f, -rotationSpeed * Time.unscaledDeltaTime);
        }
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
        if (!isLoadingUIVisible)
        {
            ShowLoadingUI();
            isLoadingUIVisible = true;
        }

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

        if (loadingIcon != null)
        {
            loadingIcon.gameObject.SetActive(true);
            isLoadingIconRotating = true;
        }

        if (tipText != null && loadingTips.Length > 0)
        {
            int randomIndex = Random.Range(0, loadingTips.Length);
            tipText.text = loadingTips[randomIndex];
            tipText.gameObject.SetActive(true);

            Debug.Log($"[SceneTransitionUI] 팁 표시: {loadingTips[randomIndex]}");
        }
    }

    /// <summary>
    /// 로딩 UI 숨김
    /// </summary>
    void HideLoadingUI()
    {
        Debug.Log("[SceneTransitionUI] 로딩 UI 숨김");

        isLoadingUIVisible = false;

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

        if (loadingIcon != null)
        {
            loadingIcon.gameObject.SetActive(false);
            isLoadingIconRotating = false;
        }

        if (tipText != null)
        {
            tipText.gameObject.SetActive(false);
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