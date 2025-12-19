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

    [Header("Loading Icon - Movement")]
    [Tooltip("로딩 아이콘 RectTransform (슬라이더 위를 이동)")]
    public RectTransform loadingIcon;

    [Tooltip("로딩 아이콘 Image 컴포넌트")]
    public Image loadingIconImage;

    [Tooltip("슬라이더 위 오프셋 (Y축 거리)")]
    public float iconYOffset = 30f;

    [Tooltip("아이콘 이동 속도 (보간)")]
    [Range(1f, 20f)]
    public float iconMoveSpeed = 10f;

    [Header("Loading Icon - Sprite Animation")]
    [Tooltip("애니메이션용 스프라이트 배열 (4개)")]
    public Sprite[] iconSprites;

    [Tooltip("스프라이트 전환 속도 (초)")]
    public float spriteAnimSpeed = 0.1f;

    [Header("Loading Tips")]
    [Tooltip("팁 텍스트 UI")]
    public Text tipText;

    [Tooltip("팁 텍스트 배열")]
    public string[] loadingTips = new string[]
    {
        "팁: 마스터 볼륨으로 전체 소리를 조절할 수 있습니다.",
        "팁: 옵션에서 배경음과 효과음을 따로 조절하세요.",
        "팁: ESC 키로 옵션 창을 열고 닫을 수 있습니다.",
        "팁: 설정은 자동으로 저장됩니다."
    };

    [Header("Animation")]
    [Tooltip("로딩 텍스트 애니메이션 (점 깜빡임)")]
    public bool animateLoadingText = true;

    private Coroutine textAnimationCoroutine;
    private Coroutine spriteAnimationCoroutine;
    private bool isLoadingUIVisible = false;
    private float currentProgress = 0f;

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

        // loadingIconImage 자동 할당
        if (loadingIcon != null && loadingIconImage == null)
        {
            loadingIconImage = loadingIcon.GetComponent<Image>();

            if (loadingIconImage == null)
            {
                Debug.LogWarning("[SceneTransitionUI] loadingIcon에 Image 컴포넌트가 없습니다!");
            }
        }

        // 로딩 UI 숨김
        HideLoadingUI();

        Debug.Log("[SceneTransitionUI] 초기화 완료");
    }

    void Update()
    {
        // 로딩 아이콘 위치 업데이트 (슬라이더 진행률 따라감)
        if (isLoadingUIVisible && loadingIcon != null && loadingSlider != null)
        {
            UpdateIconPosition();
        }
    }

    /// <summary>
    /// 로딩 아이콘 위치 업데이트 (슬라이더 위를 이동)
    /// </summary>
    void UpdateIconPosition()
    {
        // 슬라이더의 Fill Area 위치 계산
        RectTransform sliderRect = loadingSlider.GetComponent<RectTransform>();

        if (sliderRect == null) return;

        // 슬라이더 진행률 (0 ~ 1)
        float progress = loadingSlider.value;

        // 슬라이더의 실제 너비
        float sliderWidth = sliderRect.rect.width;

        // 목표 X 위치 계산 (슬라이더 왼쪽 끝 기준)
        float targetX = sliderRect.rect.xMin + (sliderWidth * progress);

        // 현재 위치에서 부드럽게 이동
        Vector2 currentPos = loadingIcon.anchoredPosition;
        float newX = Mathf.Lerp(currentPos.x, targetX, Time.unscaledDeltaTime * iconMoveSpeed);

        // Y 위치는 슬라이더 위쪽으로 고정
        float newY = sliderRect.rect.yMax + iconYOffset;

        loadingIcon.anchoredPosition = new Vector2(newX, newY);
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

        currentProgress = progress;

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
        Debug.Log("[SceneTransitionUI] 로딩 UI 표시");

        // 로딩 텍스트
        if (loadingText != null)
        {
            loadingText.gameObject.SetActive(true);

            if (animateLoadingText && textAnimationCoroutine == null)
            {
                textAnimationCoroutine = StartCoroutine(AnimateLoadingText());
            }
        }

        // 슬라이더
        if (loadingSlider != null)
        {
            loadingSlider.gameObject.SetActive(true);
            loadingSlider.value = 0f;
        }

        // 퍼센트 텍스트
        if (percentText != null)
        {
            percentText.gameObject.SetActive(true);
            percentText.text = "0%";
        }

        // 로딩 아이콘
        if (loadingIcon != null)
        {
            loadingIcon.gameObject.SetActive(true);

            // 초기 위치 설정 (슬라이더 왼쪽 끝)
            if (loadingSlider != null)
            {
                RectTransform sliderRect = loadingSlider.GetComponent<RectTransform>();

                if (sliderRect != null)
                {
                    float startX = sliderRect.rect.xMin;
                    float startY = sliderRect.rect.yMax + iconYOffset;
                    loadingIcon.anchoredPosition = new Vector2(startX, startY);
                }
            }

            // 스프라이트 애니메이션 시작
            if (iconSprites != null && iconSprites.Length > 0 && spriteAnimationCoroutine == null)
            {
                spriteAnimationCoroutine = StartCoroutine(AnimateIconSprites());
            }
        }

        // 팁 텍스트
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
        currentProgress = 0f;

        if (loadingText != null)
            loadingText.gameObject.SetActive(false);

        if (loadingSlider != null)
            loadingSlider.gameObject.SetActive(false);

        if (percentText != null)
            percentText.gameObject.SetActive(false);

        // 텍스트 애니메이션 중지
        if (textAnimationCoroutine != null)
        {
            StopCoroutine(textAnimationCoroutine);
            textAnimationCoroutine = null;
        }

        // 아이콘 숨김 + 스프라이트 애니메이션 중지
        if (loadingIcon != null)
        {
            loadingIcon.gameObject.SetActive(false);
        }

        if (spriteAnimationCoroutine != null)
        {
            StopCoroutine(spriteAnimationCoroutine);
            spriteAnimationCoroutine = null;
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
                dotCount = (dotCount + 1) % 4;
            }

            yield return new WaitForSecondsRealtime(0.3f);
        }
    }

    /// <summary>
    /// 로딩 아이콘 스프라이트 애니메이션 (4개 순환)
    /// </summary>
    IEnumerator AnimateIconSprites()
    {
        if (loadingIconImage == null || iconSprites == null || iconSprites.Length == 0)
        {
            Debug.LogWarning("[SceneTransitionUI] 스프라이트 애니메이션을 위한 설정이 부족합니다!");
            yield break;
        }

        int currentIndex = 0;

        while (true)
        {
            // 현재 스프라이트 표시
            if (iconSprites[currentIndex] != null)
            {
                loadingIconImage.sprite = iconSprites[currentIndex];
            }

            // 다음 인덱스로 (순환)
            currentIndex = (currentIndex + 1) % iconSprites.Length;

            yield return new WaitForSecondsRealtime(spriteAnimSpeed);
        }
    }
}