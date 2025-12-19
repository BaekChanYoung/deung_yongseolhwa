using Spine.Unity;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class SkinData
{
    [Tooltip("스킨 고유 ID")]
    public string skinId;

    [Tooltip("스킨 이름")]
    public string skinName;

    [Tooltip("스킨 썸네일 (Grid에 표시)")]
    public Sprite thumbnailSprite;

    [Tooltip("Spine SkeletonDataAsset")]
    public SkeletonDataAsset spineSkeletonData;

    [Tooltip("잠금 여부")]
    public bool isLocked = false;

    [Tooltip("해금 비용 (재화)")]
    public int unlockCost = 0;

    [Tooltip("설명")]
    public string description;

    // ========== 애니메이션 설정 추가 ==========
    [Header("Animation Settings")]
    [Tooltip("스킨 적용 시 재생할 기본 애니메이션 (예: idle)")]
    public string defaultAnimationName = "idle";

    [Tooltip("애니메이션 루프 여부")]
    public bool loopAnimation = true;
    // ==========================================
}

public class SkinManager : MonoBehaviour
{
    public static SkinManager Instance { get; private set; }

    [Header("Skin Database")]
    [Tooltip("모든 스킨 목록")]
    public List<SkinData> allSkins = new List<SkinData>();

    [Header("Current Skin")]
    [Tooltip("현재 선택된 스킨 ID")]
    public string currentSkinId = "default";

    [Header("Currency")]
    [Tooltip("현재 스킨 재화 (코인 등)")]
    public int currentSkinCurrency = 100;

    [Header("Player Refs")]
    [Tooltip("Start Scene의 Player_Stand")]
    public GameObject playerStandInStart;

    [Tooltip("Prototype Scene의 Player")]
    public GameObject playerInPrototype;

    // ========== 기본 애니메이션 설정 ==========
    [Header("Default Animation (Fallback)")]
    [Tooltip("SkinData에 애니메이션이 없을 때 사용할 기본 애니메이션")]
    public string fallbackAnimationName = "idle";

    [Tooltip("기본 애니메이션 루프 여부")]
    public bool fallbackLoopAnimation = true;
    // ==========================================

    // ========== PlayerPrefs 키 상수 ==========
    private const string CURRENT_SKIN_ID_KEY = "CurrentSkinId";
    private const string SKIN_CURRENCY_KEY = "SkinCurrency";
    private const string SKIN_LOCKED_PREFIX = "Skin_";
    private const string SKIN_LOCKED_SUFFIX = "_Locked";
    // ==========================================

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 로드 후 즉시 적용
        LoadPlayerPrefs();

        // 로드된 스킨을 캐릭터에 적용
        ApplySkinToCharacters();

