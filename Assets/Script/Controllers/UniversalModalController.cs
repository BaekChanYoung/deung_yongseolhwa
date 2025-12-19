using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

/// <summary>
/// 범용 모달 컨트롤러 (모든 씬에서 사용 가능)
/// </summary>
public class UniversalModalController : MonoBehaviour
{
    [Header("UI References - Text Mode")]
    [Tooltip("Text 모드를 사용할 경우 (동적 텍스트)")]
    public bool useTextMode = false;

    public Text titleTextComponent;
    public Text messageTextComponent;
    public Text leftButtonTextComponent;
    public Text rightButtonTextComponent;

    [Header("UI References - Image Mode (기존)")]
    [Tooltip("Image 모드를 사용할 경우 (미리 만든 이미지)")]
    public GameObject modalPanel;
    public Image titleImage;
    public Image messageImage;
    public Button leftButton;
    public Button rightButton;
    public Image leftButtonImage;
    public Image rightButtonImage;

    [Header("Animation")]
    public bool useScrollAnimation = true;
    public ScrollModalAnimator scrollAnimator;
    public float fadeInDuration = 0.3f;
    public float fadeOutDuration = 0.2f;
    public AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Audio")]
    public AudioClip openSfx;
    public AudioClip buttonSfx;

    private CanvasGroup canvasGroup;
    private IAudioService audioService;
    private UnityAction leftButtonCallback;
    private UnityAction rightButtonCallback;

    private bool isOpen = false;
    public bool IsOpen => isOpen;

    void Awake()
    {
        audioService = ServiceLocator.Resolve<IAudioService>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // 버튼 이벤트
        if (leftButton != null)
            leftButton.onClick.AddListener(OnLeftButtonClicked);

        if (rightButton != null)
            rightButton.onClick.AddListener(OnRightButtonClicked);

        // 초기 상태: 숨김
        Hide(true);
    }

    /// <summary>
    /// 모달 표시 (범용)
    /// </summary>
    /// <param name="title">제목 (예: "경고!")</param>
    /// <param name="message">메시지 (예: "게임을 종료 하시겠습니까?")</param>
    /// <param name="leftText">왼쪽 버튼 텍스트 (예: "예")</param>
    /// <param name="rightText">오른쪽 버튼 텍스트 (예: "아니요")</param>
    /// <param name="onLeft">왼쪽 버튼 콜백</param>
    /// <param name="onRight">오른쪽 버튼 콜백</param>
    public void Show(
        string title,
        string message,
        string leftText = "예",
        string rightText = "아니요",
        UnityAction onLeft = null,
        UnityAction onRight = null)
    {
        if (isOpen)
        {
            Debug.LogWarning("[UniversalModal] 이미 열려있습니다!");
            return;
        }

        // 콜백 저장
        leftButtonCallback = onLeft;
        rightButtonCallback = onRight;

        // Text 모드: 동적 텍스트 설정
        if (useTextMode)
        {
            SetTexts(title, message, leftText, rightText);
        }

        // 효과음
        if (audioService != null && openSfx != null)
        {
            audioService.PlaySfx(openSfx, 0.5f);
        }

        // 표시
        gameObject.SetActive(true);
        isOpen = true;

        if (useScrollAnimation && scrollAnimator != null)
        {
            StartCoroutine(FadeInWithScroll());
        }
        else
        {
            StartCoroutine(FadeIn());
        }
    }

    /// <summary>
    /// Text 컴포넌트에 텍스트 설정
    /// </summary>
    void SetTexts(string title, string message, string leftText, string rightText)
    {
        if (titleTextComponent != null)
        {
            titleTextComponent.text = title;
        }

        if (messageTextComponent != null)
        {
            messageTextComponent.text = message;
        }

        if (leftButtonTextComponent != null)
        {
            leftButtonTextComponent.text = leftText;
        }

        if (rightButtonTextComponent != null)
        {
            rightButtonTextComponent.text = rightText;
        }
    }

    /// <summary>
    /// 모달 숨김
    /// </summary>
    public void Hide(bool immediate = false)
    {
        if (immediate)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            gameObject.SetActive(false);
            isOpen = false;
        }
        else
        {
            if (useScrollAnimation && scrollAnimator != null)
            {
                StartCoroutine(FadeOutWithScroll());
            }
            else
            {
                StartCoroutine(FadeOut());
            }
        }
    }

    IEnumerator FadeIn()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;

        float elapsed = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = fadeCurve.Evaluate(elapsed / fadeInDuration);
            canvasGroup.alpha = t;
            yield return null;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
    }

    IEnumerator FadeInWithScroll()
    {
        // 배경만 먼저 페이드 인
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;

        float elapsed = 0f;

        while (elapsed < fadeInDuration * 0.5f)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / (fadeInDuration * 0.5f);
            canvasGroup.alpha = t * 0.5f; // 배경만 50%까지
            yield return null;
        }

        // 두루마리 펼치기 애니메이션
        yield return StartCoroutine(scrollAnimator.AnimateOpen());

        // 배경 완전히 보이기
        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
    }

    IEnumerator FadeOut()
    {
        canvasGroup.interactable = false;

        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = fadeCurve.Evaluate(elapsed / fadeOutDuration);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
        isOpen = false;
    }

    IEnumerator FadeOutWithScroll()
    {
        canvasGroup.interactable = false;

        // 두루마리 접기 애니메이션
        yield return StartCoroutine(scrollAnimator.AnimateClose());

        // 배경 페이드 아웃
        float elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / fadeOutDuration;
            canvasGroup.alpha = 1f - t;
            yield return null;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
        isOpen = false;
    }

    void OnLeftButtonClicked()
    {
        if (audioService != null && buttonSfx != null)
        {
            audioService.PlaySfx(buttonSfx, 0.7f);
        }

        leftButtonCallback?.Invoke();
        Hide();
    }

    void OnRightButtonClicked()
    {
        if (audioService != null && buttonSfx != null)
        {
            audioService.PlaySfx(buttonSfx, 0.5f);
        }

        rightButtonCallback?.Invoke();
        Hide();
    }

    void OnDestroy()
    {
        if (leftButton != null)
            leftButton.onClick.RemoveListener(OnLeftButtonClicked);

        if (rightButton != null)
            rightButton.onClick.RemoveListener(OnRightButtonClicked);
    }
}