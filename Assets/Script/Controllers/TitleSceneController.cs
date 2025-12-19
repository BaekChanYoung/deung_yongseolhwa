using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Title Scene을 제어하는 컨트롤러
/// "눌러서 시작하기" 버튼
/// </summary>
public class TitleSceneController : MonoBehaviour
{
    [Header("UI")]
    [Tooltip("눌러서 시작하기 버튼")]
    public Button startButton;
    public Image buttonImage;
    public Image brushBackground;

    [Header("Animation Settings")]
    [Range(0.5f, 3f)]
    [Tooltip("애니메이션 속도 (모바일: 1.0 권장)")]
    public float animationSpeed = 1.5f;

    [Range(0f, 1f)]
    [Tooltip("애니메이션 강도 (모바일: 0.3~0.5 권장)")]
    public float intensity = 0.4f;

    [Range(5f, 20f)]
    [Tooltip("위아래 움직임 범위")]
    public float moveRange = 10f;

    [Header("Audio")]
    [Tooltip("버튼 클릭 효과음")]
    public AudioClip clickSfx;

    [Header("Target Scene")]
    [Tooltip("최초 실행 시 이동할 씬")]
    public string firstTimeScene = SceneNames.DIALOGUE;
    [Tooltip("재실행 시 이동할 씬")]
    public string normalScene = SceneNames.START;

    [Header("First Time Initialization")]
    [Tooltip("최초 실행 시 지급할 스킨 코인")]
    public int initialSkinCurrency = 1000;

    [Tooltip("최초 실행 시 지급할 일러스트 재화")]
    public int initialIllustrationCurrency = 500;

    [Tooltip("기본 해금 스킨 ID (최초 실행 시 자동 해금됨)")]
    public string defaultSkinId = "default";

    [Header("Additional Unlocks (Optional)")]
    [Tooltip("최초 실행 시 추가로 해금할 스킨들 (선택사항)")]
    public string[] additionalSkins;

    [Tooltip("최초 실행 시 추가로 해금할 일러스트들 (선택사항)")]
    public string[] additionalIllustrations;

    [Header("Debug")]
    [Tooltip("디버그 로그 출력 여부")]
    public bool enableDebugLogs = true;

    private ISceneService sceneService;
    private IAudioService audioService;
    private Color originalButtonColor;
    private Color originalBrushColor;
    private Vector3 originalButtonPos;
    private Vector3 originalBrushPos;

    void Start()
    {
        // 서비스 가져오기
        sceneService = ServiceLocator.Resolve<ISceneService>();
        audioService = ServiceLocator.Resolve<IAudioService>();

        // 서비스 확인
        if (sceneService == null)
        {
            Debug.LogError("[TitleScene] ISceneService를 찾을 수 없습니다! SceneService가 Bootstrapper에 등록되어 있는지 확인하세요.");
        }
        if (audioService == null)
        {
            Debug.LogError("[TitleScene] IAudioService를 찾을 수 없습니다! AudioService가 Bootstrapper에 등록되어 있는지 확인하세요.");
        }

        // ========== 최초 실행 데이터 초기화 함수 ==========
        InitializeGameDataIfFirstTime();
        // ===============================================

        // 버튼 이벤트 연결
        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
        }

        if (buttonImage != null)
        {
            originalButtonColor = buttonImage.color;
            originalButtonPos = buttonImage.transform.localPosition;
        }

        if (brushBackground != null)
        {
            originalBrushColor = brushBackground.color;
            originalBrushPos = brushBackground.transform.localPosition;
        }

        // 선택한 애니메이션 시작
        StartCoroutine(AnimateWaveMotion());