        Debug.Log($"[SkinManager] 초기화 완료 - 현재 스킨: {currentSkinId}, 재화: {currentSkinCurrency}");
    }

    /// <summary>
    /// 모든 스킨 가져오기
    /// </summary>
    public List<SkinData> GetAllSkins()
    {
        return allSkins;
    }

    /// <summary>
    /// 현재 스킨 ID 가져오기
    /// </summary>
    public string GetCurrentSkinId()
    {
        return currentSkinId;
    }

    /// <summary>
    /// 현재 스킨 데이터 가져오기
    /// </summary>
    public SkinData GetCurrentSkin()
    {
        return allSkins.FirstOrDefault(s => s.skinId == currentSkinId);
    }

    /// <summary>
    /// 현재 재화 가져오기
    /// </summary>
    public int GetCurrentCurrency()
    {
        return currentSkinCurrency;
    }

    /// <summary>
    /// 재화 추가 (게임에서 획득)
    /// </summary>
    public void AddCurrency(int amount)
    {
        currentSkinCurrency += amount;
        SavePlayerPrefs();

        Debug.Log($"[SkinManager] Currency added: +{amount}, Total: {currentSkinCurrency}");
    }

    /// <summary>
    /// 스킨 해금 가능 여부
    /// </summary>
    public bool CanUnlock(SkinData skinData)
    {
        return currentSkinCurrency >= skinData.unlockCost && skinData.isLocked;
    }

    /// <summary>
    /// 스킨 변경
    /// </summary>
    public void SetCurrentSkin(string skinId)
    {
        SkinData skin = allSkins.FirstOrDefault(s => s.skinId == skinId);

        if (skin == null)
        {
            Debug.LogError($"[SkinManager] Skin not found: {skinId}");
            return;
        }

        if (skin.isLocked)
        {
            Debug.LogWarning($"[SkinManager] Skin is locked: {skinId}");
            return;
        }

        currentSkinId = skinId;

        // 저장!
        SavePlayerPrefs();

        // 씬의 캐릭터 업데이트
        ApplySkinToCharacters();

        Debug.Log($"[SkinManager] Skin changed to: {skin.skinName}");
    }

    /// <summary>
    /// 스킨 해금
    /// </summary>
    public void UnlockSkin(string skinId)
    {
        SkinData skin = allSkins.FirstOrDefault(s => s.skinId == skinId);

        if (skin == null)
        {
            Debug.LogError($"[SkinManager] Skin not found: {skinId}");
            return;
        }

        if (!skin.isLocked)
        {
            Debug.LogWarning($"[SkinManager] Skin already unlocked: {skinId}");
            return;
        }

        if (currentSkinCurrency < skin.unlockCost)
        {
            Debug.LogWarning($"[SkinManager] Not enough currency! Need: {skin.unlockCost}, Have: {currentSkinCurrency}");
            return;
        }

        // 재화 차감
        currentSkinCurrency -= skin.unlockCost;

        // 해금
        skin.isLocked = false;

        // 저장!
        SavePlayerPrefs();

        Debug.Log($"[SkinManager] Skin unlocked: {skin.skinName}, Remaining currency: {currentSkinCurrency}");
    }

    /// <summary>
    /// 캐릭터에 스킨 적용
    /// </summary>
    void ApplySkinToCharacters()
    {
        SkinData currentSkin = GetCurrentSkin();
        if (currentSkin == null)
        {
            Debug.LogWarning($"[SkinManager] Current skin not found: {currentSkinId}");
            return;
        }

        // Start Scene의 Player_Stand
        if (playerStandInStart != null)
        {
            ApplySkinToCharacter(playerStandInStart, currentSkin);
        }

        // Prototype Scene의 Player
        if (playerInPrototype != null)
        {
            ApplySkinToCharacter(playerInPrototype, currentSkin);
        }
    }

    /// <summary>
    /// 특정 캐릭터에 스킨 적용
    /// </summary>
    void ApplySkinToCharacter(GameObject character, SkinData skinData)
    {
        if (character == null || skinData == null) return;

        var skeletonAnim = character.GetComponentInChildren<Spine.Unity.SkeletonAnimation>();
        if (skeletonAnim != null && skinData.spineSkeletonData != null)
        {
            // 1. SkeletonDataAsset 변경
            skeletonAnim.skeletonDataAsset = skinData.spineSkeletonData;
            skeletonAnim.Initialize(true);

            // 2. 애니메이션 적용
            ApplyAnimation(skeletonAnim, skinData);

            Debug.Log($"[SkinManager] Applied skin '{skinData.skinName}' to {character.name}");
        }
        else
        {
            Debug.LogWarning($"[SkinManager] Cannot apply skin to {character.name} - SkeletonAnimation or SkeletonData missing");
        }
    }

    /// <summary>
    /// 애니메이션 적용 (새로 추가)
    /// </summary>
    void ApplyAnimation(SkeletonAnimation skeletonAnim, SkinData skinData)
    {
        if (skeletonAnim == null || skeletonAnim.Skeleton == null) return;

        // 애니메이션 이름 결정
        string animName = !string.IsNullOrEmpty(skinData.defaultAnimationName)
            ? skinData.defaultAnimationName
            : fallbackAnimationName;

        bool loop = skinData.loopAnimation || fallbackLoopAnimation;

        try
        {
            // 애니메이션 존재 여부 확인
            var animation = skeletonAnim.Skeleton.Data.FindAnimation(animName);

            if (animation != null)
            {
                // 애니메이션 재생!
                skeletonAnim.AnimationState.SetAnimation(0, animName, loop);

                Debug.Log($"[SkinManager] Animation '{animName}' applied (loop: {loop})");
            }
            else
            {
                Debug.LogWarning($"[SkinManager] Animation '{animName}' not found in skeleton. Available animations: {GetAvailableAnimations(skeletonAnim)}");

                // Fallback: 첫 번째 애니메이션 재생
                if (skeletonAnim.Skeleton.Data.Animations.Count > 0)
                {
                    var firstAnim = skeletonAnim.Skeleton.Data.Animations.Items[0];
                    skeletonAnim.AnimationState.SetAnimation(0, firstAnim.Name, loop);
                    Debug.Log($"[SkinManager] Using fallback animation: {firstAnim.Name}");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SkinManager] Failed to apply animation '{animName}': {e.Message}");
        }
    }

    /// <summary>
    /// 사용 가능한 애니메이션 목록 가져오기 (디버깅용)
    /// </summary>
    string GetAvailableAnimations(SkeletonAnimation skeletonAnim)
    {
        if (skeletonAnim == null || skeletonAnim.Skeleton == null) return "None";

        var animations = skeletonAnim.Skeleton.Data.Animations;
        if (animations.Count == 0) return "None";

        var names = new List<string>();
        foreach (var anim in animations)
        {
            names.Add(anim.Name);
        }

        return string.Join(", ", names);
    }

    /// <summary>
    /// PlayerPrefs 저장
    /// </summary>
    void SavePlayerPrefs()
    {
        // 현재 스킨 저장
        PlayerPrefs.SetString(CURRENT_SKIN_ID_KEY, currentSkinId);

        // 재화 저장
        PlayerPrefs.SetInt(SKIN_CURRENCY_KEY, currentSkinCurrency);

        // 해금 상태 저장
        foreach (var skin in allSkins)
        {
            string key = $"{SKIN_LOCKED_PREFIX}{skin.skinId}{SKIN_LOCKED_SUFFIX}";
            PlayerPrefs.SetInt(key, skin.isLocked ? 1 : 0);
        }

        PlayerPrefs.Save();

        Debug.Log($"[SkinManager] Saved - Skin: {currentSkinId}, Currency: {currentSkinCurrency}");
    }

    /// <summary>
    /// PlayerPrefs 로드
    /// </summary>
    void LoadPlayerPrefs()
    {
        // 현재 스킨 로드
        currentSkinId = PlayerPrefs.GetString(CURRENT_SKIN_ID_KEY, "default");

        // 재화 로드
        currentSkinCurrency = PlayerPrefs.GetInt(SKIN_CURRENCY_KEY, 0);

        // 해금 상태 로드
        foreach (var skin in allSkins)
        {
            string key = $"{SKIN_LOCKED_PREFIX}{skin.skinId}{SKIN_LOCKED_SUFFIX}";

            // 키가 없으면 Inspector 기본값 사용
            if (PlayerPrefs.HasKey(key))
            {
                skin.isLocked = PlayerPrefs.GetInt(key) == 1;
            }
            // else: Inspector의 기본 설정 유지
        }

        Debug.Log($"[SkinManager] Loaded - Skin: {currentSkinId}, Currency: {currentSkinCurrency}");
    }

    /// <summary>
    /// 씬 전환 시 호출 (캐릭터 레퍼런스 갱신용)
    /// </summary>
    public void RefreshCharacterReferences()
    {
        // 씬이 바뀌면 레퍼런스가 null이 될 수 있으므로 재검색
        if (playerStandInStart == null)
        {
            playerStandInStart = GameObject.Find("Player_Stand");
        }

        if (playerInPrototype == null)
        {
            playerInPrototype = GameObject.Find("Player");
        }

        // 스킨 재적용
        ApplySkinToCharacters();
    }

    /// <summary>
    /// 디버깅: 현재 상태 출력
    /// </summary>
    [ContextMenu("Debug: Print Status")]
    public void DebugPrintStatus()
    {
        Debug.Log("========================================");
        Debug.Log("[SkinManager] Current Status:");
        Debug.Log($"  - Current Skin ID: {currentSkinId}");
        Debug.Log($"  - Current Currency: {currentSkinCurrency}");
        Debug.Log("----------------------------------------");
        Debug.Log("[All Skins]");
        foreach (var skin in allSkins)
        {
            Debug.Log($"  - {skin.skinId} ({skin.skinName}): {(skin.isLocked ? "Locked" : "Unlocked")}");
        }
        Debug.Log("========================================");
    }

    /// <summary>
    /// 테스트: PlayerPrefs 초기화
    /// </summary>
    [ContextMenu("Debug: Reset PlayerPrefs")]
    public void DebugResetPlayerPrefs()
    {
        PlayerPrefs.DeleteKey(CURRENT_SKIN_ID_KEY);
        PlayerPrefs.DeleteKey(SKIN_CURRENCY_KEY);

        foreach (var skin in allSkins)
        {
            string key = $"{SKIN_LOCKED_PREFIX}{skin.skinId}{SKIN_LOCKED_SUFFIX}";
            PlayerPrefs.DeleteKey(key);
        }

        PlayerPrefs.Save();

        Debug.Log("[SkinManager] PlayerPrefs 초기화 완료! Unity 재시작 후 적용됩니다.");
    }

    /// <summary>
    /// 디버깅: 현재 캐릭터 애니메이션 목록 출력
    /// </summary>
    [ContextMenu("Debug: Print Available Animations")]
    public void DebugPrintAvailableAnimations()
    {
        Debug.Log("========================================");
        Debug.Log("[SkinManager] Available Animations:");

        if (playerStandInStart != null)
        {
            var skeletonAnim = playerStandInStart.GetComponentInChildren<SkeletonAnimation>();
            if (skeletonAnim != null)
            {
                Debug.Log($"Player_Stand: {GetAvailableAnimations(skeletonAnim)}");
            }
        }

        if (playerInPrototype != null)
        {
            var skeletonAnim = playerInPrototype.GetComponentInChildren<SkeletonAnimation>();
            if (skeletonAnim != null)
            {
                Debug.Log($"Player (Prototype): {GetAvailableAnimations(skeletonAnim)}");
            }
        }

        Debug.Log("========================================");
    }
}