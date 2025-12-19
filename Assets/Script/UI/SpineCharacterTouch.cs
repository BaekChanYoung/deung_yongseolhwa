using UnityEngine;
using Spine.Unity;

/// <summary>
/// Spine 캐릭터 터치/클릭 인터랙션
/// World Space Spine 캐릭터에 추가
/// </summary>
public class SpineCharacterTouch : MonoBehaviour
{
    [Header("Spine References")]
    [SerializeField] private SkeletonAnimation skeletonAnimation;

    [Header("Animation Names")]
    [Tooltip("기본 대기 애니메이션")]
    [SerializeField] private string idleAnimationName = "stand";

    [Tooltip("클릭 시 재생할 애니메이션")]
    [SerializeField] private string clickAnimationName = "click";

    [Header("Touch Settings")]
    [Tooltip("터치 감지 방식")]
    [SerializeField] private TouchDetectionMode detectionMode = TouchDetectionMode.Collider;

    [Tooltip("레이캐스트 레이어 (Collider 방식)")]
    [SerializeField] private LayerMask touchLayer = -1;

    [Tooltip("터치 거리 제한 (카메라로부터)")]
    [SerializeField] private float maxTouchDistance = 100f;

    [Header("Animation Settings")]
    [Tooltip("클릭 애니메이션 완료 후 대기로 복귀")]
    [SerializeField] private bool returnToIdleAfterClick = true;

    [Tooltip("클릭 쿨다운 (연속 클릭 방지)")]
    [SerializeField] private float clickCooldown = 0.5f;

    [Header("Collider Settings")]
    [Tooltip("자동으로 크기 설정할지 여부 (Spine Renderer.bounds 기준)")]
    [SerializeField] private bool autoSizeCollider = true;

    [Tooltip("수동 크기 (autoSize가 false일 때)")]
    [SerializeField] private Vector2 manualColliderSize = new Vector2(2f, 3f);

    [Tooltip("여유 크기 배율 (1.0 = 딱 맞게, 1.2 = 20% 여유)")]
    [SerializeField] private float colliderSizeMultiplier = 1.2f;

    [Header("Debug")]
    [Tooltip("Scene 뷰에서 Collider 영역 시각화")]
    [SerializeField] private bool showColliderGizmo = true;
    [SerializeField] private Color gizmoColor = Color.green;

    [Header("Optional Effects")]
    [Tooltip("클릭 시 효과음")]
    [SerializeField] private AudioClip clickSound;

    [Tooltip("클릭 시 파티클 효과")]
    [SerializeField] private ParticleSystem clickParticle;

    public enum TouchDetectionMode
    {
        Collider,       // Collider 기반 (권장)
        BoundingBox     // Spine BoundingBox 기반
    }

    private Camera mainCamera;
    private Collider2D touchCollider;
    private bool canClick = true;
    private float lastClickTime;

    void Awake()
    {
        mainCamera = Camera.main;

        // SkeletonAnimation 자동 찾기
        if (skeletonAnimation == null)
        {
            skeletonAnimation = GetComponentInChildren<SkeletonAnimation>();
        }

        // Collider 자동 찾기/생성
        if (detectionMode == TouchDetectionMode.Collider)
        {
            SetupCollider();
        }

        // 대기 애니메이션 재생
        PlayIdleAnimation();
    }

    void Start()
    {
        // 클릭 애니메이션 완료 시 대기로 복귀 설정
        if (returnToIdleAfterClick && skeletonAnimation != null)
        {
            skeletonAnimation.AnimationState.Complete += OnAnimationComplete;
        }
    }

    void OnDestroy()
    {
        if (skeletonAnimation != null)
        {
            skeletonAnimation.AnimationState.Complete -= OnAnimationComplete;
        }
    }

    void Update()
    {
        // 쿨다운 체크
        if (!canClick && Time.time - lastClickTime >= clickCooldown)
        {
            canClick = true;
        }

        // 터치/클릭 감지
        DetectTouch();
    }

    /// <summary>
    /// Collider 자동 설정
    /// </summary>
    private void SetupCollider()
    {
        touchCollider = GetComponent<Collider2D>();

        if (touchCollider == null)
        {
            // BoxCollider2D 자동 추가
            BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
            boxCollider.isTrigger = true;

            // Spine 크기에 맞게 자동 조정
            if (autoSizeCollider && skeletonAnimation != null)
            {
                Bounds bounds = skeletonAnimation.GetComponent<Renderer>().bounds;
                float width = bounds.size.x * colliderSizeMultiplier;
                float height = bounds.size.y * colliderSizeMultiplier;

                boxCollider.size = new Vector2(width, height);
                boxCollider.offset = Vector2.zero;

                Debug.Log($"[SpineCharacterTouch] Auto collider size: ({width:F2}, {height:F2})");
                Debug.Log($"[SpineCharacterTouch] Renderer bounds: {bounds.size}, Multiplier: {colliderSizeMultiplier}");
            }
            else
            {
                // 수동 크기 또는 skeletonAnimation이 없을 때
                boxCollider.size = manualColliderSize;
                boxCollider.offset = Vector2.zero;

                Debug.Log($"[SpineCharacterTouch] Manual collider size: {manualColliderSize}");
            }

            touchCollider = boxCollider;
            Debug.Log("[SpineCharacterTouch] BoxCollider2D added automatically");
        }
    }

