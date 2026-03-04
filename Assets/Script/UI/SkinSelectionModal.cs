using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Spine;
using Spine.Unity;
using TMPro;

/// <summary>
/// 스킨 선택 모달 컨트롤러
/// SkinBtn 클릭 시 표시되는 스킨 선택 UI
/// </summary>
public class SkinSelectionModal : MonoBehaviour
{
    [Header("UI References")]
    public GameObject modalPanel;
    public ScrollModalAnimator scrollAnimator;
    public Button closeButton;

    [Header("Currency Display")]
    [Tooltip("재화 표시 Text")]
    public Text currencyText;

    [Header("Skin Grid")]
    [Tooltip("스킨 아이템들이 들어갈 Grid Layout")]
    public Transform skinGridContainer;

    [Tooltip("스킨 아이템 프리팹")]
    public GameObject skinItemPrefab;

    [Header("Preview")]
    [Tooltip("미리보기 Spine 캐릭터")]
    public TextMeshProUGUI previewSpineCharacterName;
    public SkinController previewSpineCharacter;

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

    // 현재 선택된 스킨
    private SkinData currentSelectedSkin;

    // 스킨 아이템 리스트
    private List<SkinItemUI> skinItemUIList = new List<SkinItemUI>();

    // [추가] 현재 선택된/미리보기 중인 스킨의 리스트 내 인덱스
    private int currentIndex = 0;

    public bool IsOpen => isOpen;

    void Awake()
    {
        audioService = ServiceLocator.Resolve<IAudioService>();

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // UniversalModal이 연결 안 됐으면 자동 검색 (Fallback)
        if (universalModal == null)
        {
            universalModal = FindObjectOfType<UniversalModalController>();

            if (universalModal == null)
            {
                Debug.LogWarning("[SkinSelectionModal] UniversalModalController를 찾을 수 없습니다! Inspector에서 연결해주세요.");
            }
        }

        // 닫기 버튼
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(OnCloseButtonClicked);
        }

        // 초기 상태: 숨김
        Hide(true);
    }

    /// <summary>
    /// 모달 열기
    /// </summary>
    public void Show()
    {
        if (isOpen)
        {
            Debug.LogWarning("[SkinSelectionModal] 이미 열려있습니다!");
            return;
        }

        // 효과음
        if (audioService != null && openSfx != null)
        {
            audioService.PlaySfx(openSfx, 0.5f);
        }

        // 스킨 목록 로드
        LoadSkinList();

        // 재화 표시 업데이트
        UpdateCurrencyDisplay();

        SkinData useSkinData = SkinManager.Instance.GetCurrentSkin();

        OnSkinSelected(useSkinData);

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

        // 배경 페이드 인 (약간)
        float elapsed = 0f;
        float duration = 0.2f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            canvasGroup.alpha = t * 0.5f; // 50%까지
            yield return null;
        }

        // 두루마리 펼치기
        yield return StartCoroutine(scrollAnimator.AnimateOpen());

        // 배경 완전히 보이기
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

        // 두루마리 접기
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

        int currency = PlayerDataManager.instance.PullCoin();
        currencyText.text = $"코인: {currency}";
    }

    /// <summary>
    /// 스킨 목록 로드
    /// </summary>
    void LoadSkinList()
    {
        // 기존 아이템 제거
        foreach (var item in skinItemUIList)
        {
            if (item != null)
            {
                Destroy(item.gameObject);
            }
        }
        skinItemUIList.Clear();

        // SkinManager에서 스킨 목록 가져오기
        SkinManager skinManager = SkinManager.Instance;
        if (skinManager == null)
        {
            Debug.LogError("[SkinSelectionModal] SkinManager not found!");
            return;
        }

        List<SkinData> allSkins = skinManager.GetAllSkins();

        // 스킨 아이템 생성
        foreach (SkinData skinData in allSkins)
        {
            GameObject itemObj = Instantiate(skinItemPrefab, skinGridContainer);
            SkinItemUI itemUI = itemObj.GetComponent<SkinItemUI>();

            if (itemUI != null)
            {
                itemUI.Setup(skinData, this);
                skinItemUIList.Add(itemUI);
            }
        }

        // 현재 선택된 스킨 표시
        string currentSkinId = skinManager.GetCurrentSkinId();
        UpdateSelectedSkin(currentSkinId);
    }

    /// <summary>
    /// 스킨 선택 (해금된 스킨만)
    /// </summary>
    public void OnSkinSelected(SkinData skinData)
    {
        // 효과음
        if (audioService != null && selectSfx != null)
        {
            audioService.PlaySfx(selectSfx, 0.7f);
        }

        // 스킨 적용
        //SelectSkin();
        currentSelectedSkin = skinData;

        // 클릭한 스킨이 전체 리스트에서 몇 번째인지 찾아서 동기화
        List<SkinData> allSkins = SkinManager.Instance.GetAllSkins();
        currentIndex = allSkins.FindIndex(s => s.skinId == skinData.skinId);

        // UI 업데이트
        UpdateSelectedSkin(skinData.skinId);

        // 미리보기 업데이트 (선택사항)
        UpdatePreview(skinData);

        //Debug.Log($"[SkinSelectionModal] Skin selected: {skinData.skinName}");
    }

