using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 스킨 선택 Grid의 개별 아이템 UI
/// </summary>
public class SkinItemUI : MonoBehaviour
{
    [Header("UI References")]
    public Image thumbnailImage;
    public Image lockIcon;
    public Image selectedBorder;
    public Button selectButton;
    public Image backgroundImage;

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color selectedColor = Color.yellow;
    public Color lockedColor = Color.gray;

    private SkinData skinData;
    private SkinSelectionModal parentModal;
    private bool isSelected = false;

    public SkinData SkinData => skinData;

    /// <summary>
    /// 아이템 설정
    /// </summary>
    public void Setup(SkinData data, SkinSelectionModal modal)
    {
        skinData = data;
        parentModal = modal;

        // 썸네일
        if (thumbnailImage != null && skinData.thumbnailSprite != null)
        {
            thumbnailImage.sprite = skinData.thumbnailSprite;
        }

        // 잠금 아이콘
        if (lockIcon != null)
        {
            lockIcon.gameObject.SetActive(skinData.isLocked);
        }

        // 잠금 상태면 어둡게
        if (skinData.isLocked && backgroundImage != null)
        {
            backgroundImage.color = lockedColor;
        }

        // 선택 테두리 숨김
        if (selectedBorder != null)
        {
            selectedBorder.gameObject.SetActive(false);
        }

        // 버튼 이벤트
        if (selectButton != null)
        {
            selectButton.onClick.AddListener(OnClicked);
        }
    }

    /// <summary>
    /// 선택 상태 설정
    /// </summary>
    public void SetSelected(bool selected)
    {
        isSelected = selected;

        if (selectedBorder != null)
        {
            selectedBorder.gameObject.SetActive(selected);
        }

        if (backgroundImage != null && !skinData.isLocked)
        {
            backgroundImage.color = selected ? selectedColor : normalColor;
        }
    }

    /// <summary>
    /// 클릭 이벤트
    /// </summary>
    void OnClicked()
    {
        if (skinData.isLocked)
        {
            // 잠금 해제 확인 모달 표시
            ShowUnlockConfirmation();
            return;
        }

        // 스킨 선택
        if (parentModal != null)
        {
            parentModal.OnSkinSelected(skinData);
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
            parentModal.ShowUnlockConfirmationModal(skinData);
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