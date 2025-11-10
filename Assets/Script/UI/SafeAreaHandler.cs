using UnityEngine;

/// <summary>
/// 모바일 Safe Area 대응 (노치, 펀치홀, 제스처 바 등)
/// Canvas 바로 아래 Empty Object에 붙여서 사용
/// </summary>
public class SafeAreaHandler : MonoBehaviour
{
    RectTransform rectTransform;
    Rect lastSafeArea = new Rect(0, 0, 0, 0);

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    void Update()
    {
        // Safe Area가 변경되면 다시 적용 (회전, 접는 폰 등)
        if (Screen.safeArea != lastSafeArea)
        {
            ApplySafeArea();
        }
    }

    void ApplySafeArea()
    {
        Rect safeArea = Screen.safeArea;

        // Anchor를 Safe Area 비율로 조정
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;

        lastSafeArea = safeArea;

        Debug.Log($"[SafeArea] Safe Area 적용: {safeArea}");
    }
}