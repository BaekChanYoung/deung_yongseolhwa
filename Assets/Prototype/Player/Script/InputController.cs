using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public enum SwipeDirection
{
    None,
    Left,
    Right,
    Up
}

public class InputSystem : MonoBehaviour
{
    
    [ReadOnly]
    [SerializeField]
    bool isTouch; // 터치를 하는가?

    [ReadOnly]
    [SerializeField]
    SwipeDirection InputDirection;

    //swipe 변수들
    Vector2 startWorldPos;

    Vector2 endWorldPos;

    [SerializeField]
    float startTouchRange;

    [SerializeField]
    Color startTouchRangeColor;

    [SerializeField]
    float Sensitivity;
    [SerializeField]

    Color SensitivityRangeColor;


    void Start()
    {

    }

    void Update()
    {
        if (GameManager.instance.IsCanTouch)
        {
            switch (GameManager.instance.inputMode)
            {
                case InputMode.AnswerCheckMode:
                    GameManager.instance.InputDirection = answerCheckMode();
                    break;
                case InputMode.AngleCheckMode:
                    //GameManager.instance.InputDirection = angleCheckMode();
                    break;
            } 
        }
    }

    SwipeDirection answerCheckMode()
    {
        float? Inputangle = Swipe(); // 스와이프 인식

        // 스와이프 인식 후 각도에 따른 인식

        if (Inputangle == null)
        {
            InputDirection = SwipeDirection.None;
        }

        // 좌측으로 인식할 시
        if ((GameManager.instance.LeftAngle.Min < Inputangle && Inputangle < GameManager.instance.LeftAngle.Max) || Input.GetKeyDown(KeyCode.A))
        {
            //Debug.Log("left");
            InputDirection = SwipeDirection.Left;
        }

        // 우측으로 인식할 시
        if ((GameManager.instance.RightAngle.Min < Inputangle && Inputangle < GameManager.instance.RightAngle.Max) || Input.GetKeyDown(KeyCode.D))
        {
            //Debug.Log("right");
            InputDirection = SwipeDirection.Right;
        }

        // 상측으로 인식할 시
        if ((GameManager.instance.UpAngle.Min <= Inputangle && Inputangle <= GameManager.instance.UpAngle.Max) || Input.GetKeyDown(KeyCode.W))
        {
            //Debug.Log("up");
            InputDirection = SwipeDirection.Up;
        }

        return InputDirection;
    }

    float? Swipe()
    {
        float? angle = null;

        // 터치를 시작했을때
        if (Input.touchCount > 0 && !isTouch)
        {
            //firstTouchPos = Input.GetTouch(0).position; // 첫번째 터치 위치 저장
            startWorldPos = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);

            //Debug.Log("처음 터치한 좌표 : " + firstTouchPos);
            //Debug.Log("처음 터치한 좌표2 : " + Camera.main.ScreenToWorldPoint(firstTouchPos));
            if (Vector2.Distance(transform.position, startWorldPos) < startTouchRange)
            {
                isTouch = true; // 터치 시작 확인
                
            }
            else
            {
                startWorldPos = Vector2.zero;
            }
        }

        // 터치를 하고 있을때
        if (Input.touchCount > 0 && isTouch)
        {
            //endTouchPos = Input.GetTouch(0).position; // 터치가 끝날때까지 마지막 터치 위치 저장
            endWorldPos = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position);
        }

        // 터치 종료시
        if (Input.touchCount == 0 && isTouch)
        {
            //Debug.Log("시작월드지점 : " + startWorldPos);
            // Debug.Log("종료월드지점 : " + endWorldPos);
            
            //Debug.Log("스와이프 거리 : " + Vector2.Distance(startWorldPos, endWorldPos));
            if (Vector2.Distance(startWorldPos, endWorldPos) > Sensitivity) //
            {
                Vector2 dif = endWorldPos - startWorldPos; // 스와이프 방향백터 계산 (종료 위치값 - 시작 위치값)

                angle = Mathf.Atan2(dif.y, dif.x) * Mathf.Rad2Deg; // 방향 백터에 따른 스와이프 각도 계산
            }
            //Debug.Log(angle);

            isTouch = false; //터치 종료 확인

            // 스와이프 각도 값 반환
            Debug.Log("스와이프 각도 : " + angle);
        }
        return angle;
    }

    SwipeDirection angleCheckMode()
    {
        float? Inputangle = Swipe(); // 스와이프 인식

        Vector3 enemypos = GameManager.instance.enemy.TargetEnemy.transform.position;

        Vector3 prefectAngle = enemypos - transform.position;

        float angle = Mathf.Atan2(prefectAngle.y, prefectAngle.x) * Mathf.Rad2Deg;

        if (Inputangle > angle - GameManager.instance.errorAngleRange && Inputangle < angle + GameManager.instance.errorAngleRange)
        {
            return GameManager.instance.answerDirection;
        }
        else
        {
            switch (GameManager.instance.answerDirection)
            {
                case SwipeDirection.Left:
                    return SwipeDirection.Up;

                case SwipeDirection.Up:
                    if (Inputangle > 90f)
                    {
                        return SwipeDirection.Left;
                    }
                    else
                    {
                        return SwipeDirection.Right;
                    }
                case SwipeDirection.Right:
                    return SwipeDirection.Up;
            }
        }

        return 0;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = startTouchRangeColor;
        Gizmos.DrawWireSphere(transform.position, startTouchRange);
        Gizmos.color = SensitivityRangeColor;
        Gizmos.DrawWireSphere(transform.position, Sensitivity);
    }
}
