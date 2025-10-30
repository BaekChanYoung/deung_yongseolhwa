using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;


[System.Serializable]
public class AngleRange
{
    public float Max;
    public float Min;
}


[System.Serializable]
public enum DownMovementMode
{
<<<<<<< Updated upstream
    ////////////////////////////////////////
    // 게임 메니저 자기자신을 저장
    ////////////////////////////////////////
    [ReadOnly]
    public static GameManager instance;
    [Header("score")]

    //[SerializeField]
    //TextMeshProUGUI ScoreTextUI;

    [SerializeField]
    int score;

    ////////////////////////////////////////
    /// 배경화면 관련
    ////////////////////////////////////////

    [Header("Background")]

    [SerializeField]
    GameObject Background;

=======
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
>>>>>>> Stashed changes
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

    [SerializeField]
    public float AttackDuration;

    float AttackSpeed;

    [SerializeField]
<<<<<<< Updated upstream
    float PlayerDownDuration;
=======
    public float PlayerDownDuration; // Player 떨어지는데 걸리는 시간
>>>>>>> Stashed changes

    float PlayerDownSpeed;

    bool PlayerisMove;

    [ReadOnly]
    [SerializeField]
<<<<<<< Updated upstream
    Vector2 PlayerMovePos;
=======
    public float reboundPower;
>>>>>>> Stashed changes

    [HideInInspector]
    public bool isDead;
<<<<<<< Updated upstream

    ////////////////////////////////////////
    /// 적 관련
    ////////////////////////////////////////
    [Header("Enemy")]

    [SerializeField]
    GameObject Enemy; // 적 프리펩

    [SerializeField]
    GameObject Enemies; // 적 부모 오브젝트

    [SerializeField]
    GameObject EnemySpawnLines; // 적 스폰 위치들

    [SerializeField]
    float EnemyInterval; // 적 소환 시 사이의 간격

    [ReadOnly]
    [SerializeField]
    GameObject TargetEnemy; // 가장 가까운 목표 적

    [SerializeField]
    float EnemyDownDuration;

    [SerializeField]
    float StartSpawnCount;
=======
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
>>>>>>> Stashed changes



    bool isEnmeyDown;


    ////////////////////////////////////////
    //입력 관연 전역 변수들
    ////////////////////////////////////////
    [Header("Input System")]


    [SerializeField]
    AngleRange LeftAngle; // 좌측 인식 범위

    [SerializeField]
    AngleRange UpAngle; // 상측 인식 범위

    [SerializeField]
    AngleRange RightAngle; // 우측 인식 범위



    string InputDirection; // 스와이프후 인식 반향 저장

    [ReadOnly]
    [SerializeField]
    public string answerDirection;

    bool isTouch; // 터치하고 있는지 학인

    Vector2 firstTouchPos; // 터치 시작시, 시작 위치값 저장

    Vector2 endTouchPos; // 터치 종료시, 종료 위치값 저장

    float? Inputangle; // 스와이프 값 저장

