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

[System.Serializable]
public enum DownMovementMode
{
    moveToword,
    SmoothDamp
}

[System.Serializable]
public struct enemy
{
    [Tooltip("적 프리펩")]
    [SerializeField]
    public GameObject Prefab; // 적 프리펩

    [Tooltip("적 소환시 부모지정")]
    [SerializeField]
    public GameObject EnemyParent; // 적 부모 오브젝트

    [Tooltip("적 스폰시 기준 오브젝트")]
    [SerializeField]
    public GameObject EnemySpawnLines; // 적 스폰 위치들

    [ReadOnly]
    [Tooltip("가장 가까운 적")]
    [SerializeField]
    public GameObject TargetEnemy; // 가장 가까운 목표 적

    [SerializeField]
    public DownMovementMode DownMode;

    [Tooltip("적 소환시 적들의 간격")]
    [SerializeField]
    public float EnemyInterval; // 적 소환 시 사이의 간격


    [Tooltip("공격이후 적을이 내려오는데 걸리는 시간\n(효과가 없을때 기준)")]
    [SerializeField]
    public float EnemyDownDuration;

    [SerializeField]
    public float StartSpawnCount;
}

[System.Serializable]
public struct Player
{
    [SerializeField]
    public GameObject playerObj;

    [SerializeField]
    public DownMovementMode DownMode;

    [Tooltip("공격하는 걸리는 시간")]
    [SerializeField]
    public float AttackDuration; //공격하는데 걸리는 시간

    [Tooltip("공격후 내려오는데 걸리는 시간\n(효과가 없을때 기준)")]
    [SerializeField]
    public float PlayerDownDuration; // Player 떨어지는데 걸리는 시간

    [Tooltip("공격 이후 관성(미구현)")]
    [SerializeField]
    public float reboundPower;

    // [Tooltip("")]
    // [ReadOnly]
    // [SerializeField]
    // Vector2 PlayerMovePos;


    // 플레이어가 죽을시 true가 되는 변수
    [HideInInspector]
    public bool isDead;
}

[System.Serializable]
public class Background
{
    [Tooltip("배경 선택")]
    [SerializeField]
    public BackgroundList selectLayer;

    [Tooltip("루프할 배경")]
    [SerializeField]
    public GameObject[] BackgroundLayer;

    [SerializeField]
    public DownMovementMode DownMode;

    [Tooltip("배경이 내려가는 거리")]
    [SerializeField]
    public float BackgroundDownDistance; // 배경 내려가는 거리

    [Tooltip("배경이 내려갈 시 걸리는 시간")]
    [SerializeField]
    public float BackgroundDownDuration; // 1회 내려갈 시 걸리는 시간
}

[System.Serializable]
public struct Scoresetting
{
    [ReadOnly]
    [SerializeField]
    [Tooltip("게임의 점수")]
    public int score;

    [SerializeField]
    public float difficultyScale;

    [ReadOnly]
    [SerializeField]
    public float currentSpeedRate;
}


public class GameManager : MonoBehaviour
{
    [ReadOnly]
    public static GameManager instance;
    [SerializeField]
    [Tooltip("게임의 점수")]
    Scoresetting scoreSetting;

    [SerializeField]
    GameObject Timer;

    [SerializeField]
    public float MaxTime;

    [SerializeField]
    public float startTime;

    [SerializeField]
    float AddSecond;

    [Header("Game Setting")]
    [Space(5f)]

    [Tooltip("공격 히트 후 효과수치")]
    [SerializeField]
    HitSlowEffect hitSlowEffect;

    Coroutine hitEffectRoutine;

    ////////////////////////////////////////
    /// 배경화면 관련
    ////////////////////////////////////////
    [SerializeField]
    public Background background;

    ////////////////////////////////////////
    /// 플레이어 관련
    ////////////////////////////////////////
    [SerializeField]
    public Player player;

    ////////////////////////////////////////
    /// 적 관련
    // ////////////////////////////////////////
    [Space(10f)]
    [SerializeField]
    public enemy Enemy;


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

        scoreSetting.currentSpeedRate = 1f;

