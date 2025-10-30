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
    bool OverResponsiveness = false; // 민감도 초과 확인

    [ReadOnly]
    [SerializeField]
    SwipeDirection InputDirection;

    [SerializeField]
    float TouchResponsiveness; // 스와이프 민감도
    
    [SerializeField]
    Color TouchResponsivenessGizmo;


    //swipe 변수들
    Vector2 firstTouchPos;

    Vector2 endTouchPos;

    [SerializeField]
    bool justPlayerTouch;

    [SerializeField]
    Vector2 PlayerCenter;

    [SerializeField]
    float PlayerRadius; // 플레이어 반지름
    [SerializeField]
    Color PlayerRadiusGizmo;

    void Start()
    {

    }

    void Update()
    {
        if (GameManager.instance.IsCanTouch)
        {
            GameManager.instance.InputDirection = Inputsystem();
        }
    }

    SwipeDirection Inputsystem()
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
            Debug.Log("left");
            InputDirection = SwipeDirection.Left;
        }

        // 우측으로 인식할 시
        if ((GameManager.instance.RightAngle.Min < Inputangle && Inputangle < GameManager.instance.RightAngle.Max) || Input.GetKeyDown(KeyCode.D))
        {
            Debug.Log("right");
            InputDirection = SwipeDirection.Right;
        }

        // 상측으로 인식할 시
        if ((GameManager.instance.UpAngle.Min <= Inputangle && Inputangle <= GameManager.instance.UpAngle.Max) || Input.GetKeyDown(KeyCode.W))
        {
            Debug.Log("up");
            InputDirection = SwipeDirection.Up;
        }

        return InputDirection;
    }

    float? Swipe()
    {


        float? angle = null;
        // 터치를 시작했을때
        if (Input.touchCount > 0 && !isTouch && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            Vector2 touchPos = Input.GetTouch(0).position;

            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(touchPos.x, touchPos.y, 0));

            bool canTouchStart = !justPlayerTouch || PlayerRadius > Vector2.Distance((Vector2)transform.position + PlayerCenter, (Vector2)worldPos);

            Debug.Log(touchPos);

            Debug.Log(Vector2.Distance((Vector2)transform.position + PlayerCenter, worldPos));

            if (canTouchStart)
            {
                firstTouchPos = Input.GetTouch(0).position; // 첫번째 터치 위치 저장

                isTouch = true; // 터치 시작 확인

                //OverResponsiveness = false;
                
                Debug.Log("터치 시작");
            }

            
        }

        // 터치를 하고 있을때
        if (Input.touchCount > 0 && isTouch)
        {
            endTouchPos = Input.GetTouch(0).position; // 터치가 끝날때까지 마지막 터치 위치 저장
            float _TouchResponsiveness = TouchResponsiveness * 100;

            if (TouchResponsiveness < Vector2.Distance(firstTouchPos, endTouchPos))
                OverResponsiveness = true;
            if (TouchResponsiveness > Vector2.Distance(firstTouchPos, endTouchPos))
                OverResponsiveness = false;

            //Debug.Log("터치중");
            Debug.Log("터치 거리 : " + Vector2.Distance(firstTouchPos, endTouchPos));
        }

        // 터치 종료시
        if (Input.touchCount == 0 && isTouch )
        {
            if (OverResponsiveness)
            {
                Vector2 dif = endTouchPos - firstTouchPos; // 스와이프 방향백터 계산 (종료 위치값 - 시작 위치값)

                angle = Mathf.Atan2(dif.y, dif.x) * Mathf.Rad2Deg; // 방향 백터에 따른 스와이프 각도 계산
            }
            
            isTouch = false; //터치 종료 확인

            OverResponsiveness = false;

            // 스와이프 각도 값 반환
            //Debug.Log(angle);
        }
        return angle;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = PlayerRadiusGizmo;
        Gizmos.DrawWireSphere((Vector2)transform.position + PlayerCenter, PlayerRadius);

        Gizmos.color = TouchResponsivenessGizmo;
        Gizmos.DrawWireSphere((Vector2)transform.position + PlayerCenter, TouchResponsiveness);
    }
}
