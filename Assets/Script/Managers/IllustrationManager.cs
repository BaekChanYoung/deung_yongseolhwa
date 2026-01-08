using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class IllustrationData
{
    [Tooltip("�Ϸ���Ʈ ���� ID")]
    public string illustrationId;

    [Tooltip("�Ϸ���Ʈ �̸�")]
    public string illustrationName;

    [Tooltip("����� (Grid�� ǥ��)")]
    public Sprite thumbnailSprite;

    [Tooltip("��ü �Ϸ���Ʈ �̹���")]
    public Sprite illustrationSprite;

    [Tooltip("�ر� ����")]
    public bool isUnlocked = false;

    [Tooltip("�ر� ���� (�ʿ��� ��ȭ)")]
    public int unlockCost = 100;

    [Tooltip("����")]
    public string description;
}

public class IllustrationManager : MonoBehaviour
{
    public static IllustrationManager Instance { get; private set; }

    [Header("Illustration Database")]
    [Tooltip("��� �Ϸ���Ʈ ���")]
    public List<IllustrationData> allIllustrations = new List<IllustrationData>();

    [Header("Currency")]
    [Tooltip("���� ��ȭ (�Ϸ���Ʈ �رݿ�)")]
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
    /// ��� �Ϸ���Ʈ ��������
    /// </summary>
    public List<IllustrationData> GetAllIllustrations()
    {
        return allIllustrations;
    }

    /// <summary>
    /// �Ϸ���Ʈ �ر� ���� ����
    /// </summary>
    public bool CanUnlock(IllustrationData illustrationData)
    {
        return currentCurrency >= illustrationData.unlockCost && !illustrationData.isUnlocked;
    }

    /// <summary>
    /// �Ϸ���Ʈ �ر�
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

        // ��ȭ ����
        currentCurrency -= illustration.unlockCost;

        // �ر�
        illustration.isUnlocked = true;

        SavePlayerPrefs();

        Debug.Log($"[IllustrationManager] Unlocked: {illustration.illustrationName}, Remaining currency: {currentCurrency}");
    }

    /// <summary>
    /// ��ȭ �߰� (���ӿ��� ȹ��)
    /// </summary>
    public void AddCurrency(int amount)
    {
        currentCurrency += amount;
        SavePlayerPrefs();

        Debug.Log($"[IllustrationManager] Currency added: +{amount}, Total: {currentCurrency}");
    }

    /// <summary>
    /// ���� ��ȭ ��������
    /// </summary>
    public int GetCurrentCurrency()
    {
        return currentCurrency;
    }

    /// <summary>
    /// PlayerPrefs ����
    /// </summary>
    void SavePlayerPrefs()
    {
        PlayerPrefs.SetInt("IllustrationCurrency", currentCurrency);

        // �ر� ���� ����
        foreach (var illustration in allIllustrations)
        {
            PlayerPrefs.SetInt($"Illustration_{illustration.illustrationId}_Unlocked", illustration.isUnlocked ? 1 : 0);
        }

        PlayerPrefs.Save();
    }

    /// <summary>
    /// PlayerPrefs �ε�
    /// </summary>
    void LoadPlayerPrefs()
    {
        currentCurrency = PlayerPrefs.GetInt("IllustrationCurrency", 0);

        // �ر� ���� �ε�
        foreach (var illustration in allIllustrations)
        {
            illustration.isUnlocked = PlayerPrefs.GetInt($"Illustration_{illustration.illustrationId}_Unlocked", illustration.isUnlocked ? 1 : 0) == 1;
        }
    }
}