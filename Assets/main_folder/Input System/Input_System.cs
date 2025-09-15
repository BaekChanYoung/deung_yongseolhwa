using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class Input_System : MonoBehaviour
{
    [SerializeField]
    GameObject Player;

    [SerializeField]
    float2 Leftangle;
    [SerializeField]
    float2 Upangle;
    [SerializeField]
    float2 Rightangle;

    [SerializeField]
    float playerDownSpeed;

    // 터치 관련 변수
    bool isTouch;

    Vector2 firstTouchPos;

    Vector2 endTouchPos;

    float Inputangle;

    // 이동 후 다시 원래 Y높이로 내려올때 필여한 변수들
    bool isMove; // 움직였는지, 움직이고 있는지

    Vector2 downTargetPos; 

    void Start()
    {
        isTouch = false;
        isMove = false;
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
            if (Leftangle.x < Inputangle && Inputangle < Leftangle.y)
            {
                Debug.Log("left");
                Player.transform.position = new Vector2(-2f, -2f);
                isMove = true;
            }

            if (Upangle.x < Inputangle && Inputangle < Upangle.y)
            {
                Debug.Log("up");
                Player.transform.position = new Vector2(0f, -2f);
                isMove = true;
            }

            if (Rightangle.x < Inputangle && Inputangle < Rightangle.y)
            {
                Debug.Log("right");
                Player.transform.position = new Vector2(2f, -2f);
                isMove = true;
            }

            if (Input.GetKeyDown(KeyCode.W))
            {
                Debug.Log("up");
                Player.transform.position = new Vector2(0f, -2f);
                isMove = true;
            }

            if (Input.GetKeyDown(KeyCode.A))
            {
                Debug.Log("up");
                Player.transform.position = new Vector2(-2f, -2f);
                isMove = true;
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                Debug.Log("up");
                Player.transform.position = new Vector2(2f, -2f);
                isMove = true;
            }

            if (isMove)
            {
                downTargetPos = new Vector2(Player.transform.position.x, Player.transform.position.y - 2f);
                GameManager.instance.allEnemyMoveToDown();
            }
        }

        

        // 입력 후 다시 내려오기
            if (isMove)
            {
                Player.transform.position = Vector2.MoveTowards(Player.transform.position, downTargetPos, playerDownSpeed * Time.deltaTime);

                if (downTargetPos == (Vector2)Player.transform.position)
                {
                    isMove = false;
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