        //Timer.GetComponent<TimerBarController>().PauseTimer();
    }

    // Update is called once per frame
    void Update()
    {
        if (!player.isDead)
        {
            if (!Timer.GetComponent<TimerBarController>().IsTimerActive() && scoreSetting.score > 0f)
            {
                Timer.GetComponent<TimerBarController>().ResumeTimer();
            }
            // startTimer

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

            // if(Timer.GetComponent<TimerBarController>().GetRemainingTime() < 0f)
            //     {}
        }

        if (player.isDead)
        {
            if (Input.touchCount == 0)
            {

            }
            else if (Input.GetTouch(0).phase == TouchPhase.Began || Input.GetKeyDown(KeyCode.Space))
            {
                //SceneManager.LoadScene("Prototype");
            }
        }
    }

    // 가장 가까운 적을 목표 적으로 지정
    void FindToTargetEnemy()
    {
        GameObject returnEnemy = Enemy.EnemyParent.transform.GetChild(0).gameObject;
        for (int i = 1; i < Enemy.EnemyParent.transform.childCount; i++)
        {
            if (returnEnemy.transform.position.y > Enemy.EnemyParent.transform.GetChild(i).transform.position.y)
            {
                returnEnemy = Enemy.EnemyParent.transform.GetChild(i).gameObject;
            }
        }

        Enemy.TargetEnemy = returnEnemy;
    }

    // 정답을 학인하여 비교
    void CheckAnswer()
    {
        float enemy_x = Enemy.TargetEnemy.transform.position.x;
        float Player_x = player.playerObj.transform.position.x;


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

    // 정답처리 진행
    void ProcessCorrectAnswer()
    {
        Debug.Log("정답");
        AddScore();
        IsCanTouch = false;

        scoreSetting.currentSpeedRate += scoreSetting.difficultyScale;

        if (Enemy.TargetEnemy.GetComponent<EnemyController>().IsDown)
        {
            enemyRushToDown();
            backgroundRushToDown();
        }

        player.playerObj.GetComponent<PlayerController>().moveToTarget(Enemy.TargetEnemy, player.AttackDuration);

        player.playerObj.GetComponent<PlayerController>().PlayerAnimationCalculationProcessing(InputDirection);

        InputDirection = SwipeDirection.None;
    }

    // 오답 처리 진행
    void ProcessWrongAnswer()
    {
        // player.isDead = true;
        // IsCanTouch = false;

        //RestartMessage.SetActive(true);
        Dead();
    }

    public void ReLoad()
    {
        SceneManager.LoadScene("Prototype");
    }


    public void AttackSuccess()
    {
        CancelMove(hitEffectRoutine);
        hitEffectRoutine = StartCoroutine(HitEffect());
    }

    public void Dead()
    {
        Timer.GetComponent<TimerBarController>().PauseTimer();

        player.isDead = true;
        IsCanTouch = false;

        Time.timeScale = 1f;

        RestartMessage.SetActive(true);
        player.isDead = true;
    }

    public void enemySpawn()
    {
        Debug.Log("Enemy 소환");
        int Line = Random.Range(0, 3);

        Instantiate(Enemy.Prefab, Enemy.EnemySpawnLines.transform.GetChild(Line).position, Quaternion.identity, Enemy.EnemyParent.transform);
    }

    void enemyMoveToDown()
    {
        for (int i = 0; i < Enemy.EnemyParent.transform.childCount; i++)
        {
            Enemy.EnemyParent.transform.GetChild(i).GetComponent<EnemyController>().moveToDown(Enemy.EnemyInterval, Enemy.EnemyDownDuration, Enemy.DownMode);
        }
    }

    void enemyRushToDown()
    {
        for (int i = 0; i < Enemy.EnemyParent.transform.childCount; i++)
        {
            Enemy.EnemyParent.transform.GetChild(i).GetComponent<EnemyController>().rushToDown(player.AttackDuration);
        }
    }

    void backgroundMoveToDown()
    {
        for (int j = 0; j < background.BackgroundLayer.Length; j++)
        {
            for (int i = 0; i < background.BackgroundLayer[j].transform.childCount; i++)
            {
                background.BackgroundLayer[j].transform.GetChild(i).GetComponent<BackgroundController>().moveToDown(background.BackgroundDownDistance, background.BackgroundDownDuration, background.DownMode);
            }
        }
    }

    void backgroundRushToDown()
    {
        for (int j = 0; j < background.BackgroundLayer.Length; j++)
        {
            for (int i = 0; i < background.BackgroundLayer[j].transform.childCount; i++)
            {
                background.BackgroundLayer[j].transform.GetChild(i).GetComponent<BackgroundController>().rushToDown(player.AttackDuration, background.DownMode);
            }
        }
    }

    IEnumerator HitEffect()
    {
        // 순간적인 효가를 위해 잠깐 움직임 정지
        Time.timeScale = 0f;

        Timer.GetComponent<TimerBarController>().AddTime(AddSecond);
        Timer.GetComponent<TimerBarController>().TimerScale(scoreSetting.currentSpeedRate);

        yield return new WaitForSecondsRealtime(hitSlowEffect.HitStopTime);

        // 슬로우 모션을 표현하기 위힌 timescale 저장
        Time.timeScale = hitSlowEffect.HitSlowScale;

        Destroy(Enemy.TargetEnemy); // 목표 적 제거

        enemySpawn(); // 새로운 적 소환

        FindToTargetEnemy(); // 목표로 지정할 적 정하기

        CheckAnswer(); // 목표 적을 기준으로 플레이어가 입력해야하는 정답 정하기

        player.playerObj.GetComponent<PlayerController>().moveToDown(Enemy.EnemyInterval, player.PlayerDownDuration, player.DownMode);

        enemyMoveToDown();

        backgroundMoveToDown();

        IsCanTouch = true;

        yield return new WaitForSecondsRealtime(hitSlowEffect.HitSlowTime);

        //scoreSetting.currentSpeedRate += scoreSetting.difficultyScale;

        Time.timeScale = scoreSetting.currentSpeedRate;
    }

    void AddScore()
    {
        Debug.Log("점수 추가");
        scoreSetting.score++;
    }
    public int pullScore()
    {
        return scoreSetting.score;
    }

    void GameSetup()
    {
        Enemy.EnemySpawnLines.transform.position = new Vector2(0f, player.playerObj.transform.position.y + (Enemy.EnemyInterval * (Enemy.StartSpawnCount + 1)));

        for (int i = 0; i < Enemy.StartSpawnCount; i++)
        {
            enemySpawn();
            for (int j = 0; j < Enemy.EnemyParent.transform.childCount; j++)
            {
                Enemy.EnemyParent.transform.GetChild(j).Translate(Vector2.down * Enemy.EnemyInterval);
                Enemy.EnemyParent.transform.GetChild(j).GetComponent<EnemyController>().TargtPos += Vector3.down * Enemy.EnemyInterval;
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