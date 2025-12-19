using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[SerializeField]
public struct PlayerDate
{
    public int MaxScore;
    public int coin;
}

public class PlayerDataManager : MonoBehaviour
{
    [ReadOnly]
    public static PlayerDataManager instance;

    [ReadOnly][SerializeField]
    public PlayerDate playerdate;
    [ReadOnly][SerializeField]
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

        playerdate = new PlayerDate();

        LoadJson();

        SaveJson();

        Debug.Log("playerdate.score : " + playerdate.MaxScore);
        Debug.Log("playerdate.coin : " + playerdate.coin);
    }

    public void InputMaxScore(int score)
    {
        if(score > playerdate.MaxScore)
        {
            playerdate.MaxScore = score;
            Debug.Log("최고점수 달성 : " + playerdate.MaxScore);
        }
    }

    public void SaveJson()
    {
        json = JsonUtility.ToJson(playerdate);
        File.WriteAllText(Application.persistentDataPath + "/PlayerDate.json", json);
    }

    void LoadJson()
    {
        // File.Exists -> 파일 경로에 이 파일이 존제하는지 확인해줌
        if(!File.Exists(Application.persistentDataPath + "/PlayerDate.json"))
        {
            CreatJsonFile();
        }

        json = File.ReadAllText(Application.persistentDataPath + "/PlayerDate.json");
        
        if(json == null)
        {
            Debug.Log("파일이 없음, 생성시작");
            CreatJsonFile();
        }
        playerdate = JsonUtility.FromJson<PlayerDate>(json);
    }

    void CreatJsonFile()
    {
        playerdate.MaxScore = 0;
        playerdate.coin = 0;
        SaveJson();
    }

    public void TakeCoin(int addCoin = 0)
    {
        Debug.Log("코인 회득");
        playerdate.coin += addCoin;
    }

    public int PullMaxScore()
    {
        return playerdate.MaxScore;
    }

    public int PullCoin()
    {
        return playerdate.coin;
    }

    [Button("Reset PlayerData JSON")]
    void ResetJsonData()
    {
        string path = Application.persistentDataPath + "/PlayerDate.json";
        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log("기존 PlayerDate.json 삭제");
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
}