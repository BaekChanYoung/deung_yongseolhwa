using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class IllustrationData
{
    [Tooltip("일러스트 고유 ID")]
    public string illustrationId;

    [Tooltip("일러스트 이름")]
    public string illustrationName;

    [Tooltip("썸네일 (Grid에 표시)")]
    public Sprite thumbnailSprite;

    [Tooltip("전체 일러스트 이미지")]
    public Sprite illustrationSprite;

    [Tooltip("잠금 해제 여부")]
    public bool isUnlocked = false;

    [Tooltip("잠금 해제 비용 (필요한 재화)")]
    public int unlockCost = 100;

    [Tooltip("설명")]
    public string description;
}

public class IllustrationManager : MonoBehaviour
{
    public static IllustrationManager Instance { get; private set; }

    [Header("Illustration Database")]
    [Tooltip("모든 일러스트 목록")]
    public List<IllustrationData> allIllustrations = new List<IllustrationData>();

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

        //LoadPlayerPrefs();
    }

    void Start()
    {
        // 현제 보유중인 스킨 잠금해제
        List<string> haveIllustrationList = PlayerDataManager.instance.GetAllIllustrationSerialNumber();
        
        foreach (IllustrationData Illustration in allIllustrations)
        {
            Illustration.isUnlocked = false;
        }

        foreach (IllustrationData Illustration in allIllustrations)
        {
            foreach (string haveIllustration in haveIllustrationList)
            {
                if(Illustration.illustrationId == haveIllustration)
                    Illustration.isUnlocked = true;
            }
        }
    }

    /// <summary>
    /// 모든 일러스트 가져오기
    /// </summary>
    public List<IllustrationData> GetAllIllustrations()
    {
        return allIllustrations;
    }

    /// <summary>
    /// 일러스트 해금 가능 여부 확인
    /// </summary>
    public bool CanUnlock(IllustrationData illustrationData)
    {
        return PlayerDataManager.instance.PullCoin() >= illustrationData.unlockCost && !illustrationData.isUnlocked;
    }

    /// <summary>
    /// 일러스트 잠금 해제
    /// </summary>
    public void UnlockIllustration(string illustrationId)
    {
        IllustrationData illustration = allIllustrations.FirstOrDefault(i => i.illustrationId == illustrationId);

        if (illustration == null)
        {
            Debug.LogError($"[IllustrationManager] Illustration not found: {illustrationId}");
            return;
        }

        if (illustration.isUnlocked)
        {
            Debug.LogWarning($"[IllustrationManager] Already unlocked: {illustrationId}");
            return;
        }

        if (PlayerDataManager.instance.PullCoin() < illustration.unlockCost)
        {
            Debug.LogWarning($"[IllustrationManager] Not enough currency! Need: {illustration.unlockCost}, Have: {PlayerDataManager.instance.PullCoin()}");
            return;
        }

        // 재화 차감
        PlayerDataManager.instance.TakeCoin(-illustration.unlockCost);

        // 잠금 해제
        illustration.isUnlocked = true;

        // 일러스트 추가
        PlayerDataManager.instance.AddIllustration(illustrationId);

        Debug.Log($"[IllustrationManager] Unlocked: {illustration.illustrationName}, Remaining currency: {PlayerDataManager.instance.PullCoin()}");
    }

    /// <summary>
    /// 재화 추가 (게임에서 획득)
    /// </summary>
    // public void AddCurrency(int amount)
    // {
    //     currentCurrency += amount;
    //     SavePlayerPrefs();

    //     Debug.Log($"[IllustrationManager] Currency added: +{amount}, Total: {currentCurrency}");
    // }

    /// <summary>
    /// 보유 재화 가져오기
    /// </summary>
    public int GetCurrentCurrency()
    {
        return PlayerDataManager.instance.PullCoin();
    }

    /// <summary>
    /// PlayerPrefs 저장
    /// </summary>


    /// <summary>
    /// PlayerPrefs 로드
    /// </summary>

}