    /// <summary>
    /// 터치 감지
    /// </summary>
    private void DetectTouch()
    {
        if (!canClick) return;

        // 모바일: 터치
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                CheckTouchHit(touch.position);
            }
        }
        // PC: 마우스 클릭
        else if (Input.GetMouseButtonDown(0))
        {
            CheckTouchHit(Input.mousePosition);
        }
    }

    /// <summary>
    /// 터치 히트 체크
    /// </summary>
    private void CheckTouchHit(Vector2 screenPosition)
    {
        if (mainCamera == null) return;

        Ray ray = mainCamera.ScreenPointToRay(screenPosition);

        if (detectionMode == TouchDetectionMode.Collider)
        {
            // Collider 방식 (2D)
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, maxTouchDistance, touchLayer);

            if (hit.collider != null && hit.collider == touchCollider)
            {
                OnCharacterTouched();
            }
        }
        else
        {
            // BoundingBox 방식 (3D)
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, maxTouchDistance))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    OnCharacterTouched();
                }
            }
        }
    }

    /// <summary>
    /// 캐릭터 터치됨
    /// </summary>
    private void OnCharacterTouched()
    {
        if (!canClick) return;

        Debug.Log("[SpineCharacterTouch] Character touched!");

        // 클릭 애니메이션 재생
        PlayClickAnimation();

        // 쿨다운 시작
        canClick = false;
        lastClickTime = Time.time;

        // 효과음 재생
        PlayClickSound();

        // 파티클 효과
        PlayClickParticle();
    }

    /// <summary>
    /// 대기 애니메이션 재생
    /// </summary>
    private void PlayIdleAnimation()
    {
        if (skeletonAnimation == null) return;

        var trackEntry = skeletonAnimation.AnimationState.SetAnimation(0, idleAnimationName, true);
        Debug.Log($"[SpineCharacterTouch] Playing idle animation: {idleAnimationName}");
    }

    /// <summary>
    /// 클릭 애니메이션 재생
    /// </summary>
    private void PlayClickAnimation()
    {
        if (skeletonAnimation == null) return;

        var trackEntry = skeletonAnimation.AnimationState.SetAnimation(0, clickAnimationName, false);
        trackEntry.MixDuration = 0.1f;

        Debug.Log($"[SpineCharacterTouch] Playing click animation: {clickAnimationName}");
    }

    /// <summary>
    /// 애니메이션 완료 콜백
    /// </summary>
    private void OnAnimationComplete(Spine.TrackEntry trackEntry)
    {
        // 클릭 애니메이션 완료 시 대기로 복귀
        if (trackEntry.Animation.Name == clickAnimationName && returnToIdleAfterClick)
        {
            PlayIdleAnimation();
            Debug.Log("[SpineCharacterTouch] Returned to idle animation");
        }
    }

    /// <summary>
    /// 클릭 효과음 재생
    /// </summary>
    private void PlayClickSound()
    {
        if (clickSound == null) return;

        // AudioService 사용 (있으면)
        var audioService = ServiceLocator.Resolve<IAudioService>();
        if (audioService != null)
        {
            audioService.PlaySfx(clickSound, 0.7f);
        }
        else
        {
            // 없으면 기본 AudioSource 사용
            AudioSource.PlayClipAtPoint(clickSound, transform.position);
        }
    }

    /// <summary>
    /// 클릭 파티클 재생
    /// </summary>
    private void PlayClickParticle()
    {
        if (clickParticle == null) return;

        clickParticle.Play();
    }

    // ========== Public Methods ==========

    /// <summary>
    /// 터치 활성화/비활성화
    /// </summary>
    public void SetTouchEnabled(bool enabled)
    {
        this.enabled = enabled;
    }

    /// <summary>
    /// 애니메이션 이름 변경
    /// </summary>
    public void SetAnimations(string idle, string click)
    {
        idleAnimationName = idle;
        clickAnimationName = click;
        PlayIdleAnimation();
    }

    /// <summary>
    /// 강제로 대기 애니메이션 재생
    /// </summary>
    public void ForceIdle()
    {
        PlayIdleAnimation();
        canClick = true;
    }

    // ========== Debug & Visualization ==========

    /// <summary>
    /// Collider 영역 시각화 (Scene 뷰)
    /// </summary>
    void OnDrawGizmos()
    {
        if (!showColliderGizmo) return;

        BoxCollider2D box = GetComponent<BoxCollider2D>();
        if (box == null) return;

        Gizmos.color = gizmoColor;
        Vector3 center = transform.position + (Vector3)box.offset;
        Vector3 size = new Vector3(box.size.x, box.size.y, 0.1f);
        Gizmos.DrawWireCube(center, size);
    }
}