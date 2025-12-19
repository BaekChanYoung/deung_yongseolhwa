using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// World Space 기반 컷신 및 배경 관리 시스템
/// GameObject의 SpriteRenderer를 사용하여 배경/컷신 표시
/// </summary>
public class CutsceneManager : MonoBehaviour
{
    [System.Serializable]
    public class CutsceneData
    {
        [Tooltip("컷신 식별자 (TSV의 [CUTSCENE:이름]과 매칭)")]
        public string cutsceneName;

        [Tooltip("표시할 스프라이트")]
        public Sprite cutsceneSprite;

        [Tooltip("컷신 표시 시간 (초)")]
        public float duration = 2f;

        [Tooltip("페이드 인 시간")]
        public float fadeInTime = 0.5f;

        [Tooltip("페이드 아웃 시간")]
        public float fadeOutTime = 0.5f;

        [Tooltip("컷신을 배경으로 유지할지 여부 (true면 페이드 아웃 안하고 배경으로 전환)")]
        public bool keepAsBackground = false;
    }

    [Header("World Space Background/Cutscene Settings")]
    [Tooltip("배경/컷신을 표시할 GameObject (SpriteRenderer 필요)")]
    [SerializeField] private GameObject backgroundObject;

    [Tooltip("배경 SpriteRenderer (자동 할당 가능)")]
    [SerializeField] private SpriteRenderer backgroundRenderer;

    [Tooltip("컷신 전용 GameObject (선택사항, 없으면 backgroundObject 사용)")]
    [SerializeField] private GameObject cutsceneObject;

    [Tooltip("컷신 SpriteRenderer")]
    [SerializeField] private SpriteRenderer cutsceneRenderer;

    [Header("Cutscene Data")]
    [SerializeField] private CutsceneData[] cutscenes;

    [Header("Background Data")]
    [SerializeField] private BackgroundData[] backgrounds;

    [System.Serializable]
    public class BackgroundData
    {
        [Tooltip("배경 식별자")]
        public string backgroundName;

        [Tooltip("배경 스프라이트")]
        public Sprite backgroundSprite;
    }

    [Header("Sorting Layer Settings")]
    [Tooltip("배경 Sorting Layer 이름")]
    [SerializeField] private string backgroundSortingLayer = "Background";

    [Tooltip("배경 Order in Layer")]
    [SerializeField] private int backgroundOrderInLayer = 0;

    [Tooltip("컷신 Sorting Layer 이름")]
    [SerializeField] private string cutsceneSortingLayer = "Cutscene";

    [Tooltip("컷신 Order in Layer (캐릭터보다 앞에)")]
    [SerializeField] private int cutsceneOrderInLayer = 100;

    private Coroutine currentCutsceneCoroutine;
    private Sprite currentBackgroundSprite;

    private void Awake()
    {
        // SpriteRenderer 자동 할당
        if (backgroundObject != null && backgroundRenderer == null)
        {
            backgroundRenderer = backgroundObject.GetComponent<SpriteRenderer>();

            if (backgroundRenderer == null)
            {
                backgroundRenderer = backgroundObject.AddComponent<SpriteRenderer>();
                Debug.Log("[CutsceneManager] SpriteRenderer added to backgroundObject");
            }
        }

        if (cutsceneObject != null && cutsceneRenderer == null)
        {
            cutsceneRenderer = cutsceneObject.GetComponent<SpriteRenderer>();

            if (cutsceneRenderer == null)
            {
                cutsceneRenderer = cutsceneObject.AddComponent<SpriteRenderer>();
                Debug.Log("[CutsceneManager] SpriteRenderer added to cutsceneObject");
            }
        }

        // Sorting Layer 설정
        SetupSortingLayers();

        // 초기 상태 설정
        if (cutsceneRenderer != null)
        {
            Color c = cutsceneRenderer.color;
            cutsceneRenderer.color = new Color(c.r, c.g, c.b, 0f);

            if (cutsceneObject != null)
            {
                cutsceneObject.SetActive(false);
            }
        }

        // 배경 초기 스프라이트 저장
        if (backgroundRenderer != null && backgroundRenderer.sprite != null)
        {
            currentBackgroundSprite = backgroundRenderer.sprite;
        }
    }

