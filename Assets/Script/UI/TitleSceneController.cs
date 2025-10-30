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

    [Header("Audio")]
    [Tooltip("버튼 클릭 효과음")]
    public AudioClip clickSfx;

    [Header("Target Scene")]
    public string nextSceneName = SceneNames.START;

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

    void Update()
    {
        // 아무 키나 누르면 시작 (선택사항)
        if (Input.anyKeyDown && !Input.GetMouseButtonDown(0))
        {
            OnStartButtonClicked();
        }
    }

    void OnStartButtonClicked()
    {
        if (sceneService != null && sceneService.IsLoading)
        {
            return;
        }

        Debug.Log($"[TitleScene] 시작 버튼 클릭! → {nextSceneName}");

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
            sceneService.LoadSceneWithLoading(nextSceneName);
        }
    }

    IEnumerator AnimateWaveMotion()
    {
        while (true)
        {
            float time = Time.time * animationSpeed;

            // 위아래 움직임
            float yOffset = Mathf.Sin(time) * intensity * 10f;

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