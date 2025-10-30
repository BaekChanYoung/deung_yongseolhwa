using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// LoadingScene 컨트롤러
/// 로딩 애니메이션만 표시
/// </summary>
public class LoadingSceneController : MonoBehaviour
{
    [Header("UI (Optional)")]
    [Tooltip("로딩 아이콘 (회전 애니메이션)")]
    public RectTransform loadingIcon;

    [Tooltip("회전 속도")]
    public float rotationSpeed = 180f;

    [Header("Tips")]
    [Tooltip("팁 텍스트 배열")]
    public string[] loadingTips = new string[]
    {
        "Tip: 마스터 볼륨으로 전체 소리를 조절할 수 있습니다.",
        "Tip: 옵션에서 배경음과 효과음을 따로 조절하세요.",
        "Tip: ESC 키로 옵션 창을 열고 닫을 수 있습니다.",
        "Tip: 설정은 자동으로 저장됩니다."
    };

    [Tooltip("팁 텍스트 UI")]
    public Text tipText;

    void Start()
    {
        Debug.Log("[LoadingScene] 로딩 씬 진입");

        // 랜덤 팁 표시
        if (tipText != null && loadingTips.Length > 0)
        {
            int randomIndex = Random.Range(0, loadingTips.Length);
            tipText.text = loadingTips[randomIndex];
        }
    }

    void Update()
    {
        // 로딩 아이콘 회전 애니메이션
        if (loadingIcon != null)
        {
            loadingIcon.Rotate(0f, 0f, -rotationSpeed * Time.deltaTime);
        }
    }
}