        Debug.Log("[TitleScene] 초기화 완료 (모바일 최적화)");
    }

    // ========== 추가: 게임 데이터 초기화 메서드 ==========
    /// <summary>
    /// 최초 실행 시 게임 데이터 초기화
    /// - 재화 지급
    /// - 기본 스킨 해금
    /// - 추가 스킨/일러스트 해금
    /// </summary>
    void InitializeGameDataIfFirstTime()
    {
        if (FirstTimeManager.IsFirstTime())
        {
            if (enableDebugLogs)
            {
                Debug.Log("========================================");
                Debug.Log("[TitleScene] 최초 실행 감지! 게임 데이터 초기화 시작...");
                Debug.Log("========================================");
            }

            // 1. 기본 재화 + 기본 스킨 초기화
            FirstTimeManager.InitializeGameData(
                skinCurrency: initialSkinCurrency,
                illustrationCurrency: initialIllustrationCurrency,
                defaultSkinId: defaultSkinId
            );

            // 2. 추가 스킨 해금 (Inspector에서 설정한 경우)
            if (additionalSkins != null && additionalSkins.Length > 0)
            {
                if (enableDebugLogs)
                {
                    Debug.Log($"[TitleScene] 추가 스킨 해금 ({additionalSkins.Length}개):");
                }

                foreach (string skinId in additionalSkins)
                {
                    if (!string.IsNullOrEmpty(skinId))
                    {
                        FirstTimeManager.UnlockSkinForInit(skinId);

                        if (enableDebugLogs)
                        {
                            Debug.Log($"  - {skinId} 해금 완료");
                        }
                    }
                }
            }

            // 3. 추가 일러스트 해금 (Inspector에서 설정한 경우)
            if (additionalIllustrations != null && additionalIllustrations.Length > 0)
            {
                if (enableDebugLogs)
                {
                    Debug.Log($"[TitleScene] 추가 일러스트 해금 ({additionalIllustrations.Length}개):");
                }

                foreach (string illustId in additionalIllustrations)
                {
                    if (!string.IsNullOrEmpty(illustId))
                    {
                        FirstTimeManager.UnlockIllustrationForInit(illustId);

                        if (enableDebugLogs)
                        {
                            Debug.Log($"  - {illustId} 해금 완료");
                        }
                    }
                }
            }

            // 4. 초기화 완료 상태 확인
            if (enableDebugLogs)
            {
                Debug.Log("========================================");
                Debug.Log("[TitleScene] 게임 데이터 초기화 완료!");
                FirstTimeManager.CheckStatus();
                Debug.Log("========================================");
            }
        }
        else
        {
            if (enableDebugLogs)
            {
                Debug.Log("========================================");
                Debug.Log("[TitleScene] 재실행 감지됨. 저장된 데이터 사용.");
                FirstTimeManager.CheckStatus();
                Debug.Log("========================================");
            }
        }
    }
    // ==========================================

    /*void Update()
    {
        // 아무 키나 누르면 시작 (선택사항)
        if (Input.anyKeyDown && !Input.GetMouseButtonDown(0))
        {
            OnStartButtonClicked();
        }
    }*/

    void OnStartButtonClicked()
    {
        if (sceneService != null && sceneService.IsLoading)
        {
            return;
        }

        // ========== 최초 실행 체크 (추가!) ==========
        bool isFirstTime = FirstTimeManager.IsFirstTime();
        string targetScene = isFirstTime ? firstTimeScene : normalScene;

        Debug.Log("========================================");
        Debug.Log($"[TitleScene] 시작 버튼 클릭!");
        Debug.Log($"  최초 실행: {isFirstTime}");
        Debug.Log($"  목표 씬: {targetScene}");
        Debug.Log("========================================");
        // ==========================================

        sceneService.LoadSceneWithLoading(targetScene);

        if (audioService != null && clickSfx != null)
        {
            audioService.PlaySfx(clickSfx, 0.8f);
        }

        if (startButton != null)
        {
            startButton.interactable = false;
        }

        // 애니메이션 정지
        StopAllCoroutines();

        if (sceneService != null)
        {
            sceneService.LoadSceneWithLoading(targetScene);
        }
    }

    IEnumerator AnimateWaveMotion()
    {
        while (true)
        {
            float time = Time.time * animationSpeed;

            // 위아래 움직임
            float yOffset = Mathf.Sin(time) * intensity * moveRange;

            // 알파 변화
            float normalizedY = Mathf.Sin(time);
            float alpha = 0.85f - Mathf.Sin(time) * 0.15f;

            // 버튼 이미지
            if (buttonImage != null)
            {
                Vector3 pos = originalButtonPos;
                pos.y += yOffset;
                buttonImage.transform.localPosition = pos;

                Color c = originalButtonColor;
                c.a = alpha;
                buttonImage.color = c;
            }

            // 붓터치 배경 (반대 방향 움직임)
            if (brushBackground != null)
            {
                Vector3 pos = originalBrushPos;
                pos.y -= yOffset * 0.3f;
                brushBackground.transform.localPosition = pos;

                float brushAlpha = 0.7f + normalizedY * 0.2f;

                Color c = originalBrushColor;
                c.a = (0.7f + Mathf.Sin(time + Mathf.PI) * 0.2f) * originalBrushColor.a;
                brushBackground.color = c;
            }

            yield return null;
        }
    }

    void OnDestroy()
    {
        if (startButton != null)
            startButton.onClick.RemoveListener(OnStartButtonClicked);
    }
}