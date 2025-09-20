using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class Input_System2 : MonoBehaviour
{
    [SerializeField]
    GameObject Player;

    [SerializeField]
    float2 Leftangle;
    [SerializeField]
    float2 Upangle;
    [SerializeField]
    float2 Rightangle;

    [ReadOnly]
    [SerializeField]
    string InputDirection;

    // 터치 관련 변수
    bool isTouch;

    Vector2 firstTouchPos;

    Vector2 endTouchPos;

    float Inputangle;

    // 이동 후 다시 원래 Y높이로 내려올때 필여한 변수들
    public bool isMove; // 움직였는지, 움직이고 있는지

    Vector2 downTargetPos;

    void Start()
    {
        isTouch = false;
        isMove = false;
        
        InputDirection = "None";
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log(Input.touchCount);
        Inputangle = 0f;

        Swipe1();

        // 입력한 각도에 따라서 이동
        if (!isMove)
        {
            if ((Leftangle.x < Inputangle && Inputangle < Leftangle.y) || Input.GetKeyDown(KeyCode.A))
            {
                Debug.Log("left");
                InputDirection = "left";
            }

            if ((Upangle.x < Inputangle && Inputangle < Upangle.y) || Input.GetKeyDown(KeyCode.W))
            {
                Debug.Log("up");
                InputDirection = "up";
            }

            if ((Rightangle.x < Inputangle && Inputangle < Rightangle.y) || Input.GetKeyDown(KeyCode.D))
            {
                Debug.Log("right");
                InputDirection = "right";
            }

            //만약 정답이면
            if (GameManager2.instance.answerDirection == InputDirection)
            {
                //Player.GetComponent<Player>().movetoTarget();
                GameManager2.instance.killEnemy();
                InputDirection = "None";
                isMove = true;
            }
        }
    }   



    void Swipe1()
    {
        // 최초 터치시
        if (Input.touchCount > 0 && !isTouch)
        {
            firstTouchPos = Input.GetTouch(0).position; // 첫번째 터치 위치 저장

            isTouch = true;
        }

        if (Input.touchCount > 0 && isTouch)
        {
            endTouchPos = Input.GetTouch(0).position; // 터치가 끝날때까지 마지막 터치 위치 저장
        }
        if (Input.touchCount == 0 && isTouch)
        {
            Vector2 dif = endTouchPos - firstTouchPos;

            float angle = Mathf.Atan2(dif.y, dif.x) * Mathf.Rad2Deg;

            Debug.Log(angle);

            isTouch = false;

            Inputangle = angle;
        }
        
    }
}
