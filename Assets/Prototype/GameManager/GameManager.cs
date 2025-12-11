using System.Collections;
using System.IO;
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

// 내려가는 모드 목록
[System.Serializable]
public enum DownMovementMode
{
    moveToword,
    SmoothDamp
}

[System.Serializable]
public struct Enemy
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

    [Tooltip("시작시 소환할 적 수(게임에 등장할 적의 수)")]
    [SerializeField]
    public float StartSpawnCount;

    [SerializeField]
    [ReadOnly]
    public int LastSpawnPoint;

    [HideInInspector]
    public int repeatCount;
}

[System.Serializable]
public struct Player
{
    [SerializeField]
    public GameObject playerObj;

    [SerializeField]
    public EffectManager PlayerEffect;

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

    [SerializeField]
    public AudioClip[] attackSound;
}
[System.Serializable]
public class ChangePoint
{
    public BackgroundList changeBackground;
    public float Point;
}

[System.Serializable]
public class Background
{
    [Tooltip("배경 선택")]
    [SerializeField]
    public BackgroundList selectLayer;

    [SerializeField]
    public ChangePoint[] changePoint;

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
public struct ScoreSetting
{
    [ReadOnly]
    [SerializeField]
    [Tooltip("게임 점수")]
    public int score;

    [Tooltip("점수비례 타이머 가속의 최소치(시작 수치)")]
    [SerializeField]
    public float MinScale;

    [Tooltip("점수비례 타이머 가속의 최대치(최대 수치)")]
    [SerializeField]
    public float MaxScale;

    [SerializeField]
    [Tooltip("스코어 스케일의 변화량\n(이 수치에 비례해서 최대치에 가까워짐)")]
    public float difficultyScale;

    [Tooltip("알파값(건들지 마시오)\n이 수치에 따라서 그래프의 모양이 바뀜")]
    [SerializeField]
    public float Alpha;

    [Tooltip("현제 가속")]
    [ReadOnly]
    [SerializeField]
    public float currentSpeedRate;
}

[System.Serializable]
public enum InputMode
{
    AnswerCheckMode,
    AngleCheckMode
}

public class GameManager : MonoBehaviour
{
    [ReadOnly]
    public static GameManager instance;
    [SerializeField]
    [Tooltip("게임의 점수")]
    ScoreSetting scoreSetting;

    // Timer UI 오브젝트
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
    public HitSlowEffect hitSlowEffect;

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
    public Enemy enemy;


    ////////////////////////////////////////
    //입력 관연 전역 변수들
    ////////////////////////////////////////
    [Header("Input System")]

    [SerializeField]
    public InputMode inputMode;

    [ReadOnly]
    [SerializeField]
    public bool IsCanTouch;

    [SerializeField]
    public AngleRange LeftAngle; // 좌측 인식 범위

    [SerializeField]
    public AngleRange UpAngle; // 상측 인식 범위

    [SerializeField]
    public AngleRange RightAngle; // 우측 인식 범위

    [SerializeField]
    public float errorAngleRange;

    [HideInInspector]
    public SwipeDirection InputDirection; // 스와이프후 인식 반향 저장

    [ReadOnly]
    [SerializeField]
    public SwipeDirection answerDirection; // Player이 입력해야 하는 정답을 저장

    [Header("Restart")]
    [SerializeField]
    GameObject RestartMessage;

    [SerializeField]
    public float DeadFallTime;

    [ReadOnly]
    [SerializeField]
    bool isStart = false;

    //
    private ISceneService sceneService;
    
    // 카메라
    [SerializeField]
    GameObject gameCamera;
    CameraMovement cameraMovement;

    [Header("Score For Coin")]
    
    [SerializeField]
    int minScoreForCoin;

    [SerializeField]
    int maxScoreForCoin;
    
    [ReadOnly][SerializeField]
    private int noHaveCoinEnemyCount;

    void Awake()
    {
        #region Awake

        sceneService = ServiceLocator.Resolve<ISceneService>();

        // VSync 설정을 끄고 (0)
        QualitySettings.vSyncCount = 0;

        // 목표 프레임 속도를 60으로 설정
        Application.targetFrameRate = 60;

        // 또는 화면의 기본 재생 빈도로 설정 (예: 60Hz, 90Hz, 120Hz 등)
        // Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;

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

        #endregion
    }

