using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 두루마리 펼침 애니메이션
/// 상단 고정, 메인 세로 확장, 하단 따라서 이동
/// </summary>
public class ScrollModalAnimator : MonoBehaviour
{
    [Header("References")]
    public RectTransform scrollTop;      // 상단 두루마리 (고정)
    public RectTransform scrollMain;     // 중앙 내용 (세로 확장)
    public RectTransform scrollBottom;   // 하단 두루마리 (아래로 이동)
    public CanvasGroup mainCanvasGroup;  // 중앙 내용 페이드용

    [Header("Content References - Image Mode")]
    [Tooltip("Image 모드용 (기존)")]
    public Image titleImage;
    public Image titleText;
    public Image messageImage;
    public Image messageText;
    public Image leftButtonImage;
    public Image rightButtonImage;
    public Image leftButtonText;
    public Image rightButtonText;

    [Header("Content References - Text Mode")]
    [Tooltip("Text 모드용 (동적 텍스트)")]
    public bool useTextMode = false;
    public Text titleTextComponent;
    public Text messageTextComponent;
    public Text leftButtonTextComponent;
    public Text rightButtonTextComponent;

    [Header("Button References")]
    public Button leftButton;
    public Button rightButton;

    [Header("Animation Settings")]
    public float animationDuration = 0.5f;
    public AnimationCurve heightCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public AnimationCurve alphaCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Content Fade Settings")]
    [Tooltip("콘텐츠 페이드 시작 시점 (0~1, 0.8 = 80% 펼쳐졌을 때)")]
    [Range(0f, 1f)]
    public float contentFadeStartTime = 0.8f;

    [Tooltip("콘텐츠 페이드 지속 시간")]
    public float contentFadeDuration = 0.3f;

    [Header("Start Position")]
    [Tooltip("하단 두루마리 시작 Y 위치 (에디터에서 자동 저장)")]
    public float bottomStartPosY = 207.5f;

    private float mainOriginalHeight;
    private Vector2 bottomOriginalPos;

    void Awake()
    {
        // 원본 값 저장
        if (scrollMain != null)
        {
            mainOriginalHeight = scrollMain.sizeDelta.y;
        }

        if (scrollBottom != null)
        {
            bottomOriginalPos = scrollBottom.anchoredPosition;
        }

        // Inspector에서 설정 안 했으면 ScrollMain Pos Y 사용
        if (bottomStartPosY == 0f && scrollMain != null)
        {
            bottomStartPosY = scrollMain.anchoredPosition.y;
            Debug.Log($"[ScrollModalAnimator] 하단 시작 위치 자동 설정: {bottomStartPosY}");
        }

        // CanvasGroup 가져오기 또는 추가
        if (mainCanvasGroup == null && scrollMain != null)
        {
            mainCanvasGroup = scrollMain.GetComponent<CanvasGroup>();
            if (mainCanvasGroup == null)
            {
                mainCanvasGroup = scrollMain.gameObject.AddComponent<CanvasGroup>();
            }
        }
    }

    /// <summary>
    /// 모달 열기 애니메이션
    /// </summary>
    public IEnumerator AnimateOpen()
    {
        // ========== 초기 상태 설정 (찌그러지지 않게!) ==========

        // 상단 두루마리: 그대로 표시 (찌그러지지 않음)
        if (scrollTop != null)
        {
            scrollTop.gameObject.SetActive(true);
            // 위치/크기 변경 없음!
        }

        // 메인 두루마리: Height 0으로 시작
        if (scrollMain != null)
        {
            Vector2 size = scrollMain.sizeDelta;
            size.y = 0f;
            scrollMain.sizeDelta = size;
            scrollMain.gameObject.SetActive(true);
        }

        // 메인 내용: 투명
        if (mainCanvasGroup != null)
        {
            mainCanvasGroup.alpha = 0f;
        }

        SetContentAlpha(0f);
        SetContentInteractable(false);

        // 하단 두루마리: 상단 바로 아래 (메인이 없으므로)
        if (scrollBottom != null)
        {
            scrollBottom.anchoredPosition = new Vector2(
                bottomOriginalPos.x,
                bottomStartPosY // ← ScrollMain Pos Y (207.5)
            );
            scrollBottom.gameObject.SetActive(true);
        }

        // ========== 애니메이션 실행 ==========

        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / animationDuration;
            float heightT = heightCurve.Evaluate(t);

            // 메인 두루마리 - 세로로 늘어남
            if (scrollMain != null)
            {
                Vector2 size = scrollMain.sizeDelta;
                size.y = mainOriginalHeight * heightT;
                scrollMain.sizeDelta = size;
            }

            // 하단 두루마리 - 메인 따라서 아래로 이동
            if (scrollBottom != null)
            {
                float currentMainHeight = mainOriginalHeight * heightT;

                scrollBottom.anchoredPosition = new Vector2(
                    bottomOriginalPos.x,
                    bottomStartPosY - currentMainHeight // ← 시작위치에서 메인 높이만큼 내려감
                );
            }

            // 메인 내용 - 페이드 인 (약간 지연)
            if (mainCanvasGroup != null && t > 0.3f)
            {
                float delayedT = (t - 0.3f) / 0.7f;
                mainCanvasGroup.alpha = alphaCurve.Evaluate(delayedT);
            }

            yield return null;
        }

        // ========== 최종 상태 보정 ==========

        if (scrollMain != null)
        {
            Vector2 size = scrollMain.sizeDelta;
            size.y = mainOriginalHeight;
            scrollMain.sizeDelta = size;
        }

