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

    [Tooltip("해금 여부")]
    public bool isUnlocked = false;

    [Tooltip("해금 조건 (필요한 재화)")]
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

    [Header("Currency")]
    [Tooltip("현재 재화 (일러스트 해금용)")]
    public int currentCurrency = 500;

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

        LoadPlayerPrefs();
    }

    /// <summary>
    /// 모든 일러스트 가져오기
    /// </summary>
    public List<IllustrationData> GetAllIllustrations()
    {
        return allIllustrations;
    }

    /// <summary>
    /// 일러스트 해금 가능 여부
    /// </summary>
    public bool CanUnlock(IllustrationData illustrationData)
    {
        return currentCurrency >= illustrationData.unlockCost && !illustrationData.isUnlocked;
    }

    /// <summary>
    /// 일러스트 해금
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

        if (currentCurrency < illustration.unlockCost)
        {
            Debug.LogWarning($"[IllustrationManager] Not enough currency! Need: {illustration.unlockCost}, Have: {currentCurrency}");
            return;
        }

        // 재화 차감
        currentCurrency -= illustration.unlockCost;

        // 해금
        illustration.isUnlocked = true;

        SavePlayerPrefs();

        Debug.Log($"[IllustrationManager] Unlocked: {illustration.illustrationName}, Remaining currency: {currentCurrency}");
    }

    /// <summary>
    /// 재화 추가 (게임에서 획득)
    /// </summary>
    public void AddCurrency(int amount)
    {
        currentCurrency += amount;
        SavePlayerPrefs();

        Debug.Log($"[IllustrationManager] Currency added: +{amount}, Total: {currentCurrency}");
    }

    /// <summary>
    /// 현재 재화 가져오기
    /// </summary>
    public int GetCurrentCurrency()
    {
        return currentCurrency;
    }

    /// <summary>
    /// PlayerPrefs 저장
    /// </summary>
    void SavePlayerPrefs()
    {
        PlayerPrefs.SetInt("IllustrationCurrency", currentCurrency);

        // 해금 상태 저장
        foreach (var illustration in allIllustrations)
        {
            PlayerPrefs.SetInt($"Illustration_{illustration.illustrationId}_Unlocked", illustration.isUnlocked ? 1 : 0);
        }

        PlayerPrefs.Save();
    }

    /// <summary>
    /// PlayerPrefs 로드
    /// </summary>
    void LoadPlayerPrefs()
    {
        currentCurrency = PlayerPrefs.GetInt("IllustrationCurrency", 0);

        // 해금 상태 로드
        foreach (var illustration in allIllustrations)
        {
            illustration.isUnlocked = PlayerPrefs.GetInt($"Illustration_{illustration.illustrationId}_Unlocked", illustration.isUnlocked ? 1 : 0) == 1;
        }
    }
}