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



public class GameManager : MonoBehaviour
{
    ////////////////////////////////////////
    // 게임 메니저 자기자신을 저장
    ////////////////////////////////////////
    [ReadOnly]
    public static GameManager instance;

    ////////////////////////////////////////
    /// 플레이어 관련
    ////////////////////////////////////////
    [Header("Player")]
    [SerializeField]
    GameObject Player;

    [SerializeField]
    float AttackDuration;

    float AttackSpeed;

    [SerializeField]
    float PlayerDownDuration;

    float PlayerDownSpeed;

    bool PlayerisMove;

    [ReadOnly]
    [SerializeField]
    Vector2 PlayerMovePos;

    bool isDead;

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

    [ReadOnly]
    [SerializeField]
    GameObject TargetEnemy; // 가장 가까운 목표 적

    [SerializeField]
    float EnemyDownDuration;

    [SerializeField]
    float StartSpawnCount;

    float EnemyDownSpeed;

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


        InputDirection = "None";
        RestartMessage.SetActive(false);

        GameSetup();
    }


    void Update()
    {
        if (!isDead)
        {
            FindToTargetEnemy(); // 목표 적 정하기

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
                InputDirection = "None";
                PlayerisMove = true; // 플레이어 움직임
                PlayerMovePos = TargetEnemy.transform.position; // 플레이어가 움직여야할 위치값 저장
                StartCoroutine(PlayerMoveToTarget()); // 플레이어가 목표를 향해서 움직이기 시작
            }
            // 만약 틀린다면
            else if (answerDirection != InputDirection && InputDirection != "None")
            {
                PlayerisMove = true; // 플레이어 움직임

                StartCoroutine(PlayerMoveToTarget(InputDirection));

                InputDirection = "None";
            }
        }

        if (isDead)
        {
            if (Input.GetTouch(0).phase == TouchPhase.Began)
            {
                SceneManager.LoadScene("Prototype");
            }
        }
    }



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
            }
        }

        TargetEnemy = returnEnemy;
    }

    void Dead()
    {
        RestartMessage.SetActive(true);
        isDead = true;
    }

    void GameSetup()
    {
        for (int i = 0; i < StartSpawnCount; i++)
        {
            enemySpawn();
            for (int j = 0; j < Enemies.transform.childCount; j++)
            {
                Enemies.transform.GetChild(j).Translate(Vector2.down * 2f);
            }
        }

    }

    IEnumerator PlayerMoveToTarget()
    {
        Debug.Log("정답");
        float distance = Vector2.Distance(Player.transform.position, PlayerMovePos);

        AttackSpeed = distance / AttackDuration;

        while (true)
        {
            yield return null;
            Player.transform.position = Vector2.MoveTowards(Player.transform.position, PlayerMovePos, AttackSpeed * Time.deltaTime);
            if ((Vector2)Player.transform.position == PlayerMovePos)
            {
                Destroy(TargetEnemy);
                enemySpawn();
                StartCoroutine(EnemyMoveToDown());
                StartCoroutine(PlayerMoveToDown());
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


    IEnumerator PlayerMoveToDown()
    {
        PlayerMovePos = Player.transform.position + Vector3.down * 2f;

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

    IEnumerator EnemyMoveToDown()
    {
        yield return null;
        //Debug.Log("문재 부분?");
        isEnmeyDown = true;
        int EnemyCount = Enemies.transform.childCount;

        Debug.Log(EnemyCount);
        Vector2[] EnemyMovePos = new Vector2[EnemyCount];

        for (int i = 0; i < EnemyCount; i++)
        {
            EnemyMovePos[i] = (Vector2)Enemies.transform.GetChild(i).transform.position + Vector2.down * 2f;
        }

        EnemyDownSpeed = 2f / EnemyDownDuration;

        while (true)
        {
            int Count = 0;

            yield return null;
            for (int i = 0; i < EnemyCount; i++)
            {
                Debug.Log("적 움직임");

                Transform E = Enemies.transform.GetChild(i);
                //Debug.Log("Enemy Number : " + i);

                E.position = Vector2.MoveTowards(E.position, EnemyMovePos[i], EnemyDownSpeed * Time.deltaTime);
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