        if (scrollBottom != null)
        {
            scrollBottom.anchoredPosition = bottomOriginalPos;
        }

        if (mainCanvasGroup != null)
        {
            mainCanvasGroup.alpha = 1f;
        }

        yield return StartCoroutine(FadeInContent());
    }

    /// <summary>
    /// 모달 닫기 애니메이션 (역순)
    /// </summary>
    public IEnumerator AnimateClose()
    {
        yield return StartCoroutine(FadeOutContent());
        
        float elapsed = 0f;
        float closeDuration = animationDuration * 0.7f; // 닫을 때는 약간 빠르게

        while (elapsed < closeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / closeDuration;
            float heightT = heightCurve.Evaluate(t);

            // 메인 내용 - 페이드 아웃 (먼저 사라짐)
            if (mainCanvasGroup != null)
            {
                mainCanvasGroup.alpha = 1f - t;
            }

            // 메인 두루마리 - 세로로 줄어듦
            if (scrollMain != null)
            {
                Vector2 size = scrollMain.sizeDelta;
                size.y = mainOriginalHeight * (1f - heightT);
                scrollMain.sizeDelta = size;
            }

            // 하단 두루마리 - 위로 올라감
            if (scrollBottom != null)
            {
                float currentMainHeight = mainOriginalHeight * (1f - heightT);

                scrollBottom.anchoredPosition = new Vector2(
                    bottomOriginalPos.x,
                    bottomStartPosY - currentMainHeight // ← 시작위치로 돌아감
                );
            }

            yield return null;
        }

        // ========== 최종 상태 ==========

        if (scrollMain != null)
        {
            Vector2 size = scrollMain.sizeDelta;
            size.y = 0f;
            scrollMain.sizeDelta = size;
        }

        if (scrollBottom != null)
        {
            scrollBottom.anchoredPosition = new Vector2(bottomOriginalPos.x, bottomStartPosY);
        }

        if (mainCanvasGroup != null)
        {
            mainCanvasGroup.alpha = 0f;
        }
    }
    IEnumerator FadeInContent()
    {
        float elapsed = 0f;

        while (elapsed < contentFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / contentFadeDuration;
            float alphaT = alphaCurve.Evaluate(t);

            SetContentAlpha(alphaT);

            yield return null;
        }

        SetContentAlpha(1f);
        SetContentInteractable(true); // 버튼 활성화
    }

    // ========== 콘텐츠 페이드 아웃 ==========
    IEnumerator FadeOutContent()
    {
        SetContentInteractable(false); // 버튼 비활성화

        float elapsed = 0f;
        float fadeDuration = contentFadeDuration * 0.7f; // 빠르게

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / fadeDuration;
            float alphaT = alphaCurve.Evaluate(t);

            SetContentAlpha(1f - alphaT);

            yield return null;
        }

        SetContentAlpha(0f);
    }

    // ========== 콘텐츠 알파 설정 ==========
    void SetContentAlpha(float alpha)
    {
        // Text 모드
        if (useTextMode)
        {
            if (titleTextComponent != null)
            {
                Color c = titleTextComponent.color;
                c.a = alpha;
                titleTextComponent.color = c;
            }

            if (messageTextComponent != null)
            {
                Color c = messageTextComponent.color;
                c.a = alpha;
                messageTextComponent.color = c;
            }

            if (leftButtonTextComponent != null)
            {
                Color c = leftButtonTextComponent.color;
                c.a = alpha;
                leftButtonTextComponent.color = c;
            }

            if (rightButtonTextComponent != null)
            {
                Color c = rightButtonTextComponent.color;
                c.a = alpha;
                rightButtonTextComponent.color = c;
            }
        }
        // Image 모드 (기존)
        else
        {
            // 제목 이미지
            if (titleImage != null)
            {
                Color c = titleImage.color;
                c.a = alpha;
                titleImage.color = c;
            }
            // 제목 텍스트
            if (titleText != null)
            {
                Color c = titleText.color;
                c.a = alpha;
                titleText.color = c;
            }

            // 메시지 이미지
            if (messageImage != null)
            {
                Color c = messageImage.color;
                c.a = alpha;
                messageImage.color = c;
            }

            // 메시지 텍스트
            if (messageText != null)
            {
                Color c = messageText.color;
                c.a = alpha;
                messageText.color = c;
            }

            // 왼쪽 버튼 이미지 (버튼 자체는 투명)
            if (leftButtonImage != null)
            {
                Color c = leftButtonImage.color;
                c.a = alpha;
                leftButtonImage.color = c;
            }

            // 왼쪽 버튼 텍스트
            if (leftButtonText != null)
            {
                Color c = leftButtonText.color;
                c.a = alpha;
                leftButtonText.color = c;
            }

            // 오른쪽 버튼 이미지
            if (rightButtonImage != null)
            {
                Color c = rightButtonImage.color;
                c.a = alpha;
                rightButtonImage.color = c;
            }

            // 오른쪽 버튼 텍스트
            if (rightButtonText != null)
            {
                Color c = rightButtonText.color;
                c.a = alpha;
                rightButtonText.color = c;
            }
        }
    }

    // ========== 버튼 활성화/비활성화 ==========
    void SetContentInteractable(bool interactable)
    {
        if (leftButton != null)
        {
            leftButton.interactable = interactable;
        }

        if (rightButton != null)
        {
            rightButton.interactable = interactable;
        }
    }
}