public void SelectSkin()
    {
        if (currentSelectedSkin == null) return;

        if (currentSelectedSkin.isLocked)
        {
            ShowUnlockConfirmationModal(currentSelectedSkin);
            return;
        }

        try
        {
            // 1. 시도 로그
            if (currencyText != null) currencyText.text = "장착 시도 중...";

            // 여기가 문제의 구간!
            SkinManager.Instance.SetCurrentSkin(currentSelectedSkin.skinId);
            
            // 2. 무사히 통과했다면
            if (currencyText != null) currencyText.text = "장착 성공!";
        }
        catch (System.Exception e)
        {
            // 3. 에러가 터졌다면 코인 텍스트를 빨간색으로 바꾸고 에러 내용을 화면에 박아버림!
            if (currencyText != null)
            {
                currencyText.color = Color.red;
                currencyText.text = "에러: " + e.Message;
            }
        }
    }

    /// <summary>
    /// 잠금 해제 확인 모달 표시 (SkinItemUI에서 호출)
    /// </summary>
    public void ShowUnlockConfirmationModal(SkinData skinData)
    {
        if (universalModal == null)
        {
            Debug.LogError("[SkinSelectionModal] UniversalModal이 연결되지 않았습니다!");
            return;
        }

        string message = $"{skinData.skinName}을(를) 해금하시겠습니까?\n비용: {skinData.unlockCost} 코인";

        universalModal.Show(
            title: "스킨 해금",
            message: message,
            leftText: "해금",
            rightText: "취소",
            onLeft: () => {
                TryUnlockSkin(skinData);
            },
            onRight: null
        );
    }

    /// <summary>
    /// 스킨 해금 시도 (UniversalModal 콜백에서 호출)
    /// </summary>
    public void TryUnlockSkin(SkinData skinData)
    {
        SkinManager manager = SkinManager.Instance;

        // 재화 확인
        if (manager.CanUnlock(skinData))
        {
            // 해금 실행
            manager.UnlockSkin(skinData.skinId);

            // 효과음
            if (audioService != null && unlockSfx != null)
            {
                audioService.PlaySfx(unlockSfx, 1.0f);
            }

            // UI 갱신
            LoadSkinList();
            //UpdateCurrencyDisplay();
            OnSkinSelected(skinData);

            Debug.Log($"[SkinSelectionModal] 스킨 해금 완료: {skinData.skinName}");

            // TODO: 해금 축하 연출
        }
        else
        {
            Debug.Log($"[SkinSelectionModal] 재화 부족!");

            // TODO: 재화 부족 알림
        }
    }

    /// <summary>
    /// 선택된 스킨 UI 업데이트
    /// </summary>
    void UpdateSelectedSkin(string skinId)
    {
        foreach (var itemUI in skinItemUIList)
        {
            if (itemUI != null)
            {
                itemUI.SetSelected(itemUI.SkinData.skinId == skinId);
                //currentSelectedSkin = itemUI.SkinData;
            }
        }

        
    }

    /// <summary>
    /// 미리보기 업데이트
    /// </summary>
    void UpdatePreview(SkinData skinData)
    {
        if (previewSpineCharacter == null) return;

        // TODO: Spine 캐릭터 변경 로직

        previewSpineCharacterName.text = skinData.skinName;

        previewSpineCharacter.SpineChange();

        // SpineCharacterController 등을 통해 캐릭터 변경
        Debug.Log($"[SkinSelectionModal] Preview updated: {skinData.skinName}");
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
    }

    /// <summary>
    /// 화살표 버튼 클릭 (방향: 왼쪽 -1, 오른쪽 1)
    /// </summary>
    public void OnClickArrow(int direction)
    {
        List<SkinData> allSkins = SkinManager.Instance.GetAllSkins();
        if (allSkins.Count == 0) return;

        // 인덱스 이동
        currentIndex += direction;

        // 인덱스 범위 순환 (끝에서 오른쪽 누르면 처음으로, 처음에서 왼쪽 누르면 끝으로)
        if (currentIndex < 0) currentIndex = allSkins.Count - 1;
        else if (currentIndex >= allSkins.Count) currentIndex = 0;

        SkinData targetSkin = allSkins[currentIndex];

        // 만약 잠겨있는 스킨이라면 바로 장착하지 않고 미리보기와 테두리만 보여줍니다.
        // 잠겨있지 않다면 클릭한 것과 동일하게 처리합니다.
        if (targetSkin.isLocked)
        {
            currentSelectedSkin = targetSkin;
            UpdateSelectedSkin(targetSkin.skinId);
            UpdatePreview(targetSkin);
        }
        else
        {
            OnSkinSelected(targetSkin);
        }
    }
}