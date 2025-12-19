using UnityEngine;

/// <summary>
/// 다양한 방식으로 배경 자동 맞춤을 지원
/// 1. OnEnable (활성화될 때)
/// 2. Sprite 변경 감지
/// 3. 수동 호출
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class AutoFitBackground : MonoBehaviour
{
    [Header("맞춤 설정")]
    [Tooltip("true: 화면 꽉 채움, false: 화면에 맞춤")]
    [SerializeField] private bool fillScreen = true;

    [Tooltip("추가 여백")]
    [SerializeField] private float padding = 0f;

    [Tooltip("카메라 중심에 배치")]
    [SerializeField] private bool centerToCamera = true;

    [Header("자동 갱신")]
    [Tooltip("GameObject 활성화 시 자동 맞춤")]
    [SerializeField] private bool fitOnEnable = true;

    [Tooltip("Sprite 변경 감지")]
    [SerializeField] private bool detectSpriteChange = true;

    [Tooltip("해상도 변경 감지 (성능 주의)")]
    [SerializeField] private bool detectResolutionChange = false;

    private SpriteRenderer spriteRenderer;
    private Camera targetCamera;
    private Sprite lastSprite;
    private Vector2 lastScreenSize;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        targetCamera = Camera.main;

        if (spriteRenderer != null)
            lastSprite = spriteRenderer.sprite;
    }

    void OnEnable()
    {
        if (fitOnEnable)
        {
            FitToScreen();
        }
    }

    void Update()
    {
        // Sprite 변경 감지
        if (detectSpriteChange && spriteRenderer != null)
        {
            if (spriteRenderer.sprite != lastSprite)
            {
                lastSprite = spriteRenderer.sprite;
                FitToScreen();
                Debug.Log($"[{gameObject.name}] Sprite changed - refitting");
            }
        }

        // 해상도 변경 감지
        if (detectResolutionChange)
        {
            Vector2 currentSize = new Vector2(Screen.width, Screen.height);
            if (currentSize != lastScreenSize)
            {
                lastScreenSize = currentSize;
                FitToScreen();
                Debug.Log($"[{gameObject.name}] Resolution changed - refitting");
            }
        }
    }

    [ContextMenu("Fit To Screen")]
    public void FitToScreen()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (targetCamera == null)
            targetCamera = Camera.main;

        if (spriteRenderer == null || spriteRenderer.sprite == null || targetCamera == null)
        {
            Debug.LogWarning($"[{gameObject.name}] Cannot fit - missing components!");
            return;
        }

        // 카메라 크기
        float camHeight = targetCamera.orthographicSize * 2f;
        float camWidth = camHeight * targetCamera.aspect;

        // 목표 크기
        float targetW = camWidth + padding * 2f;
        float targetH = camHeight + padding * 2f;

        // Sprite 크기
        float spriteW = spriteRenderer.sprite.bounds.size.x;
        float spriteH = spriteRenderer.sprite.bounds.size.y;

        // 스케일 계산
        float scaleX = targetW / spriteW;
        float scaleY = targetH / spriteH;
        float scale = fillScreen ? Mathf.Max(scaleX, scaleY) : Mathf.Min(scaleX, scaleY);

        // 적용
        transform.localScale = new Vector3(scale, scale, 1f);

        // 중앙 배치
        if (centerToCamera)
        {
            Vector3 pos = transform.position;
            pos.x = targetCamera.transform.position.x;
            pos.y = targetCamera.transform.position.y;
            transform.position = pos;
        }
    }

    // 외부에서 설정 변경
    public void SetFillScreen(bool fill)
    {
        fillScreen = fill;
        FitToScreen();
    }

    public void SetPadding(float newPadding)
    {
        padding = newPadding;
        FitToScreen();
    }
}