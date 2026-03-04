using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// 일러스트 선택 모달 컨트롤러
/// IllustrationBtn 클릭 시 표시되는 일러스트 갤러리 UI
/// </summary>
public class IllustrationModal : MonoBehaviour
{
    [Header("UI References")]
    public GameObject modalPanel;
    public ScrollModalAnimator scrollAnimator;
    public Button closeButton;

    [Header("Currency Display")]
    [Tooltip("재화 표시 Text")]
    public Text currencyText;

    [Header("Illustration Grid")]
    [Tooltip("일러스트 아이템들이 들어갈 Grid Layout")]
    public Transform illustrationGridContainer;

    [Tooltip("일러스트 아이템 프리팹")]
    public GameObject illustrationItemPrefab;

    [Header("Full View")]
    [Tooltip("일러스트 전체보기 Panel")]
    public GameObject fullViewPanel;

    [Tooltip("전체보기 이미지")]
    public Image fullViewImage;

    [Tooltip("전체보기 닫기 버튼")]
    public Button fullViewCloseButton;

    [Tooltip("이미지 그림 설명 Panel")]
    public GameObject fullImageDescriptionPanel;

    [Tooltip("이미지 그림 설명 TextMesh")]
    public TextMeshProUGUI fullImageDescription;

    [Tooltip("설명보기 닫기 버튼")]
    public Button descriptionCloseButton;

    [Tooltip("설명보기 열기 버튼")]
    public Button descriptionOpenButton;

    [Header("Universal Modal")]
    [Tooltip("해금 확인 모달 (Inspector에서 연결!)")]
    public UniversalModalController universalModal;

    [Header("Audio")]
    public AudioClip openSfx;
    public AudioClip closeSfx;
    public AudioClip selectSfx;
    public AudioClip unlockSfx;

    private CanvasGroup canvasGroup;
    private IAudioService audioService;
    private bool isOpen = false;

    private bool descriptionIsOpen = true;

    // 일러스트 아이템 리스트
    private List<IllustrationItemUI> illustrationItemUIList = new List<IllustrationItemUI>();

    public bool IsOpen => isOpen;

    void Awake()
    {
        audioService = ServiceLocator.Resolve<IAudioService>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // UniversalModal이 없을 경우 자동으로 검색 (Fallback)
        if (universalModal == null)
        {
            universalModal = FindObjectOfType<UniversalModalController>();

            if (universalModal == null)
            {
                Debug.LogWarning("[IllustrationModal] UniversalModalController를 찾을 수 없습니다! Inspector에서 연결해주세요.");
            }
        }

        // 닫기 버튼
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }

        // 전체보기 닫기 버튼
        if (fullViewCloseButton != null)
        {
            fullViewCloseButton.onClick.AddListener(CloseFullView);
        }

        // 전체보기 패널 초기화
        if (fullViewPanel != null)
        {
            fullViewPanel.SetActive(false);
        }

        // 설명 닫기 버튼 초기화
        if(descriptionCloseButton != null)
        {
            descriptionCloseButton.onClick.AddListener(CloseDescriptionView);
        }

