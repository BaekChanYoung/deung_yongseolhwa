using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class AngleRange
{
    public float Max;
    public float Min;
}

[System.Serializable]
public struct HitSlowEffect
{
    [Tooltip("공격이 히트 시 정지효과를 주는 시간")]
    [SerializeField]
    public float HitStopTime;

    [Tooltip("공격이 히트 시 슬로우 효과를 주는 강도")]
    [SerializeField]
    public float HitSlowScale;


    [Tooltip("공격이 히트 시 슬로우 효과를 주는 시간")]
    public float HitSlowTime;
}


public class GameManager : MonoBehaviour
{
    [ReadOnly]
    public static GameManager instance;
    [SerializeField]
    [Tooltip("게임의 점수")]
    int score;

    [Header("Game Setting")]
    [Tooltip("공격 히트 후 효과수치")]
    [SerializeField]
    HitSlowEffect hitSlowEffect;

    Coroutine hitEffectRoutine;

    ////////////////////////////////////////
    /// 배경화면 관련
    ////////////////////////////////////////

    [Header("Background")]

    [Tooltip("배경 루프")]
    [SerializeField]
    GameObject[] BackgroundLayer;

    [Tooltip("배경이 내려가는 거리")]
    [SerializeField]
    float BackgroundDownDistance; // 배경 내려가는 거리

    [Tooltip("배경이 내려갈 시 걸리는 시간")]
    [SerializeField]
    float BackgroundDownDuration; // 1회 내려갈 시 걸리는 시간



    ////////////////////////////////////////
    /// 플레이어 관련
    ////////////////////////////////////////
    [Header("Player")]
    [SerializeField]
    GameObject Player;

    [Tooltip("공격하는 걸리는 시간")]
    [SerializeField]
    public float AttackDuration; //공격하는데 걸리는 시간

    [Tooltip("공격후 내려오는데 걸리는 시간\n(효과가 없을때 기준)")]
    [SerializeField]
    float PlayerDownDuration; // Player 떨어지는데 걸리는 시간

    [Tooltip("공격 이후 관성(미구현)")]
    [SerializeField]
    float reboundPower;

    // [Tooltip("")]
    // [ReadOnly]
    // [SerializeField]
    // Vector2 PlayerMovePos;


    // 플레이어가 죽을시 true가 되는 변수
    [HideInInspector]
    public bool isDead;
    

    ////////////////////////////////////////
    /// 적 관련
    ////////////////////////////////////////
    [Header("Enemy")]

    [Tooltip("적 프리펩")]
    [SerializeField]
    GameObject Enemy; // 적 프리펩

    [Tooltip("적 소환시 부모지정")]
    [SerializeField]
    GameObject EnemyParents; // 적 부모 오브젝트

    [Tooltip("적 스폰시 기준 오브젝트")]
    [SerializeField]
    GameObject EnemySpawnLines; // 적 스폰 위치들

    [Tooltip("적 소환시 적들의 간격")]
    [SerializeField]
    float EnemyInterval; // 적 소환 시 사이의 간격

    [ReadOnly]
    [Tooltip("가장 가까운 적")]
    [SerializeField]
    GameObject TargetEnemy; // 가장 가까운 목표 적

    [Tooltip("공격이후 적을이 내려오는데 걸리는 시간\n(효과가 없을때 기준)")]
    [SerializeField]
    float EnemyDownDuration;

    [SerializeField]
    float StartSpawnCount;


    ////////////////////////////////////////
    //입력 관연 전역 변수들
    ////////////////////////////////////////
    [Header("Input System")]

    [ReadOnly]
    [SerializeField]
    public bool IsCanTouch;

    [SerializeField]
    public AngleRange LeftAngle; // 좌측 인식 범위

    [SerializeField]
    public AngleRange UpAngle; // 상측 인식 범위

    [SerializeField]
    public AngleRange RightAngle; // 우측 인식 범위


    [HideInInspector]
    public SwipeDirection InputDirection; // 스와이프후 인식 반향 저장

    [ReadOnly]
    [SerializeField]
    public SwipeDirection answerDirection; // Player이 입력해야 하는 정답을 저장

    [Header("Restart")]
    [SerializeField]
    GameObject RestartMessage;

    void Awake()
    {
        // VSync 설정을 끄고 (0)
        QualitySettings.vSyncCount = 0;

        // 목표 프레임 속도를 60으로 설정
        Application.targetFrameRate = 60;

        // 또는 화면의 기본 재생 빈도로 설정 (예: 60Hz, 90Hz, 120Hz 등)
        // Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;
    }

    void Start()
    {
        // 게임 매니저 중복 확인
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("씬에 두개 이상의 게임 매니저가 존재합니다!");
            Destroy(gameObject);
        }

        GameSetup();

        IsCanTouch = true;

        FindToTargetEnemy(); // 목표로 지정할 적 정하기
        CheckAnswer(); // 목표 적을 기준으로 플레이어가 입력해야하는 정답 정하기