    void Start()
    {
        #region Start

        GameSetup();

        IsCanTouch = true;

        FindToTargetEnemy(); // 목표로 지정할 적 정하기
        CheckAnswer(); // 목표 적을 기준으로 플레이어가 입력해야하는 정답 정하기

        RestartMessage.SetActive(false);

        scoreSetting.currentSpeedRate = 1f;

        //Timer.GetComponent<TimerBarController>().PauseTimer();

        cameraMovement = gameCamera.GetComponent<CameraMovement>();


        #endregion
    }

    // Update is called once per frame
    void Update()
    {
        if (!player.isDead)
        {
            if (!isStart)
            {
                Timer.GetComponent<TimerBarController>().ResumeTimer();
                isStart = true;
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

            CheckBackground();
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
        GameObject returnEnemy = enemy.EnemyParent.transform.GetChild(0).gameObject;
        for (int i = 1; i < enemy.EnemyParent.transform.childCount; i++)
        {
            if (returnEnemy.transform.position.y > enemy.EnemyParent.transform.GetChild(i).transform.position.y)
            {
                returnEnemy = enemy.EnemyParent.transform.GetChild(i).gameObject;
            }
        }

        enemy.TargetEnemy = returnEnemy;
    }

    // 정답을 학인하여 비교
    void CheckAnswer()
    {
        float enemy_x = enemy.TargetEnemy.transform.position.x;
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

        scoreSetting.currentSpeedRate = ScaleToScore(scoreSetting.score);

        if (enemy.TargetEnemy.GetComponent<EnemyController>().IsDown)
        {
            enemyRushToDown();
            backgroundRushToDown();
            effectRushToDown();
        }

        player.playerObj.GetComponent<PlayerController>().moveToTarget(enemy.TargetEnemy, player.AttackDuration);

        player.playerObj.GetComponent<PlayerController>().PlayerAnimationCalculationProcessing(InputDirection);

        InputDirection = SwipeDirection.None;

        player.PlayerEffect.targetAngle();

        player.PlayerEffect.windFX();
    }
    //////////////////////////////////////////////////////////////
    /// 오답 처리 진행
    //////////////////////////////////////////////////////////////
    void ProcessWrongAnswer()
    {
        Timer.GetComponent<TimerBarController>().PauseTimer();

        player.isDead = true;
        IsCanTouch = false;

        Time.timeScale = 1f;

        //RestartMessage.SetActive(true);
        player.isDead = true;

        scoreSetting.currentSpeedRate = ScaleToScore(scoreSetting.score);

        if (enemy.TargetEnemy.GetComponent<EnemyController>().IsDown)
        {
            enemyRushToDown();
            backgroundRushToDown();
            effectRushToDown();
        }

        float x = player.playerObj.transform.position.x;

        switch (InputDirection)
        {
            case SwipeDirection.Left:
                x += -3f;
                break;
            case SwipeDirection.Up:
                x += 0;
                break;
            case SwipeDirection.Right:
                x += 3f;
                break;
        }

        Vector2 TP = new Vector2(x, enemy.TargetEnemy.transform.position.y);

        player.playerObj.GetComponent<PlayerController>().moveToPos(TP, player.AttackDuration);

        player.playerObj.GetComponent<PlayerController>().PlayerAnimationCalculationProcessing(InputDirection);

        //Dead();
    }

    public void ReLoad()
    {
        SceneManager.LoadScene("Prototype");
    }


    public void AttackSuccess()
    {
        CancelMove();
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

        PlayerDataManager.instance.InputMaxScore(scoreSetting.score);
       PlayerDataManager.instance.SaveJson();
    }

    //////////////////////////////////////////////////////////////
    /// 적 소환 메서드
    //////////////////////////////////////////////////////////////
    public void enemySpawn()
    {
        //Debug.Log("enemy 소환");
        GameObject ennmy;

        int SpawnPoint = enemy.LastSpawnPoint;

        int changeCheck;

        switch (SpawnPoint)
        {
            case (0):
                changeCheck = Random.Range(0, 2 + enemy.repeatCount);

                if (changeCheck == 0)
                {
                    SpawnPoint = 0;
                    enemy.repeatCount++;
                }
                else
                {
                    SpawnPoint = 1;
                    enemy.repeatCount = 0;
                }
                break;
            case (1):
                changeCheck = Random.Range(0, 3 + enemy.repeatCount);

                if (changeCheck == 0)
                {
                    SpawnPoint = 1;
                    enemy.repeatCount += 2;
                }
                else
                {
                    if (changeCheck % 2 == 0)
                        SpawnPoint = 0;
                    else
                        SpawnPoint = 2;
                }
                break;
            case (2):
                changeCheck = Random.Range(0, 2 + enemy.repeatCount);

                if (changeCheck == 0)
                {
                    SpawnPoint = 2;
                    enemy.repeatCount++;
                }
                else
                {
                    SpawnPoint = 1;
                    enemy.repeatCount = 0;
                }
                break;
        }

        // 마지막 스폰 위치 저장
        enemy.LastSpawnPoint = SpawnPoint;

        // 적 소환
        ennmy = Instantiate(enemy.Prefab, enemy.EnemySpawnLines.transform.GetChild(SpawnPoint).position, Quaternion.identity, enemy.EnemyParent.transform);

        CoinManagement date = ennmy.GetComponent<CoinManagement>();

        TryGrantScoreReward(date);
    }

    void enemyMoveToDown()
    {
        for (int i = 0; i < enemy.EnemyParent.transform.childCount; i++)
        {
            enemy.EnemyParent.transform.GetChild(i).GetComponent<EnemyController>().moveToDown(enemy.EnemyInterval, enemy.EnemyDownDuration, enemy.DownMode);
        }
    }

    void enemyRushToDown()
    {
        for (int i = 0; i < enemy.EnemyParent.transform.childCount; i++)
        {
            enemy.EnemyParent.transform.GetChild(i).GetComponent<EnemyController>().rushToDown(player.AttackDuration);
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

    void effectMoveToDown()
    {
        for (int i = 0; i < player.PlayerEffect.gameObject.transform.childCount; i++)
        {
            player.PlayerEffect.gameObject.transform.GetChild(i).GetComponent<EffectMovementController>().moveToDown(enemy.EnemyInterval, enemy.EnemyDownDuration, enemy.DownMode);
        }
    }

    void effectRushToDown()
    {
        for (int i = 0; i < player.PlayerEffect.gameObject.transform.childCount; i++)
        {
            player.PlayerEffect.gameObject.transform.GetChild(i).GetComponent<EffectMovementController>().rushToDown(player.AttackDuration);
        }
    }

    IEnumerator HitEffect()
    {
        cameraMovement.ShakeMove();

        player.PlayerEffect.AttackSlashFX();

        player.playerObj.GetComponent<PlayerController>().AttackIsHit(true);

        for (int i = 0; i < player.attackSound.Length; i++)
        {
            ServiceLocator.Resolve<IAudioService>().PlaySfx(player.attackSound[i]);
        } 
        
        // 순간적인 효과를 위해 잠깐 움직임 정지
        Time.timeScale = 0f;

        Timer.GetComponent<TimerBarController>().AddTime(AddSecond);
        Timer.GetComponent<TimerBarController>().TimerScale(scoreSetting.currentSpeedRate);

        // 타이머 잠시 멈춤
        Timer.GetComponent<TimerBarController>().PauseTimer();

        yield return new WaitForSecondsRealtime(hitSlowEffect.HitStopTime); // HitStopTime 시간동안 잠시 정지

        // 슬로우 모션을 표현하기 위힌 timescale 저장
        Time.timeScale = hitSlowEffect.HitSlowScale;

        enemy.TargetEnemy.GetComponent<EnemyController>().Dead(); // 목표 적 제거

        enemySpawn(); // 새로운 적 소환

        FindToTargetEnemy(); // 목표로 지정할 적 정하기

        CheckAnswer(); // 목표 적을 기준으로 플레이어가 입력해야하는 정답 정하기

        player.playerObj.GetComponent<PlayerController>().moveToDown(enemy.EnemyInterval, player.PlayerDownDuration, player.DownMode);

        enemyMoveToDown();

        backgroundMoveToDown();

        effectMoveToDown();

        IsCanTouch = true;

        // 타이머 제계
        Timer.GetComponent<TimerBarController>().ResumeTimer();

        yield return new WaitForSecondsRealtime(hitSlowEffect.HitSlowTime);

        //scoreSetting.currentSpeedRate += scoreSetting.difficultyScale;

        //Time.timeScale = scoreSetting.currentSpeedRate;

        Time.timeScale = 1;
    }

    #region Score
    void AddScore()
    {
        Debug.Log("점수 추가");
        scoreSetting.score++;
    }

    
    //////////////////////////////////////////////////////////////
    /// 점수 반환
    //////////////////////////////////////////////////////////////
    public int pullScore()
    {
        return scoreSetting.score;
    }

    public int PullMaxScore()
    {
        return PlayerDataManager.instance.PullMaxScore();
    }
    
    float ScaleToScore(float score)
    {
        float scale = scoreSetting.MinScale + (scoreSetting.MaxScale - scoreSetting.MinScale) * (1 - Mathf.Exp(-scoreSetting.difficultyScale * Mathf.Pow(score, scoreSetting.Alpha)));
        return scale;
    }
    #endregion

    #region GemeSetUp
    void GameSetup()
    {
        //적 스폰 라인 위치 계산 후 배치
        enemy.EnemySpawnLines.transform.position = new Vector2(0f, player.playerObj.transform.position.y + (enemy.EnemyInterval * (enemy.StartSpawnCount + 1)));

        // 적 소환
        for (int i = 0; i < enemy.StartSpawnCount; i++)
        {
            enemySpawn();
            for (int j = 0; j < enemy.EnemyParent.transform.childCount; j++)
            {
                enemy.EnemyParent.transform.GetChild(j).Translate(Vector2.down * enemy.EnemyInterval);
                enemy.EnemyParent.transform.GetChild(j).GetComponent<EnemyController>().TargtPos += Vector3.down * enemy.EnemyInterval;
            }
        }

        // 적 중복 소환시 카운팅하는 변수 초기화
        enemy.repeatCount = 0;

        noHaveCoinEnemyCount = 0;
    }
    #endregion

    void CancelMove()
    {
        if (hitEffectRoutine != null)
        {
            StopCoroutine(hitEffectRoutine);
            hitEffectRoutine = null;
        }

    }
    
    public void TotheStart()
    {
        sceneService.LoadSceneWithLoading(SceneNames.START);
    }

    void CheckBackground()
    {
        for (int i = 0; i < background.changePoint.Length; i++)
        {
            if (scoreSetting.score >= background.changePoint[i].Point)
            {
                Debug.Log("배경변경 : " + background.changePoint[i].changeBackground);
                background.selectLayer = background.changePoint[i].changeBackground;
            }
        }
    }

    public PlayerDataManager FindPlayerDateManager()
    {
        GameObject dateManager = GameObject.Find("PlayerDate");
        return dateManager.GetComponent<PlayerDataManager>();
    }

    public void TryGrantScoreReward(CoinManagement coinDate)
    {
        if(maxScoreForCoin == noHaveCoinEnemyCount)
        {
            noHaveCoinEnemyCount = 0;
            coinDate.addCoin(1);
        }
        else if(minScoreForCoin <= noHaveCoinEnemyCount && maxScoreForCoin > noHaveCoinEnemyCount)
        {
            int R = Random.Range(minScoreForCoin,maxScoreForCoin);
            if(R < noHaveCoinEnemyCount)
            {
                noHaveCoinEnemyCount = 0;
                coinDate.addCoin(1);
            }
            else
            {
                noHaveCoinEnemyCount++;
            }
        }
        else
        {
            noHaveCoinEnemyCount++;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        float h = Mathf.Sin((90f - (errorAngleRange/2)) * Mathf.Deg2Rad);

        float w = Mathf.Cos((90f - (errorAngleRange/2)) * Mathf.Deg2Rad);

        Vector2 errorAngle1 = new Vector2(w,h).normalized;
        Vector2 errorAngle2 = new Vector2(-w,h).normalized;

        Vector2 pos = player.playerObj.transform.position;

        Gizmos.DrawLine(pos, pos + (errorAngle1 * enemy.EnemyInterval));
        Gizmos.DrawLine(pos,  pos + (errorAngle2 * enemy.EnemyInterval));
    }
}