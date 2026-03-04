using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class PlayerData // [핵심 1] struct에서 class로 변경하여 데이터 유실 방지!
{
    [ReadOnly] public int MaxScore;
    [ReadOnly] public int coin;
    
    public PlayerSkinData defaultSkin;
<<<<<<< HEAD

    [ReadOnly]
    public List<int> skinList;

    [ReadOnly]
    public int useSkinNumber;
=======
    
    // 리스트가 null이 되지 않도록 기본값 할당
    [ReadOnly] public List<string> skinList = new List<string>(); 
    [ReadOnly] public List<string> IllustrationList = new List<string>();
    [ReadOnly] public string useSkinNumber;
>>>>>>> parent of 5f3c1ba (0.1.9)
}

public class PlayerDataManager : MonoBehaviour
{
    [ReadOnly]
    public static PlayerDataManager instance;

    [SerializeField]
    public PlayerData playerdata;

    [ReadOnly]
    public bool IsChangeSkin = false;

    [SerializeField][ReadOnly]
    string json;

    void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); return; }

        DontDestroyOnLoad(gameObject);

        // [핵심 2] playerdata = new PlayerData(); 삭제!
        // 인스펙터에서 넣은 defaultSkin 값이 날아가지 않도록 보호합니다.
        if (playerdata == null) playerdata = new PlayerData();

        LoadJson();
        SaveJson();
    }

    // ... (InputMaxScore 함수는 기존과 동일하게 유지) ...
    public void InputMaxScore(int score)
    {
        if(score > playerdata.MaxScore)
        {
            playerdata.MaxScore = score;
            Debug.Log("최고점수 달성 : " + playerdata.MaxScore);
        }
    }

    public void SaveJson()
    {
<<<<<<< HEAD
        json = JsonUtility.ToJson(playerdata);
=======
        json = JsonUtility.ToJson(playerdata, true);
>>>>>>> parent of 5f3c1ba (0.1.9)
        File.WriteAllText(Application.persistentDataPath + "/PlayerData.json", json);
    }

    void LoadJson()
    {
        string path = Application.persistentDataPath + "/PlayerData.json";
        if(!File.Exists(path))
        {
            CreatJsonFile();
        }

        json = File.ReadAllText(path);
        
        if(string.IsNullOrEmpty(json))
        {
            CreatJsonFile();
            json = File.ReadAllText(path);
        }
        
        // [핵심 3] FromJson 대신 FromJsonOverwrite 사용!
        // 기존 defaultSkin 연결은 유지하면서, 파일에 저장된 코인과 스킨 리스트만 쏙 덮어씌웁니다.
        JsonUtility.FromJsonOverwrite(json, playerdata);
    }

    void CreatJsonFile()
    {
        playerdata.MaxScore = 0;
<<<<<<< HEAD
        playerdata.coin = 100;
        playerdata.skinList = new List<int>() {0};
        playerdata.useSkinNumber = 0;
=======
        playerdata.coin = 1000;
        
        playerdata.skinList = new List<string>();
        // defaultSkin이 비어있지 않을 때만 안전하게 추가
        if (playerdata.defaultSkin != null)
        {
            playerdata.skinList.Add(playerdata.defaultSkin.serialNumber);
            playerdata.useSkinNumber = playerdata.defaultSkin.serialNumber;
        }
        
        playerdata.IllustrationList = new List<string>();
>>>>>>> parent of 5f3c1ba (0.1.9)
        SaveJson();
    }

    public void TakeCoin(int addCoin = 0)
    {
        playerdata.coin += addCoin;
        // [안전 추가] 재화가 변동될 때마다 즉시 디스크에 저장하여 엇갈림 방지
        SaveJson(); 
        Debug.Log("코인 변동 적용 및 저장 완료");
    }

    public int PullMaxScore() { return playerdata.MaxScore; }
    public int PullCoin() { return playerdata.coin; }

    [Button("Save PlayerData JSON")]
    void SaveJsonDate() { SaveJson(); }

    [Button("Reset PlayerData JSON")]
    public void ResetJsonData()
    {
        string path = Application.persistentDataPath + "/PlayerData.json";
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("기존 PlayerData.json 삭제");
        }

        playerdata.skinList.Clear();
        CreatJsonFile();
        LoadJson();
        
        Debug.Log("PlayerData JSON 초기화 완료");
    }

    [Button("Load PlayerData JSON")]
    void ReadJsonDate() { LoadJson(); }

    // 스킨 추가하는 메서드
    public void AddSkin(PlayerSkinData newSkin)
    {
        if (playerdata.skinList == null) playerdata.skinList = new List<string>();

        // 중복 획득 방지 로직 추가
        if (!playerdata.skinList.Contains(newSkin.serialNumber))
        {
            playerdata.skinList.Add(newSkin.serialNumber);
            SaveJson();
        }
    }   

    public void ChangeSkin(int useNumber)
    {
        playerdata.useSkinNumber = useNumber;
        IsChangeSkin = true;
        SaveJson();
    }

    public int? GetUseSkinSerialNumber()
    {
<<<<<<< HEAD
        int? useData = null;
        foreach (int skin in playerdata.skinList)
=======
        if (playerdata.IllustrationList == null) playerdata.IllustrationList = new List<string>();
        if (!playerdata.IllustrationList.Contains(serialNumber))
        {
            playerdata.IllustrationList.Add(serialNumber);
            SaveJson();
        }
    }

    public string GetUseSkinSerialNumber()
    {
        string useData = null;
        foreach (string skin in playerdata.skinList)
>>>>>>> parent of 5f3c1ba (0.1.9)
        {
            if(skin == playerdata.useSkinNumber)
                useData = skin;
        }

        if(useData == null && playerdata.skinList.Count > 0)
        {
            useData = playerdata.skinList[0];
            playerdata.useSkinNumber = 0;
        }
        return useData;
    }
<<<<<<< HEAD
    public List<int> GetAllSkinSerialNumber()
    {
        return playerdata.skinList;
    }
=======
    
    public List<string> GetAllSkinSerialNumber() { return playerdata.skinList; }
    public List<string> GetAllIllustrationSerialNumber() { return playerdata.IllustrationList; }
>>>>>>> parent of 5f3c1ba (0.1.9)
}