        RestartMessage.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isDead)
        {
            FindToTargetEnemy(); // 목표로 지정할 적 정하기
            CheckAnswer(); // 목표 적을 기준으로 플레이어가 입력해야하는 정답 정하기

            // 정답과 입력갑이 일치한다면
            if (answerDirection == InputDirection && InputDirection != SwipeDirection.None)
            {
                Time.timeScale = 1f;
                ProcessCorrectAnswer(); // 정답처리 실시
            }

            // 만약 틀린다면
            else if (answerDirection != InputDirection && InputDirection != SwipeDirection.None)
            {
                ProcessWrongAnswer(); // 오답처리 실시
            }
        }

        if (isDead)
        {
            if (Input.touchCount == 0)
            {

            }
            else if (Input.GetTouch(0).phase == TouchPhase.Began && Input.GetKeyDown(KeyCode.Space))
            {
                SceneManager.LoadScene("Prototype");
            }
        }
    }

    void FindToTargetEnemy()
    {
        GameObject returnEnemy = EnemyParents.transform.GetChild(0).gameObject;
        for (int i = 1; i < EnemyParents.transform.childCount; i++)
        {
            if (returnEnemy.transform.position.y > EnemyParents.transform.GetChild(i).transform.position.y)
            {
                returnEnemy = EnemyParents.transform.GetChild(i).gameObject;
            }
        }

        TargetEnemy = returnEnemy;
    }

    void CheckAnswer()
    {
        float enemy_x = TargetEnemy.transform.position.x;
        float Player_x = Player.transform.position.x;


        if (enemy_x == Player_x)
        {
            answerDirection = SwipeDirection.Up;
        }

        if (enemy_x < Player_x)
        {
            answerDirection = SwipeDirection.Left;
        }

        if (enemy_x > Player_x)
        {
            answerDirection = SwipeDirection.Right;
        }
    }

    void ProcessCorrectAnswer()
    {
        Debug.Log("정답");
        AddScore();
        IsCanTouch = false;

        if (TargetEnemy.GetComponent<EnemyController>().IsDown)
        {
            enemyRushToDown();
            backgroundRushToDown();
        }

        Player.GetComponent<PlayerController>().moveToTarget(TargetEnemy, AttackDuration);

        InputDirection = SwipeDirection.None;
    }

    void ProcessWrongAnswer()
    {
        isDead = true;
        IsCanTouch = false;
        RestartMessage.SetActive(true);
    }


    public void AttackSuccess()
    {
        CancelMove(hitEffectRoutine);
        hitEffectRoutine = StartCoroutine(HitEffect());
    }

    void Dead()
    {
        RestartMessage.SetActive(true);
        isDead = true;
    }

    public void enemySpawn()
    {
        Debug.Log("Enemy 소환");
        int Line = Random.Range(0, 3);

        Instantiate(Enemy, EnemySpawnLines.transform.GetChild(Line).position, Quaternion.identity, EnemyParents.transform);
    }

    void enemyMoveToDown()
    {
        for (int i = 0; i < EnemyParents.transform.childCount; i++)
        {
            EnemyParents.transform.GetChild(i).GetComponent<EnemyController>().MovetoDown(EnemyInterval, EnemyDownDuration);
        }
    }

    void enemyRushToDown()
    {
        for (int i = 0; i < EnemyParents.transform.childCount; i++)
        {
            EnemyParents.transform.GetChild(i).GetComponent<EnemyController>().rushToDown(AttackDuration);
        }
    }

    void backgroundMoveToDown()
    {
        for (int j = 0; j < BackgroundLayer.Length; j++)
        {
            for (int i = 0; i < BackgroundLayer[j].transform.childCount; i++)
            {
                BackgroundLayer[j].transform.GetChild(i).GetComponent<Background>().MoveToDown(BackgroundDownDistance, BackgroundDownDuration);
            }
        }
    }

    void backgroundRushToDown()
    {
        for (int j = 0; j < BackgroundLayer.Length; j++)
        {
            for (int i = 0; i < BackgroundLayer[j].transform.childCount; i++)
            {
                BackgroundLayer[j].transform.GetChild(i).GetComponent<Background>().rushToDown(AttackDuration);
            }
        }
    }

    IEnumerator HitEffect()
    {
        // 순간적인 효가를 위해 잠깐 움직임 정지
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(hitSlowEffect.HitStopTime);

        // 슬로우 모션을 표현하기 위힌 timescale 저장
        Time.timeScale = hitSlowEffect.HitSlowScale;

        Destroy(TargetEnemy); // 목표 적 제거

        enemySpawn(); // 새로운 적 소환

        FindToTargetEnemy(); // 목표로 지정할 적 정하기

        CheckAnswer(); // 목표 적을 기준으로 플레이어가 입력해야하는 정답 정하기

        Player.GetComponent<PlayerController>().moveToDown(EnemyInterval, PlayerDownDuration);

        enemyMoveToDown();

        backgroundMoveToDown();

        IsCanTouch = true;

        yield return new WaitForSecondsRealtime(hitSlowEffect.HitSlowTime);

        Time.timeScale = 1f;
    }

    void AddScore()
    {
        Debug.Log("점수 추가");
        score++;
    }
    public int pullScore()
    {
        return score;
    }

    void GameSetup()
    {
        EnemySpawnLines.transform.position = new Vector2(0f, Player.transform.position.y + (EnemyInterval * (StartSpawnCount + 1)));

        for (int i = 0; i < StartSpawnCount; i++)
        {
            enemySpawn();
            for (int j = 0; j < EnemyParents.transform.childCount; j++)
            {
                EnemyParents.transform.GetChild(j).Translate(Vector2.down * EnemyInterval);
                EnemyParents.transform.GetChild(j).GetComponent<EnemyController>().TargtPos += Vector3.down * EnemyInterval;
            }
        }
    }

    void CancelMove(Coroutine ct)
    {
        if (ct != null)
        {
            StopCoroutine(ct);
            ct = null;
        }
    }
}