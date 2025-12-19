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

    [Header("Character Info")]
    [SerializeField] private string characterName;

    [Tooltip("이 스탠드가 그룹 캐릭터인지 여부")]
    [SerializeField] private bool isGroupCharacter = false;

    [Header("Single Character (기본)")]
    [SerializeField] private SkeletonAnimation skeletonAnimation;

    [Header("Group Characters (여러 캐릭터 동시 등장)")]
    [Tooltip("그룹으로 등장할 캐릭터들 (순서대로 배치)")]
    [SerializeField] private SkeletonAnimation[] groupSkeletons;

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

    // 그룹 캐릭터용 원래 위치들
    private Vector3[] groupOriginalPositions;
    private Color[] groupOriginalColors;

    // 빠른 검색을 위한 Dictionary
    private Dictionary<string, ExpressionMapping> expressionDict;

    private void Awake()
    {
        characterTransform = transform;
        originalPosition = characterTransform.position;

        // 단일 캐릭터 초기화
        if (!isGroupCharacter)
        {
            if (skeletonAnimation == null)
            {
                skeletonAnimation = GetComponentInChildren<SkeletonAnimation>();
            }

            if (skeletonAnimation != null)
            {
                originalColor = skeletonAnimation.Skeleton.GetColor();
            }
        }
        // 그룹 캐릭터 초기화
        else
        {
            if (groupSkeletons != null && groupSkeletons.Length > 0)
            {
                groupOriginalPositions = new Vector3[groupSkeletons.Length];
                groupOriginalColors = new Color[groupSkeletons.Length];

                for (int i = 0; i < groupSkeletons.Length; i++)
                {
                    if (groupSkeletons[i] != null)
                    {
                        groupOriginalPositions[i] = groupSkeletons[i].transform.position;
                        groupOriginalColors[i] = groupSkeletons[i].Skeleton.GetColor();
                    }
                }

                Debug.Log($"[SpineCharacterStand] Group character '{characterName}' initialized with {groupSkeletons.Length} members");
            }
            else
            {
                Debug.LogWarning($"[SpineCharacterStand] Group character '{characterName}' has no group skeletons assigned!");
            }
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

        if (!isGroupCharacter)
        {
            StartCoroutine(FadeIn(position));
        }
        else
        {
            StartCoroutine(GroupFadeIn(position));
        }
    }

    /// <summary>
    /// 캐릭터 숨김
    /// </summary>
    public void Hide()
    {
        // 단일 캐릭터
        if (!isGroupCharacter)
        {
            if (skeletonAnimation != null)
            {
                skeletonAnimation.Skeleton.SetColor(new Color(originalColor.r, originalColor.g, originalColor.b, 0f));
            }
        }
        // 그룹 캐릭터
        else
        {
            if (groupSkeletons != null)
            {
                for (int i = 0; i < groupSkeletons.Length; i++)
                {
                    if (groupSkeletons[i] != null && i < groupOriginalColors.Length)
                    {
                        Color c = groupOriginalColors[i];
                        groupSkeletons[i].Skeleton.SetColor(new Color(c.r, c.g, c.b, 0f));
                    }
                }
            }
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
        Debug.Log($"[SpineCharacterStand] {characterName} received expression: '{expression}'");

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

        ExpressionMapping mapping = GetExpressionMapping(expression);

        if (mapping != null)
        {
            Debug.Log($"[SpineCharacterStand] Mapped '{expression}' → Animation: '{mapping.animationName}' (Shake: {mapping.enableShake})");

            // 단일 캐릭터
            if (!isGroupCharacter)
            {
                PlaySpineAnimation(skeletonAnimation, mapping.animationName);
            }
            // 그룹 캐릭터 - 모든 캐릭터에 동일 애니메이션 적용
            else
            {
                if (groupSkeletons != null)
                {
                    foreach (var skeleton in groupSkeletons)
                    {
                        if (skeleton != null)
                        {
                            PlaySpineAnimation(skeleton, mapping.animationName);
                        }
                    }
                }
            }

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

            if (!isGroupCharacter)
            {
                PlaySpineAnimation(skeletonAnimation, defaultAnimation);
            }
            else
            {
                if (groupSkeletons != null)
                {
                    foreach (var skeleton in groupSkeletons)
                    {
                        if (skeleton != null)
                        {
                            PlaySpineAnimation(skeleton, defaultAnimation);
                        }
                    }
                }
            }
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
    private void PlaySpineAnimation(SkeletonAnimation skeleton, string animationName, float mixDuration = 0.2f)
    {
        if (skeleton == null)
        {
            Debug.LogError("[SpineCharacterStand] SkeletonAnimation is null!");
            return;
        }

        Debug.Log($"[SpineCharacterStand] Playing animation: '{animationName}' on {skeleton.name}");

        var trackEntry = skeleton.state.SetAnimation(0, animationName, true);

        if (trackEntry != null)
        {
            trackEntry.MixDuration = mixDuration;
        }
        else
        {
            Debug.LogWarning($"[SpineCharacterStand] Animation '{animationName}' not found! Using default: {defaultAnimation}");
            skeleton.state.SetAnimation(0, defaultAnimation, true);
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
    /// 단일 캐릭터 페이드 인
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
            case "center":
                startPos = new Vector3(originalPosition.x, originalPosition.y - moveDistance, originalPosition.z);
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
    /// 그룹 캐릭터 페이드 인
    /// </summary>
    private IEnumerator GroupFadeIn(string position)
    {
        if (groupSkeletons == null || groupSkeletons.Length == 0)
        {
            Debug.LogError("[SpineCharacterStand] No group skeletons assigned!");
            yield break;
        }

        // 각 캐릭터의 시작 위치 계산
        Vector3[] startPositions = new Vector3[groupSkeletons.Length];

        for (int i = 0; i < groupSkeletons.Length; i++)
        {
            if (groupSkeletons[i] != null && i < groupOriginalPositions.Length)
            {
                Vector3 originalPos = groupOriginalPositions[i];

                switch (position.ToLower())
                {
                    case "left":
                    case "left_group":
                        startPositions[i] = new Vector3(originalPos.x - moveDistance, originalPos.y, originalPos.z);
                        break;
                    case "right":
                    case "right_group":
                        startPositions[i] = new Vector3(originalPos.x + moveDistance, originalPos.y, originalPos.z);
                        break;
                    case "center":
                        startPositions[i] = new Vector3(originalPos.x, originalPos.y - moveDistance, originalPos.z);
                        break;
                    default:
                        startPositions[i] = originalPos;
                        break;
                }

                groupSkeletons[i].transform.position = startPositions[i];

                // 초기 알파값 0
                Color c = groupOriginalColors[i];
                groupSkeletons[i].Skeleton.SetColor(new Color(c.r, c.g, c.b, 0f));
            }
        }

        float elapsed = 0f;

        // 모든 캐릭터 동시에 페이드 인
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeInDuration;

            for (int i = 0; i < groupSkeletons.Length; i++)
            {
                if (groupSkeletons[i] != null && i < groupOriginalPositions.Length)
                {
                    // 위치 이동
                    groupSkeletons[i].transform.position = Vector3.Lerp(startPositions[i], groupOriginalPositions[i], t);

                    // 알파값 증가
                    Color c = groupOriginalColors[i];
                    Color newColor = new Color(c.r, c.g, c.b, Mathf.Lerp(0f, 1f, t));
                    groupSkeletons[i].Skeleton.SetColor(newColor);
                }
            }

            yield return null;
        }

        // 최종 상태
        for (int i = 0; i < groupSkeletons.Length; i++)
        {
            if (groupSkeletons[i] != null && i < groupOriginalPositions.Length)
            {
                groupSkeletons[i].transform.position = groupOriginalPositions[i];
                groupSkeletons[i].Skeleton.SetColor(groupOriginalColors[i]);
            }
        }
    }

    /// <summary>
    /// 캐릭터 이름 반환
    /// </summary>
    public string GetCharacterName()
    {
        return characterName;
    }

    /// <summary>
    /// 그룹 캐릭터 여부 확인
    /// </summary>
    public bool IsGroupCharacter()
    {
        return isGroupCharacter;
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