    /// <summary>
    /// Sorting Layer 초기 설정
    /// </summary>
    private void SetupSortingLayers()
    {
        if (backgroundRenderer != null)
        {
            backgroundRenderer.sortingLayerName = backgroundSortingLayer;
            backgroundRenderer.sortingOrder = backgroundOrderInLayer;
            Debug.Log($"[CutsceneManager] Background sorting: {backgroundSortingLayer}, Order: {backgroundOrderInLayer}");
        }

        if (cutsceneRenderer != null)
        {
            cutsceneRenderer.sortingLayerName = cutsceneSortingLayer;
            cutsceneRenderer.sortingOrder = cutsceneOrderInLayer;
            Debug.Log($"[CutsceneManager] Cutscene sorting: {cutsceneSortingLayer}, Order: {cutsceneOrderInLayer}");
        }
    }

    /// <summary>
    /// 컷신 재생
    /// </summary>
    public void PlayCutscene(string cutsceneName, System.Action onComplete = null)
    {
        CutsceneData data = GetCutsceneData(cutsceneName);

        if (data == null)
        {
            Debug.LogWarning($"[CutsceneManager] Cutscene '{cutsceneName}' not found!");
            onComplete?.Invoke();
            return;
        }

        if (currentCutsceneCoroutine != null)
        {
            StopCoroutine(currentCutsceneCoroutine);
        }

        currentCutsceneCoroutine = StartCoroutine(PlayCutsceneCoroutine(data, onComplete));
    }

    /// <summary>
    /// 컷신 재생 코루틴
    /// </summary>
    private IEnumerator PlayCutsceneCoroutine(CutsceneData data, System.Action onComplete)
    {
        Debug.Log($"[CutsceneManager] Playing cutscene: {data.cutsceneName}");

        // 컷신용 Renderer 결정 (전용 오브젝트가 있으면 사용, 없으면 배경 사용)
        SpriteRenderer targetRenderer = cutsceneRenderer != null ? cutsceneRenderer : backgroundRenderer;
        GameObject targetObject = cutsceneRenderer != null ? cutsceneObject : backgroundObject;

        if (targetRenderer == null)
        {
            Debug.LogError("[CutsceneManager] No SpriteRenderer available for cutscene!");
            onComplete?.Invoke();
            yield break;
        }

        // 스프라이트 설정
        targetRenderer.sprite = data.cutsceneSprite;

        // 오브젝트 활성화
        if (targetObject != null)
        {
            targetObject.SetActive(true);
        }

        // 페이드 인
        yield return StartCoroutine(FadeSpriteRenderer(targetRenderer, 0f, 1f, data.fadeInTime));

        // 표시 시간 대기 (클릭으로 스킵 가능)
        float elapsed = 0f;
        while (elapsed < data.duration)
        {
            // 클릭으로 스킵
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("[CutsceneManager] Cutscene skipped by input");
                break;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // keepAsBackground가 true면 배경으로 유지
        if (data.keepAsBackground)
        {
            Debug.Log($"[CutsceneManager] Keeping cutscene '{data.cutsceneName}' as background");

            // 컷신을 배경으로 전환
            if (backgroundRenderer != null)
            {
                backgroundRenderer.sprite = data.cutsceneSprite;
                currentBackgroundSprite = data.cutsceneSprite;
            }

            // 컷신 전용 오브젝트 사용 중이었다면 숨김
            if (cutsceneRenderer != null && targetRenderer == cutsceneRenderer)
            {
                yield return StartCoroutine(FadeSpriteRenderer(cutsceneRenderer, 1f, 0f, 0.3f));

                if (cutsceneObject != null)
                {
                    cutsceneObject.SetActive(false);
                }
            }
        }
        else
        {
            // 페이드 아웃
            yield return StartCoroutine(FadeSpriteRenderer(targetRenderer, 1f, 0f, data.fadeOutTime));

            // 비활성화 (컷신 전용 오브젝트만)
            if (cutsceneRenderer != null && targetObject == cutsceneObject)
            {
                cutsceneObject.SetActive(false);
            }
        }

        currentCutsceneCoroutine = null;

        Debug.Log($"[CutsceneManager] Cutscene '{data.cutsceneName}' completed");
        onComplete?.Invoke();
    }

    /// <summary>
    /// 배경 이미지 즉시 변경
    /// </summary>
    public void ChangeBackground(string backgroundName)
    {
        BackgroundData data = GetBackgroundData(backgroundName);

        if (data == null)
        {
            Debug.LogWarning($"[CutsceneManager] Background '{backgroundName}' not found!");
            return;
        }

        if (backgroundRenderer != null)
        {
            backgroundRenderer.sprite = data.backgroundSprite;
            currentBackgroundSprite = data.backgroundSprite;
            Debug.Log($"[CutsceneManager] Background changed to: {backgroundName}");
        }
    }

    /// <summary>
    /// 배경 이미지 변경 (페이드 효과 포함)
    /// </summary>
    public void ChangeBackgroundWithFade(string backgroundName, float fadeDuration = 0.5f)
    {
        StartCoroutine(ChangeBackgroundWithFadeCoroutine(backgroundName, fadeDuration));
    }

    private IEnumerator ChangeBackgroundWithFadeCoroutine(string backgroundName, float fadeDuration)
    {
        BackgroundData data = GetBackgroundData(backgroundName);

        if (data == null || backgroundRenderer == null)
        {
            Debug.LogWarning($"[CutsceneManager] Cannot change background to '{backgroundName}'");
            yield break;
        }

        // 페이드 아웃
        yield return StartCoroutine(FadeSpriteRenderer(backgroundRenderer, 1f, 0f, fadeDuration / 2f));

        // 스프라이트 변경
        backgroundRenderer.sprite = data.backgroundSprite;
        currentBackgroundSprite = data.backgroundSprite;

        // 페이드 인
        yield return StartCoroutine(FadeSpriteRenderer(backgroundRenderer, 0f, 1f, fadeDuration / 2f));

        Debug.Log($"[CutsceneManager] Background changed to: {backgroundName}");
    }

    /// <summary>
    /// SpriteRenderer 페이드 효과
    /// </summary>
    private IEnumerator FadeSpriteRenderer(SpriteRenderer renderer, float startAlpha, float endAlpha, float duration)
    {
        if (renderer == null) yield break;

        Color startColor = renderer.color;
        startColor.a = startAlpha;
        renderer.color = startColor;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, t);

            Color newColor = renderer.color;
            newColor.a = alpha;
            renderer.color = newColor;

            yield return null;
        }