        // 설명 보기 버튼 초기화
        if(descriptionOpenButton != null)
        {
            descriptionOpenButton.onClick.AddListener(ShowDescriptionView);
            descriptionOpenButton.gameObject.SetActive(false);
        }
        // 초기 상태: 숨김
        Hide(true);
    }

    /// <summary>
    /// 모달 표시
    /// </summary>
    public void Show()
    {
        if (isOpen)
        {
            Debug.LogWarning("[IllustrationModal] 이미 열려있습니다!");
            return;
        }

        // 효과음
        if (audioService != null && openSfx != null)
        {
            audioService.PlaySfx(openSfx, 0.5f);
        }

        // 일러스트 목록 로드
        LoadIllustrationList();

        // 재화 표시 업데이트
        UpdateCurrencyDisplay();

        // 표시
        gameObject.SetActive(true);
        isOpen = true;

        StartCoroutine(ShowAnimation());
    }

    /// <summary>
    /// 모달 닫기
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
            StartCoroutine(HideAnimation());
        }
    }

    /// <summary>
    /// 표시 애니메이션
    /// </summary>
    IEnumerator ShowAnimation()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;

        // 배경 페이드 인
        float elapsed = 0f;
        float duration = 0.2f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            canvasGroup.alpha = t * 0.5f;
            yield return null;
        }

        // 스크롤 애니메이션
        yield return StartCoroutine(scrollAnimator.AnimateOpen());

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
    }

    /// <summary>
    /// 숨김 애니메이션
    /// </summary>
    IEnumerator HideAnimation()
    {
        canvasGroup.interactable = false;

        // 효과음
        if (audioService != null && closeSfx != null)
        {
            audioService.PlaySfx(closeSfx, 0.5f);
        }

        // 스크롤 닫기
        yield return StartCoroutine(scrollAnimator.AnimateClose());

        // 배경 페이드 아웃
        float elapsed = 0f;
        float duration = 0.2f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            canvasGroup.alpha = 1f - t;
            yield return null;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
        isOpen = false;
    }

    /// <summary>
    /// 재화 표시 업데이트
    /// </summary>
    void UpdateCurrencyDisplay()
    {
        if (currencyText == null) return;

        int currency = IllustrationManager.Instance.GetCurrentCurrency();
        currencyText.text = $"보유 재화: {currency}";
    }

    /// <summary>
    /// 일러스트 목록 로드
    /// </summary>
    void LoadIllustrationList()
    {
        // 기존 아이템 삭제
        foreach (var item in illustrationItemUIList)
        {
            if (item != null)
            {
                Destroy(item.gameObject);
            }
        }
        illustrationItemUIList.Clear();

        // IllustrationManager에서 일러스트 목록 가져오기
        IllustrationManager illustrationManager = IllustrationManager.Instance;
        if (illustrationManager == null)
        {
            Debug.LogError("[IllustrationModal] IllustrationManager를 찾을 수 없습니다!");
            return;
        }

        List<IllustrationData> allIllustrations = illustrationManager.GetAllIllustrations();

        // 일러스트 아이템 생성
        foreach (IllustrationData illustrationData in allIllustrations)
        {
            GameObject itemObj = Instantiate(illustrationItemPrefab, illustrationGridContainer);
            IllustrationItemUI itemUI = itemObj.GetComponent<IllustrationItemUI>();

            if (itemUI != null)
            {
                itemUI.Setup(illustrationData, this);
                illustrationItemUIList.Add(itemUI);
            }
        }
    }

    /// <summary>
    /// 일러스트 선택 (전체보기)
    /// </summary>
    public void OnIllustrationSelected(IllustrationData illustrationData)
    {
        // 효과음
        if (audioService != null && selectSfx != null)
        {
            audioService.PlaySfx(selectSfx, 0.7f);
        }

        // 전체보기 표시
        ShowFullView(illustrationData);

        ShowDescriptionView();

        DescriptionUpdate(illustrationData);
    }

    /// <summary>
    /// 잠금 해제 확인 모달 표시 (IllustrationItemUI에서 호출)
    /// </summary>
    public void ShowUnlockConfirmationModal(IllustrationData illustrationData)
    {
        if (universalModal == null)
        {
            Debug.LogError("[IllustrationModal] UniversalModal이 할당되지 않았습니다!");
            return;
        }

        string message = $"{illustrationData.illustrationName}을(를) 해금하시겠습니까?\n비용: {illustrationData.unlockCost}";

        universalModal.Show(
            title: "일러스트 해금",
            message: message,
            leftText: "해금",
            rightText: "취소",
            onLeft: () => {
                TryUnlockIllustration(illustrationData);
            },
            onRight: null
        );
    }

    /// <summary>
    /// 일러스트 해금 시도 (UniversalModal 콜백에서 호출)
    /// </summary>
    public void TryUnlockIllustration(IllustrationData illustrationData)
    {
        IllustrationManager manager = IllustrationManager.Instance;

        // 재화 확인
        if (manager.CanUnlock(illustrationData))
        {
            // 해금 실행
            manager.UnlockIllustration(illustrationData.illustrationId);

            // 효과음
            if (audioService != null && unlockSfx != null)
            {
                audioService.PlaySfx(unlockSfx, 1.0f);
            }

            // UI 갱신
            LoadIllustrationList();
            UpdateCurrencyDisplay();

            Debug.Log($"[IllustrationModal] 일러스트 해금 완료: {illustrationData.illustrationName}");
        }
        else
        {
            Debug.Log($"[IllustrationModal] 재화 부족!");
            // TODO: 재화 부족 알림 연출
        }
    }

    /// <summary>
    /// 전체보기 표시
    /// </summary>
    void ShowFullView(IllustrationData illustrationData)
    {
        if (fullViewPanel == null || fullViewImage == null) return;

        fullViewImage.sprite = illustrationData.illustrationSprite;
        fullViewPanel.SetActive(true);

        Debug.Log($"[IllustrationModal] 전체보기: {illustrationData.illustrationName}");
    }

    /// <summary>
    /// 설명 업데이트
    /// </summary>
    void DescriptionUpdate(IllustrationData illustrationData)
    {
        fullImageDescription.text = illustrationData.description;
    }

    /// <summary>
    /// 설명 보기
    /// </summary>
    void ShowDescriptionView()
    {
        if(descriptionIsOpen == false)
        {
            descriptionIsOpen = true;
            fullImageDescriptionPanel.SetActive(true);
            descriptionOpenButton.gameObject.SetActive(false);
            descriptionCloseButton.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 설명 닫기
    /// </summary>
    void CloseDescriptionView()
    {
        if(descriptionIsOpen == true)
        {
            descriptionIsOpen = false;
            fullImageDescriptionPanel.SetActive(false);
            descriptionOpenButton.gameObject.SetActive(true);
            descriptionCloseButton.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 전체보기 닫기
    /// </summary>
    void CloseFullView()
    {
        if (fullViewPanel != null)
        {
            fullViewPanel.SetActive(false);
        }
    }

    /// <summary>
    /// 닫기 버튼 클릭
    /// </summary>
    void OnCloseButtonClicked()
    {
        Hide();
    }

    void OnDestroy()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(OnCloseButtonClicked);
        }

        if (fullViewCloseButton != null)
        {
            fullViewCloseButton.onClick.RemoveListener(CloseFullView);
        }
    }
}