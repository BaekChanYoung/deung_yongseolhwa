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
    public PlayerSkinData skinData;

    [Tooltip("잠금 여부")]
    public bool isLocked = false;

    [Tooltip("해금 비용 (재화)")]
    public int unlockCost = 0;

    [Tooltip("설명")]
    public string description;
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
    }

    /// <summary>
    /// 모든 스킨 가져오기
    /// </summary>
    /// 

    void Start()
    {
        // 현제 보유중인 스킨 잠금해제
        List<int> haveSkinList = PlayerDataManager.instance.GetAllSkinSerialNumber();

        foreach (SkinData skin in allSkins)
        {
            foreach (int haveSkin in haveSkinList)
            {
                if(skin.skinData.serialNumber == haveSkin)
                    skin.isLocked = false;
            }
        }

        SaveCurrentSkinId();

        Debug.Log($"[SkinManager] 초기화 완료 - 현재 스킨: {currentSkinId}");
    }

    public List<SkinData> GetAllSkins()
    {
        return allSkins;
    }

    public void SaveCurrentSkinId()
    {
        int? usePlayerSkinData = PlayerDataManager.instance.GetUseSkinSerialNumber();

        SkinData useSkinData = allSkins.FirstOrDefault(s => s.skinData.serialNumber == usePlayerSkinData);

        currentSkinId = useSkinData.skinId;
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
    /// 스킨 해금 가능 여부
    /// </summary>
    public bool CanUnlock(SkinData skinData)
    {
        return PlayerDataManager.instance.PullCoin() >= skinData.unlockCost && skinData.isLocked;
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

        //currentSkinId = skinId;
        PlayerDataManager.instance.ChangeSkin(skin.skinData.serialNumber);
        

        // 저장!
        //SavePlayerPrefs();

        // 씬의 캐릭터 업데이트
        //ApplySkinToCharacters();

        SaveCurrentSkinId();

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

        if (PlayerDataManager.instance.PullCoin() < skin.unlockCost)
        {
            Debug.LogWarning($"[SkinManager] Not enough currency! Need: {skin.unlockCost}, Have: {PlayerDataManager.instance.PullCoin()}");
            return;
        }

        // 재화 차감
        //currentSkinCurrency -= skin.unlockCost;
        PlayerDataManager.instance.TakeCoin(-skin.unlockCost);

        // 해금
        skin.isLocked = false;

        PlayerDataManager.instance.AddSkin(skin.skinData);

        Debug.Log($"[SkinManager] Skin unlocked: {skin.skinName}, Remaining currency: {PlayerDataManager.instance.PullCoin()}");
    }

    public PlayerSkinData GetSkinData()
    {
        PlayerSkinData useData = null;
        foreach (SkinData skin in allSkins)
        {
            if(skin.skinData.serialNumber == PlayerDataManager.instance.GetUseSkinSerialNumber())
                useData = skin.skinData;
        }

        if(useData == null)
        {
            useData = allSkins[0].skinData;
            PlayerDataManager.instance.ChangeSkin(0);
        }
        return useData;
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
        //ApplySkinToCharacters();
    }



    /// <summary>
    /// 디버깅: 현재 상태 출력
    /// </summary>
    // [ContextMenu("Debug: Print Status")]
    // public void DebugPrintStatus()
    // {
    //     Debug.Log("========================================");
    //     Debug.Log("[SkinManager] Current Status:");
    //     Debug.Log($"  - Current Skin ID: {currentSkinId}");
    //     Debug.Log($"  - Current Currency: {currentSkinCurrency}");
    //     Debug.Log("----------------------------------------");
    //     Debug.Log("[All Skins]");
    //     foreach (var skin in allSkins)
    //     {
    //         Debug.Log($"  - {skin.skinId} ({skin.skinName}): {(skin.isLocked ? "Locked" : "Unlocked")}");
    //     }
    //     Debug.Log("========================================");
    // }

    // /// <summary>
    // /// 테스트: PlayerPrefs 초기화
    // /// </summary>
    // [ContextMenu("Debug: Reset PlayerPrefs")]
    // public void DebugResetPlayerPrefs()
    // {
    //     PlayerPrefs.DeleteKey(CURRENT_SKIN_ID_KEY);
    //     PlayerPrefs.DeleteKey(SKIN_CURRENCY_KEY);

    //     foreach (var skin in allSkins)
    //     {
    //         string key = $"{SKIN_LOCKED_PREFIX}{skin.skinId}{SKIN_LOCKED_SUFFIX}";
    //         PlayerPrefs.DeleteKey(key);
    //     }

    //     PlayerPrefs.Save();

    //     Debug.Log("[SkinManager] PlayerPrefs 초기화 완료! Unity 재시작 후 적용됩니다.");
    // }

    // /// <summary>
    // /// 디버깅: 현재 캐릭터 애니메이션 목록 출력
    // /// </summary>
    // [ContextMenu("Debug: Print Available Animations")]
    // public void DebugPrintAvailableAnimations()
    // {
    //     Debug.Log("========================================");
    //     Debug.Log("[SkinManager] Available Animations:");

    //     if (playerStandInStart != null)
    //     {
    //         var skeletonAnim = playerStandInStart.GetComponentInChildren<SkeletonAnimation>();
    //         if (skeletonAnim != null)
    //         {
    //             Debug.Log($"Player_Stand: {GetAvailableAnimations(skeletonAnim)}");
    //         }
    //     }

    //     if (playerInPrototype != null)
    //     {
    //         var skeletonAnim = playerInPrototype.GetComponentInChildren<SkeletonAnimation>();
    //         if (skeletonAnim != null)
    //         {
    //             Debug.Log($"Player (Prototype): {GetAvailableAnimations(skeletonAnim)}");
    //         }
    //     }

    //     Debug.Log("========================================");
    //}
}