using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 일러스트 Grid의 개별 아이템 UI
/// </summary>
public class IllustrationItemUI : MonoBehaviour
{
    [Header("UI References")]
    public Image thumbnailImage;
    public Image lockOverlay;
    public Image lockIcon;
    public Text costText;
    public Button selectButton;

    [Header("Lock Settings")]
    public Color lockOverlayColor = new Color(0, 0, 0, 0.7f);

    private IllustrationData illustrationData;
    private IllustrationModal parentModal;

    public IllustrationData IllustrationData => illustrationData;

    /// <summary>
    /// 아이템 설정
    /// </summary>
    public void Setup(IllustrationData data, IllustrationModal modal)
    {
        illustrationData = data;
        parentModal = modal;

        UpdateUI();

        // 버튼 이벤트
        if (selectButton != null)
        {
            selectButton.onClick.AddListener(OnClicked);
        }
    }

    /// <summary>
    /// UI 업데이트
    /// </summary>
    void UpdateUI()
    {
        // 썸네일
        if (thumbnailImage != null && illustrationData.thumbnailSprite != null)
        {
            thumbnailImage.sprite = illustrationData.thumbnailSprite;
        }

        // 잠금 상태
        bool isLocked = !illustrationData.isUnlocked;

        if (lockOverlay != null)
        {
            lockOverlay.gameObject.SetActive(isLocked);
            lockOverlay.color = lockOverlayColor;
        }

        if (lockIcon != null)
        {
            lockIcon.gameObject.SetActive(isLocked);
        }

        // 비용 표시
        if (costText != null)
        {
            costText.gameObject.SetActive(isLocked);
            if (isLocked)
            {
                costText.text = illustrationData.unlockCost.ToString();
            }
        }
    }

    /// <summary>
    /// 클릭 이벤트
    /// </summary>
    void OnClicked()
    {
        if (!illustrationData.isUnlocked)
        {
            // 잠금 해제 확인 모달
            ShowUnlockConfirmation();
        }
        else
        {
            // 전체보기
            if (parentModal != null)
            {
                parentModal.OnIllustrationSelected(illustrationData);
            }
        }
    }

    /// <summary>
    /// 잠금 해제 확인 모달 표시
    /// </summary>
    void ShowUnlockConfirmation()
    {
        // parentModal을 통해 접근 (FindObjectOfType 제거!)
        if (parentModal != null)
        {
            parentModal.ShowUnlockConfirmationModal(illustrationData);
        }
    }

    void OnDestroy()
    {
        if (selectButton != null)
        {
            selectButton.onClick.RemoveListener(OnClicked);
        }
    }
}