using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class EdgeAnchor : MonoBehaviour
{
    [Header("카메라 설정")]
    [SerializeField] private Camera targetCamera;

    [Header("앵커 위치")]
    [SerializeField] private AnchorPosition anchorPosition = AnchorPosition.TopLeft;

    [Header("오프셋")]
    [SerializeField] private Vector2 offset = Vector2.zero;

    [Header("자동 스케일")]
    [SerializeField] private bool autoScale = false;
    [SerializeField] private float targetWorldSize = 1f;

    [Header("업데이트 설정")]
    [SerializeField] private bool updateOnResolutionChange = true;

    public enum AnchorPosition
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        Center,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }

    private SpriteRenderer spriteRenderer;
    private Vector2 lastScreenSize;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    void Start()
    {
        UpdatePosition();
        lastScreenSize = new Vector2(Screen.width, Screen.height);
    }

    void Update()
    {
        if (updateOnResolutionChange)
        {
            Vector2 currentScreenSize = new Vector2(Screen.width, Screen.height);
            if (currentScreenSize != lastScreenSize)
            {
                UpdatePosition();
                lastScreenSize = currentScreenSize;
            }
        }
    }

    [ContextMenu("Update Position")]
    public void UpdatePosition()
    {
        if (targetCamera == null)
        {
            Debug.LogError("Camera is missing!");
            return;
        }

        // 자동 스케일
        if (autoScale && spriteRenderer != null && spriteRenderer.sprite != null)
        {
            float spriteSize = Mathf.Max(
                spriteRenderer.sprite.bounds.size.x,
                spriteRenderer.sprite.bounds.size.y
            );
            float scale = targetWorldSize / spriteSize;
            transform.localScale = new Vector3(scale, scale, 1f);
        }

        // 카메라가 보는 월드 좌표 범위
        float cameraHeight = targetCamera.orthographicSize * 2f;
        float cameraWidth = cameraHeight * targetCamera.aspect;

        Vector3 cameraPos = targetCamera.transform.position;

        // Sprite의 현재 크기 (스케일 적용 후)
        Vector2 currentSpriteSize = Vector2.zero;
        if (spriteRenderer != null && spriteRenderer.sprite != null)
        {
            currentSpriteSize = spriteRenderer.bounds.size;
        }

        // 앵커 위치 계산 (매개변수 이름 변경!)
        Vector3 newPosition = CalculateEdgePosition(
            cameraPos, cameraWidth, cameraHeight,
            currentSpriteSize, // 변수명 변경
            anchorPosition, offset
        );

        // Z 위치는 유지
        newPosition.z = transform.position.z;

        transform.position = newPosition;

        Debug.Log($"Edge anchored - Position: {anchorPosition}, " +
                  $"World Pos: {newPosition}, Camera Size: {cameraWidth:F2}x{cameraHeight:F2}");
    }

    // 매개변수 이름 변경: spriteSize → spriteBounds
    Vector3 CalculateEdgePosition(
        Vector3 cameraPos, float cameraW, float cameraH,
        Vector2 spriteBounds, // 이름 변경!
        AnchorPosition position, Vector2 posOffset)
    {
        Vector3 pos = cameraPos;

        // 가로 위치
        switch (position)
        {
            case AnchorPosition.TopLeft:
            case AnchorPosition.MiddleLeft:
            case AnchorPosition.BottomLeft:
                pos.x = cameraPos.x - (cameraW / 2f) + (spriteBounds.x / 2f);
                break;

            case AnchorPosition.TopCenter:
            case AnchorPosition.Center:
            case AnchorPosition.BottomCenter:
                pos.x = cameraPos.x;
                break;

            case AnchorPosition.TopRight:
            case AnchorPosition.MiddleRight:
            case AnchorPosition.BottomRight:
                pos.x = cameraPos.x + (cameraW / 2f) - (spriteBounds.x / 2f);
                break;
        }

        // 세로 위치
        switch (position)
        {
            case AnchorPosition.TopLeft:
            case AnchorPosition.TopCenter:
            case AnchorPosition.TopRight:
                pos.y = cameraPos.y + (cameraH / 2f) - (spriteBounds.y / 2f);
                break;

            case AnchorPosition.MiddleLeft:
            case AnchorPosition.Center:
            case AnchorPosition.MiddleRight:
                pos.y = cameraPos.y;
                break;

            case AnchorPosition.BottomLeft:
            case AnchorPosition.BottomCenter:
            case AnchorPosition.BottomRight:
                pos.y = cameraPos.y - (cameraH / 2f) + (spriteBounds.y / 2f);
                break;
        }

        // 오프셋 적용
        pos.x += posOffset.x;
        pos.y += posOffset.y;

        return pos;
    }

    // 런타임에 위치 변경
    public void SetAnchorPosition(AnchorPosition position)
    {
        anchorPosition = position;
        UpdatePosition();
    }

    // 오프셋 변경
    public void SetOffset(Vector2 newOffset)
    {
        offset = newOffset;
        UpdatePosition();
    }

    // Gizmo로 시각화
    void OnDrawGizmosSelected()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (targetCamera == null)
            return;

        // 화면 경계 표시
        float cameraHeight = targetCamera.orthographicSize * 2f;
        float cameraWidth = cameraHeight * targetCamera.aspect;

        Gizmos.color = Color.yellow;
        Vector3 cameraPos = targetCamera.transform.position;

        // 화면 테두리
        Vector3[] corners = new Vector3[5];
        corners[0] = new Vector3(cameraPos.x - cameraWidth / 2, cameraPos.y + cameraHeight / 2, 0);
        corners[1] = new Vector3(cameraPos.x + cameraWidth / 2, cameraPos.y + cameraHeight / 2, 0);
        corners[2] = new Vector3(cameraPos.x + cameraWidth / 2, cameraPos.y - cameraHeight / 2, 0);
        corners[3] = new Vector3(cameraPos.x - cameraWidth / 2, cameraPos.y - cameraHeight / 2, 0);
        corners[4] = corners[0];

        for (int i = 0; i < 4; i++)
        {
            Gizmos.DrawLine(corners[i], corners[i + 1]);
        }

        // 앵커 포인트 표시
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 0.2f);
    }
}