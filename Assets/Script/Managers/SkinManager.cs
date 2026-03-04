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
        List<string> haveSkinList = PlayerDataManager.instance.GetAllSkinSerialNumber();

        foreach (SkinData skin in allSkins)
        {
            foreach (string haveSkin in haveSkinList)
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

/// <summary>
    /// 플레이어 데이터에서 현재 스킨 ID를 동기화
    /// </summary>
    public void SaveCurrentSkinId()
    {
        string usePlayerSkinData = PlayerDataManager.instance.GetUseSkinSerialNumber();
        SkinData useSkinData = allSkins.FirstOrDefault(s => s.skinData.serialNumber == usePlayerSkinData);

        // [수정된 부분] 처음 실행 시 데이터가 비어있어 Null이 나오는 것을 방어!
        if (useSkinData != null)
        {
            currentSkinId = useSkinData.skinId;
        }
        else
        {
            // 만약 못 찾았다면, 무조건 0번(기본 스킨)으로 강제 초기화하여 에러 방지
            currentSkinId = allSkins[0].skinId;
            PlayerDataManager.instance.ChangeSkin(allSkins[0].skinData.serialNumber);
            Debug.LogWarning("[SkinManager] 초기 장착 데이터가 없어 기본 스킨으로 강제 세팅합니다.");
        }
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

        if (skin == null || skin.isLocked) return;

        // 1. 매니저에 저장 명령 (껐다 켰을 때를 대비한 디스크 저장)
        PlayerDataManager.instance.ChangeSkin(skin.skinData.serialNumber);
        currentSkinId = skinId;

        // ==========================================
        // 2. [최종 해결 로직] 빙빙 돌지 않고 데이터를 다이렉트로 꽂아줍니다!
        // ==========================================
        // true를 넣으면 팝업창 뒤에 가려져서 비활성화(Inactive)된 캐릭터까지 모조리 찾습니다.
        SkinController[] allControllers = FindObjectsOfType<SkinController>(true); 
        
        foreach(var controller in allControllers)
        {
            // GetSkinData()를 부르게 하지 말고, 우리가 찾은 새 스킨 데이터를 직접 넘겨버립니다.
            controller.SpineChange(skin.skinData); 
        }

        Debug.Log($"[SkinManager] 다이렉트 스킨 갱신 완료: {skin.skinName}");
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
            PlayerDataManager.instance.ChangeSkin(useData.serialNumber);
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
}