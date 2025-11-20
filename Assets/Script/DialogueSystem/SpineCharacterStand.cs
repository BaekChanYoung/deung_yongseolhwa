using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using Spine;

/// <summary>
/// Spine 기반 캐릭터 스탠드 (World Space)
/// 표정별 애니메이션 재생 및 등장 효과
/// </summary>
public class SpineCharacterStand : MonoBehaviour
{
    [System.Serializable]
    public class ExpressionMapping
    {
        [Tooltip("Google Sheets의 Expression 값")]
        public string expressionKey;

        [Tooltip("재생할 Spine 애니메이션 이름")]
        public string animationName;

        [Tooltip("화난 표정처럼 흔들림 효과 적용")]
        public bool enableShake = false;
    }

    [Header("Spine Settings")]
    [SerializeField] private SkeletonAnimation skeletonAnimation;
    [SerializeField] private string characterName;

    [Header("Expression Mappings")]
    [Tooltip("Expression → Animation 매핑 (자유롭게 추가/삭제 가능)")]
    [SerializeField]
    private List<ExpressionMapping> expressionMappings = new List<ExpressionMapping>()
    {
        new ExpressionMapping { expressionKey = "Base", animationName = "animation", enableShake = false },
        new ExpressionMapping { expressionKey = "Normal", animationName = "animation", enableShake = false },
        new ExpressionMapping { expressionKey = "Idle", animationName = "animation", enableShake = false }
    };

    [Header("Fallback Animation")]
    [Tooltip("매칭되는 애니메이션이 없을 때 재생할 기본 애니메이션")]
    [SerializeField] private string defaultAnimation = "animation";

    [Header("Shake Settings")]
    [Tooltip("흔들림 효과 활성화")]
    [SerializeField] private bool enableShakeEffect = true;
    [SerializeField] private float shakeAmount = 0.1f;
    [SerializeField] private float shakeDuration = 0.15f;

    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 0.3f;
    [SerializeField] private float moveDistance = 1f;

    private Transform characterTransform;
    private Vector3 originalPosition;
    private Coroutine shakeCoroutine;
    private string currentExpression = "";
    private Color originalColor;

    // 빠른 검색을 위한 Dictionary
    private Dictionary<string, ExpressionMapping> expressionDict;

    private void Awake()
    {
        characterTransform = transform;

        if (skeletonAnimation == null)
        {
            skeletonAnimation = GetComponentInChildren<SkeletonAnimation>();
        }

        originalPosition = characterTransform.position;

        if (skeletonAnimation != null)
        {
            originalColor = skeletonAnimation.Skeleton.GetColor();
        }

        // Dictionary 생성 (대소문자 구분 없이)
        BuildExpressionDictionary();

        Hide();
    }

    /// <summary>
    /// Expression 매핑을 Dictionary로 변환 (빠른 검색)
    /// </summary>
    private void BuildExpressionDictionary()
    {
        expressionDict = new Dictionary<string, ExpressionMapping>(System.StringComparer.OrdinalIgnoreCase);

        foreach (var mapping in expressionMappings)
        {
            if (!string.IsNullOrEmpty(mapping.expressionKey))
            {
                // 중복 체크
                if (expressionDict.ContainsKey(mapping.expressionKey))
                {
                    Debug.LogWarning($"[SpineCharacterStand] Duplicate expression key: '{mapping.expressionKey}' in {characterName}");
                }
                else
                {
                    expressionDict[mapping.expressionKey] = mapping;
                }
            }
        }

        Debug.Log($"[SpineCharacterStand] {characterName} loaded {expressionDict.Count} expression mappings");
    }

    /// <summary>
    /// 캐릭터 등장
    /// </summary>
    public void Show(string position)
    {
        gameObject.SetActive(true);
        StartCoroutine(FadeIn(position));
    }

    /// <summary>
    /// 캐릭터 숨김
    /// </summary>
    public void Hide()
    {
        if (skeletonAnimation != null)
        {
            skeletonAnimation.Skeleton.SetColor(new Color(originalColor.r, originalColor.g, originalColor.b, 0f));
        }

        gameObject.SetActive(false);

        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }

        characterTransform.position = originalPosition;
    }

    /// <summary>
    /// 표정 설정 (Spine 애니메이션 재생)
    /// </summary>
    public void SetExpression(string expression)
    {
        if (skeletonAnimation == null)
        {
            Debug.LogError($"[SpineCharacterStand] SkeletonAnimation is null on {characterName}!");
            return;
        }

        Debug.Log($"[SpineCharacterStand] {characterName} received expression: '{expression}'");

        // 같은 표정이면 스킵
        if (currentExpression.Equals(expression, System.StringComparison.OrdinalIgnoreCase))
        {
            Debug.Log($"[SpineCharacterStand] Same expression, skipping: {expression}");
            return;
        }

        currentExpression = expression;

        // 이전 흔들림 정지
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
            characterTransform.position = originalPosition;
        }

        // Expression 매칭
        ExpressionMapping mapping = GetExpressionMapping(expression);

        if (mapping != null)
        {
            Debug.Log($"[SpineCharacterStand] Mapped '{expression}' → Animation: '{mapping.animationName}' (Shake: {mapping.enableShake})");

            // 애니메이션 재생
            PlaySpineAnimation(mapping.animationName);

            // 흔들림 효과
            if (mapping.enableShake && enableShakeEffect)
            {
                Debug.Log($"[SpineCharacterStand] Starting shake effect for: {expression}");
                shakeCoroutine = StartCoroutine(ShakeLoop());
            }
        }
        else
        {
            Debug.LogWarning($"[SpineCharacterStand] No mapping found for '{expression}', using default: {defaultAnimation}");
            PlaySpineAnimation(defaultAnimation);
        }
    }

    /// <summary>
    /// Expression에 해당하는 매핑 찾기
    /// </summary>
    private ExpressionMapping GetExpressionMapping(string expression)
    {
        if (expressionDict == null || expressionDict.Count == 0)
        {
            BuildExpressionDictionary();
        }

        // Dictionary에서 찾기 (대소문자 구분 없음)
        if (expressionDict.TryGetValue(expression, out ExpressionMapping mapping))
        {
            return mapping;
        }

        return null;
    }

    /// <summary>
    /// Spine 애니메이션 재생
    /// </summary>
    private void PlaySpineAnimation(string animationName, float mixDuration = 0.2f)
    {
        if (skeletonAnimation == null)
        {
            Debug.LogError("[SpineCharacterStand] SkeletonAnimation is null!");
            return;
        }

        Debug.Log($"[SpineCharacterStand] Playing animation: '{animationName}'");

        var trackEntry = skeletonAnimation.state.SetAnimation(0, animationName, true);

        if (trackEntry != null)
        {
            trackEntry.MixDuration = mixDuration;
            Debug.Log($"[SpineCharacterStand] Animation '{animationName}' started successfully!");
        }
        else
        {
            Debug.LogWarning($"[SpineCharacterStand] Animation '{animationName}' not found! Using default: {defaultAnimation}");
            skeletonAnimation.state.SetAnimation(0, defaultAnimation, true);
        }
    }

    /// <summary>
    /// 흔들림 효과 반복
    /// </summary>
    private IEnumerator ShakeLoop()
    {
        // 2번만 반복 (한 반복당 위로 이동 + 아래로 이동)
        for (int i = 0; i < 2; i++)
        {
            yield return StartCoroutine(MoveToPosition(originalPosition + Vector3.up * shakeAmount, shakeDuration));
            yield return StartCoroutine(MoveToPosition(originalPosition + Vector3.down * shakeAmount, shakeDuration));
        }

        // 종료 시 원래 위치로 복구 및 코루틴 레퍼런스 정리
        characterTransform.position = originalPosition;
        shakeCoroutine = null;
    }

    /// <summary>
    /// 특정 위치로 부드럽게 이동
    /// </summary>
    private IEnumerator MoveToPosition(Vector3 targetPos, float duration)
    {
        Vector3 startPos = characterTransform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            characterTransform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        characterTransform.position = targetPos;
    }

    /// <summary>
    /// 페이드 인 효과
    /// </summary>
    private IEnumerator FadeIn(string position)
    {
        if (skeletonAnimation == null)
        {
            Debug.LogError("[SpineCharacterStand] SkeletonAnimation is null in FadeIn!");
            yield break;
        }

        Vector3 startPos = originalPosition;

        switch (position.ToLower())
        {
            case "left":
                startPos = new Vector3(originalPosition.x - moveDistance, originalPosition.y, originalPosition.z);
                break;
            case "right":
                startPos = new Vector3(originalPosition.x + moveDistance, originalPosition.y, originalPosition.z);
                break;
        }

        characterTransform.position = startPos;

        var skeleton = skeletonAnimation.Skeleton;
        Color startColor = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        Color endColor = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
        skeleton.SetColor(startColor);

        float elapsed = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeInDuration;

            Color currentColor = Color.Lerp(startColor, endColor, t);
            skeleton.SetColor(currentColor);

            characterTransform.position = Vector3.Lerp(startPos, originalPosition, t);

            yield return null;
        }

        skeleton.SetColor(endColor);
        characterTransform.position = originalPosition;
    }

    /// <summary>
    /// Inspector에서 매핑 추가 (에디터 전용)
    /// </summary>
    [ContextMenu("Add Expression Mapping")]
    private void AddExpressionMapping()
    {
        expressionMappings.Add(new ExpressionMapping
        {
            expressionKey = "NewExpression",
            animationName = "animation",
            enableShake = false
        });
    }

    /// <summary>
    /// 모든 매핑 출력 (디버그용)
    /// </summary>
    [ContextMenu("Print All Mappings")]
    private void PrintAllMappings()
    {
        Debug.Log($"===== {characterName} Expression Mappings =====");
        foreach (var mapping in expressionMappings)
        {
            Debug.Log($"'{mapping.expressionKey}' → '{mapping.animationName}' (Shake: {mapping.enableShake})");
        }
        Debug.Log($"Total: {expressionMappings.Count} mappings");
    }
}