        Color finalColor = renderer.color;
        finalColor.a = endAlpha;
        renderer.color = finalColor;
    }

    /// <summary>
    /// 컷신 데이터 검색
    /// </summary>
    private CutsceneData GetCutsceneData(string cutsceneName)
    {
        foreach (var cutscene in cutscenes)
        {
            if (cutscene.cutsceneName.Equals(cutsceneName, System.StringComparison.OrdinalIgnoreCase))
            {
                return cutscene;
            }
        }
        return null;
    }

    /// <summary>
    /// 배경 데이터 검색
    /// </summary>
    private BackgroundData GetBackgroundData(string backgroundName)
    {
        foreach (var bg in backgrounds)
        {
            if (bg.backgroundName.Equals(backgroundName, System.StringComparison.OrdinalIgnoreCase))
            {
                return bg;
            }
        }
        return null;
    }

    /// <summary>
    /// 컷신 즉시 종료
    /// </summary>
    public void SkipCutscene()
    {
        if (currentCutsceneCoroutine != null)
        {
            StopCoroutine(currentCutsceneCoroutine);
            currentCutsceneCoroutine = null;

            // 컷신 페이드 아웃 없이 즉시 숨김
            if (cutsceneRenderer != null)
            {
                Color c = cutsceneRenderer.color;
                cutsceneRenderer.color = new Color(c.r, c.g, c.b, 0f);

                if (cutsceneObject != null)
                {
                    cutsceneObject.SetActive(false);
                }
            }

            Debug.Log("[CutsceneManager] Cutscene skipped");
        }
    }

    /// <summary>
    /// 현재 배경 스프라이트 반환
    /// </summary>
    public Sprite GetCurrentBackground()
    {
        return currentBackgroundSprite;
    }

    /// <summary>
    /// 배경을 기본 상태로 복원
    /// </summary>
    public void RestoreDefaultBackground()
    {
        if (backgrounds != null && backgrounds.Length > 0)
        {
            ChangeBackground(backgrounds[0].backgroundName);
        }
    }
}