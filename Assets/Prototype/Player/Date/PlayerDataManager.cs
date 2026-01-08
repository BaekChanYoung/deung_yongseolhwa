using System.Collections.Generic;
using System.IO;
using UnityEngine;


[System.Serializable]
[SerializeField]
public struct PlayerData
{
    [ReadOnly]
    public int MaxScore;

    [ReadOnly]
    public int coin;

    public PlayerSkinData defaultSkin;

    [ReadOnly]
    public List<PlayerSkinData> skinList;

    [ReadOnly]
    public int useSkinNumber;
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
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            //Debug.LogWarning("씬에 두개 이상의 플레이어 데이터 매니저가 존재합니다!");
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        playerdata = new PlayerData();

        LoadJson();

        SaveJson();

        Debug.Log("playerdata.score : " + playerdata.MaxScore);
        Debug.Log("playerdata.coin : " + playerdata.coin);
    }

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
        json = JsonUtility.ToJson(playerdata);
        File.WriteAllText(Application.persistentDataPath + "/PlayerData.json", json);
    }

    void LoadJson()
    {
        // File.Exists -> 파일 경로에 이 파일이 존제하는지 확인해줌
        if(!File.Exists(Application.persistentDataPath + "/PlayerData.json"))
        {
            CreatJsonFile();
        }

        json = File.ReadAllText(Application.persistentDataPath + "/PlayerData.json");
        
        if(json == null)
        {
            Debug.Log("파일이 없음, 생성시작");
            CreatJsonFile();
        }
        playerdata = JsonUtility.FromJson<PlayerData>(json);
    }

    void CreatJsonFile()
    {
        playerdata.MaxScore = 0;
        playerdata.coin = 100;
        playerdata.skinList = new List<PlayerSkinData>() {playerdata.defaultSkin};
        playerdata.useSkinNumber = 0;
        SaveJson();
    }

    public void TakeCoin(int addCoin = 0)
    {
        Debug.Log("코인 회득");
        playerdata.coin += addCoin;
    }

    public int PullMaxScore()
    {
        return playerdata.MaxScore;
    }

    public int PullCoin()
    {
        return playerdata.coin;
    }

    [Button("Save PlayerData JSON")]
    void SaveJsonDate()
    {
        SaveJson();
    }

    [Button("Reset PlayerData JSON")]
    public void ResetJsonData()
    {
        string path = Application.persistentDataPath + "/PlayerData.json";
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("기존 PlayerData.json 삭제");
        }

        CreatJsonFile();
        LoadJson();
        
        Debug.Log("PlayerData JSON 초기화 완료");
    }

    [Button("Load PlayerData JSON")]
    void ReadJsonDate()
    {
        LoadJson();
    }

    // 스킨 추가하는 메서드
    public void AddSkin(PlayerSkinData newSkin)
    {
        playerdata.skinList.Add(newSkin);
        SaveJson();
    }   

    public void ChangeSkin(int useNumber)
    {
        playerdata.useSkinNumber = useNumber;
        IsChangeSkin = true;
        SaveJson();
    }

    public PlayerSkinData GetSkinData()
    {
        PlayerSkinData useData = null;
        foreach (PlayerSkinData skin in playerdata.skinList)
        {
            if(skin.serialNumber == playerdata.useSkinNumber)
                useData = skin;
        }

        if(useData == null)
        {
            useData = playerdata.skinList[0];
            playerdata.useSkinNumber = 0;
        }
        return useData;
    }
    public List<PlayerSkinData> GetAllSkinData()
    {
        return playerdata.skinList;
    }
}