    [Header("Restart")]
    [SerializeField]
    GameObject RestartMessage;

<<<<<<< Updated upstream
    void Start()
    {
=======
    void Awake()
    {
        // VSync 설정을 끄고 (0)
        QualitySettings.vSyncCount = 0;

        // 목표 프레임 속도를 60으로 설정
        Application.targetFrameRate = 60;

        // 또는 화면의 기본 재생 빈도로 설정 (예: 60Hz, 90Hz, 120Hz 등)
        // Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;

>>>>>>> Stashed changes
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
    }

    void Start()
    {
        


        InputDirection = "None";
        RestartMessage.SetActive(false);

<<<<<<< Updated upstream
        GameSetup();
=======
        scoreSetting.currentSpeedRate = 1f;

        //Timer.GetComponent<TimerBarController>().PauseTimer();
>>>>>>> Stashed changes
    }


    void Update()
    {
        if (!player.isDead)
        {
<<<<<<< Updated upstream
            FindToTargetEnemy(); // 목표 적 정하기
=======
            if (!Timer.GetComponent<TimerBarController>().IsTimerActive() && scoreSetting.score > 0f)
            {
                Timer.GetComponent<TimerBarController>().ResumeTimer();
            }
            // startTimer

            FindToTargetEnemy(); // 목표로 지정할 적 정하기
            CheckAnswer(); // 목표 적을 기준으로 플레이어가 입력해야하는 정답 정하기
>>>>>>> Stashed changes

            // 정답 정하기
            float enemy_x = TargetEnemy.transform.position.x;
            float Player_x = Player.transform.position.x;


            if (enemy_x == Player_x)
            {
                answerDirection = "up";
            }

            if (enemy_x < Player_x)
            {
                answerDirection = "left";
            }

            if (enemy_x > Player_x)
            {
                answerDirection = "right";
            }

            // 입력하기
            if (!PlayerisMove && !isEnmeyDown) // 플레이어가 움직이지 않는다면
            {
                InputSystem();
            }

            //만약 정답이면
            if (answerDirection == InputDirection)
            {
                rightAnswer();
                //StartCoroutine(PlayerMoveToTarget()); // 플레이어가 목표를 향해서 움직이기 시작
            }
            // 만약 틀린다면
            else if (answerDirection != InputDirection && InputDirection != "None")
            {
                PlayerisMove = true; // 플레이어 움직임

                StartCoroutine(PlayerMoveToTarget(InputDirection));

                InputDirection = "None";
            }

            // if(Timer.GetComponent<TimerBarController>().GetRemainingTime() < 0f)
            //     {}
        }

        if (player.isDead)
        {
            if (Input.touchCount == 0)
            {

            }
<<<<<<< Updated upstream
            else if (Input.GetTouch(0).phase == TouchPhase.Began)
=======
            else if (Input.GetTouch(0).phase == TouchPhase.Began || Input.GetKeyDown(KeyCode.Space))
>>>>>>> Stashed changes
            {
                //SceneManager.LoadScene("Prototype");
            }
        }
    }

<<<<<<< Updated upstream


    void InputSystem()
    {
        Swipe(); // 스와이프 인식

        // 스와이프 인식 후 각도에 따른 인식

        // 좌측으로 인식할 시
        if ((LeftAngle.Min < Inputangle && Inputangle < LeftAngle.Max) || Input.GetKeyDown(KeyCode.A))
        {
            Debug.Log("left");
            InputDirection = "left";
        }

        // 우측으로 인식할 시
        if ((RightAngle.Min < Inputangle && Inputangle < RightAngle.Max) || Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log("right");
            InputDirection = "right";
        }

        // 상측으로 인식할 시
        if ((UpAngle.Min <= Inputangle && Inputangle <= UpAngle.Max) || Input.GetKeyDown(KeyCode.W))
        {
            Debug.Log("up");
            InputDirection = "up";
        }

        Inputangle = null;

    }



    void Swipe()
    {
        // 터치를 안하고 있을때
        if (Input.touchCount > 0 && !isTouch)
        {
            firstTouchPos = Input.GetTouch(0).position; // 첫번째 터치 위치 저장

            isTouch = true; // 터치 시작 확인
        }

        // 터치를 하고 있을때
        if (Input.touchCount > 0 && isTouch)
        {
            endTouchPos = Input.GetTouch(0).position; // 터치가 끝날때까지 마지막 터치 위치 저장
        }

        // 터치 종료시
        if (Input.touchCount == 0 && isTouch)
        {
            Vector2 dif = endTouchPos - firstTouchPos; // 스와이프 방향백터 계산 (종료 위치값 - 시작 위치값)

            float angle = Mathf.Atan2(dif.y, dif.x) * Mathf.Rad2Deg; // 방향 백터에 따른 스와이프 각도 계산

            //Debug.Log(angle);

            isTouch = false; //터치 종료 확인

            Inputangle = angle; // 스와이프 각도 값 반환
            Debug.Log(Inputangle);
        }
    }

    public void enemySpawn()
    {
        Debug.Log("Enemy 소환");
        int Line = Random.Range(0, 3);

        Instantiate(Enemy, EnemySpawnLines.transform.GetChild(Line).position, Quaternion.identity, Enemies.transform);
    }

    void FindToTargetEnemy()
    {
        GameObject returnEnemy = Enemies.transform.GetChild(0).gameObject;
        for (int i = 1; i < Enemies.transform.childCount; i++)
        {
            if (returnEnemy.transform.position.y > Enemies.transform.GetChild(i).transform.position.y)
            {
                returnEnemy = Enemies.transform.GetChild(i).gameObject;
=======
    // 가장 가까운 적을 목표 적으로 지정
    void FindToTargetEnemy()
    {
        GameObject returnEnemy = Enemy.EnemyParent.transform.GetChild(0).gameObject;
        for (int i = 1; i < Enemy.EnemyParent.transform.childCount; i++)
        {
            if (returnEnemy.transform.position.y > Enemy.EnemyParent.transform.GetChild(i).transform.position.y)
            {
                returnEnemy = Enemy.EnemyParent.transform.GetChild(i).gameObject;
>>>>>>> Stashed changes
            }
        }

        Enemy.TargetEnemy = returnEnemy;
    }

<<<<<<< Updated upstream
    void Dead()
=======
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
>>>>>>> Stashed changes
    {
        Timer.GetComponent<TimerBarController>().PauseTimer();

        player.isDead = true;
        IsCanTouch = false;

        Time.timeScale = 1f;

        RestartMessage.SetActive(true);
        player.isDead = true;
    }

<<<<<<< Updated upstream
=======
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

>>>>>>> Stashed changes
    void AddScore()
    {
        Debug.Log("점수 추가");
        scoreSetting.score++;
    }

    // 다른 곳에서 점수 불러오기
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
<<<<<<< Updated upstream
            for (int j = 0; j < Enemies.transform.childCount; j++)
            {
                Enemies.transform.GetChild(j).Translate(Vector2.down * EnemyInterval);
            }
        }

    }
    void rightAnswer()
    {
        Debug.Log("정답");
        AddScore();
        enemySpawn();

        InputDirection = "None";

        PlayerisMove = true; // 플레이어 움직임

        


        StartCoroutine(PlayerMoveToTarget());
        //StartCoroutine(PlayerMove());
        //StartCoroutine(EnemyMove());
    }

    ////////////////////////////////////////
    // prototype 0.0.3 이후 사용 안할것 같음
    ////////////////////////////////////////
    /// 다시 쓰는구만
    IEnumerator PlayerMoveToTarget()
    {
        PlayerMovePos = TargetEnemy.transform.position; // 플레이어가 움직여야할 위치값 저장

        float distance = Vector2.Distance(Player.transform.position, PlayerMovePos); // 움직여야 할 거리 계산

        AttackSpeed = distance / AttackDuration; // 공격시 이동 속도 계산

        while (true)
        {
            yield return null;
            Player.transform.position = Vector2.MoveTowards(Player.transform.position, PlayerMovePos, AttackSpeed * Time.deltaTime);
            if ((Vector2)Player.transform.position == PlayerMovePos)
            {
                Destroy(TargetEnemy);
                //enemySpawn();
                StartCoroutine(EnemyMoveToDown());
                StartCoroutine(PlayerMoveToDown());
                yield break;
=======
            for (int j = 0; j < Enemy.EnemyParent.transform.childCount; j++)
            {
                Enemy.EnemyParent.transform.GetChild(j).Translate(Vector2.down * Enemy.EnemyInterval);
                Enemy.EnemyParent.transform.GetChild(j).GetComponent<EnemyController>().TargtPos += Vector3.down * Enemy.EnemyInterval;
>>>>>>> Stashed changes
            }
        }
    }

    IEnumerator PlayerMove()
    {
        PlayerMovePos = new Vector2(TargetEnemy.transform.position.x, Player.transform.position.y); // 플레이어가 움직여야할 위치값 저장

        float distance = Vector2.Distance(Player.transform.position, PlayerMovePos); // 움직여야 할 거리 계산

        AttackSpeed = distance / AttackDuration; // 공격시 이동 속도 계산
        
        while (true)
        {
            yield return null;
            Player.transform.position = Vector2.MoveTowards(Player.transform.position, PlayerMovePos, AttackSpeed * Time.deltaTime);
            if ((Vector2)Player.transform.position == PlayerMovePos)
            {
                //Destroy(TargetEnemy);
                yield break;
            }
        }
    }

    IEnumerator EnemyMove()
    {
        yield return null;
        //Debug.Log("문재 부분?");
        isEnmeyDown = true;
        int EnemyCount = Enemies.transform.childCount;

        Debug.Log(EnemyCount);
        Vector2[] EnemyMovePos = new Vector2[EnemyCount];
        float[] EnemyMoveSpeed = new float[EnemyCount];

        for (int i = 0; i < EnemyCount; i++)
        {
            EnemyMovePos[i] = (Vector2)Enemies.transform.GetChild(i).transform.position + Vector2.down * EnemyInterval;
            float distance = Vector2.Distance((Vector2)Enemies.transform.GetChild(i).transform.position, EnemyMovePos[i]); // 움직여야 할 거리 계산
            EnemyMoveSpeed[i] = distance / EnemyDownDuration;
        }

        // 배경 내리기 시작
        Background.GetComponent<BackgroundManager>().MovetoBackground(BackgroundDownDistance, BackgroundDownDuration);


        while (true)
        {
            int Count = 0;

            yield return null;
            for (int i = 0; i < EnemyCount; i++)
            {
                Debug.Log("적 움직임");

                Transform E = Enemies.transform.GetChild(i);
                //Debug.Log("Enemy Number : " + i);

                E.position = Vector2.MoveTowards(E.position, EnemyMovePos[i], EnemyMoveSpeed[i] * Time.deltaTime);
                //Debug.Log("Enemy Target Pos : " + EnemyMovePos[i]);

                if ((Vector2)E.transform.position == EnemyMovePos[i])
                {
                    Count++;
                }
            }

            Debug.Log(EnemyCount);
            Debug.Log(Count);

            if (Count == EnemyCount)
            {
                Debug.Log("적 내려오기 중지");
                isEnmeyDown = false;
                PlayerisMove = false;
                Destroy(TargetEnemy);
                yield break;
            }

        }
    }

    IEnumerator PlayerMoveToTarget(string InputDirection)
    {
        switch (InputDirection)
        {
            case "up":
                PlayerMovePos = new Vector2(Player.transform.position.x, TargetEnemy.transform.position.y);
                break;
            case "left":
                PlayerMovePos = new Vector2(Player.transform.position.x - 2f, TargetEnemy.transform.position.y);
                break;
            case "right":
                PlayerMovePos = new Vector2(Player.transform.position.x + 2f, TargetEnemy.transform.position.y);
                break;
        }

        Debug.Log("오답");

        float distance = Vector2.Distance(Player.transform.position, PlayerMovePos);

        AttackSpeed = distance / AttackDuration;
        while (true)
        {
            yield return null;
            Player.transform.position = Vector2.MoveTowards(Player.transform.position, PlayerMovePos, AttackSpeed * Time.deltaTime);
            if ((Vector2)Player.transform.position == PlayerMovePos)
            {
                Dead();
                yield break;
            }
        }
    }
    ////////////////////////////////////////
    // prototype 0.0.3 이후 사용 안할것 같음
    ////////////////////////////////////////
    /// 다시 쓰는구만
    IEnumerator PlayerMoveToDown()
    {
        PlayerMovePos = Player.transform.position + Vector3.down * EnemyInterval;

        float distance = Vector2.Distance(Player.transform.position, PlayerMovePos);

        PlayerDownSpeed = distance / PlayerDownDuration;
        while (true)
        {
            yield return null;
            Player.transform.position = Vector2.MoveTowards(Player.transform.position, PlayerMovePos, PlayerDownSpeed * Time.deltaTime);
            if ((Vector2)Player.transform.position == PlayerMovePos)
            {
                PlayerisMove = false;
                yield break;
            }
        }
    }


    ////////////////////////////////////////
    // prototype 0.0.3 이후 사용 안할것 같음
    ////////////////////////////////////////
    /// 다시 쓰는구만
    IEnumerator EnemyMoveToDown()
    {
        yield return null;
        //Debug.Log("문재 부분?");
        isEnmeyDown = true;
        int EnemyCount = Enemies.transform.childCount;

        Debug.Log(EnemyCount);
        Vector2[] EnemyMovePos = new Vector2[EnemyCount];
        float[] EnemyMoveSpeed = new float[EnemyCount];

        for (int i = 0; i < EnemyCount; i++)
        {
            EnemyMovePos[i] = (Vector2)Enemies.transform.GetChild(i).transform.position + Vector2.down * EnemyInterval;
            float distance = Vector2.Distance((Vector2)Enemies.transform.GetChild(i).transform.position, EnemyMovePos[i]); // 움직여야 할 거리 계산
            EnemyMoveSpeed[i] = distance / EnemyDownDuration;
        }

        // 배경 내리기
        Background.GetComponent<BackgroundManager>().MovetoBackground(BackgroundDownDistance, BackgroundDownDuration);

        while (true)
        {
            int Count = 0;

            yield return null;
            for (int i = 0; i < EnemyCount; i++)
            {
                Debug.Log("적 움직임");

                Transform E = Enemies.transform.GetChild(i);
                //Debug.Log("Enemy Number : " + i);

                E.position = Vector2.MoveTowards(E.position, EnemyMovePos[i], EnemyMoveSpeed[i] * Time.deltaTime);
                //Debug.Log("Enemy Target Pos : " + EnemyMovePos[i]);

                if ((Vector2)E.transform.position == EnemyMovePos[i])
                {
                    Count++;
                }
            }

            Debug.Log(EnemyCount);
            Debug.Log(Count);

            if (Count == EnemyCount)
            {
                Debug.Log("적 내려오기 중지");
                isEnmeyDown = false;
                yield break;
            }

        }
    }
}
