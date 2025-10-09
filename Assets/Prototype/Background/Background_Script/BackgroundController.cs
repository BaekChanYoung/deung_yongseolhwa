using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class BackgroundController  : MonoBehaviour
{
    //bool IsDown;

    Vector3 TargetPos;

    float height;

    Coroutine DownRoutine;


    private void Awake()
    {
        // 가로 길이를 측정하는 처리
        BoxCollider2D BackgroundCollider = GetComponent<BoxCollider2D>();

        height = BackgroundCollider.size.y;
    }

    void Start()
    {
        TargetPos = transform.position;
    }

    void Update()
    {
        if (transform.position.y <= -height)
        {
            Reposition();
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


    
    
    public void rushToDown(float DownDuration, DownMovementMode DownMode = DownMovementMode.moveToword)
    {
        if (!(DownMode == DownMovementMode.moveToword))
        {
            float distance = Vector2.Distance((Vector2)transform.position, TargetPos);

            float DownSpeed = distance / DownDuration;
            //IsDown = true;

            CancelMove(DownRoutine);

            DownRoutine = StartCoroutine(DownMove(distance, DownSpeed));
        }
    }

    public void moveToDown(float Movedistance, float DownDuration, DownMovementMode DownMode = DownMovementMode.moveToword)
    {

        TargetPos += Vector3.down * Movedistance;

        float distance = Vector2.Distance((Vector2)transform.position, TargetPos);

        if (DownMode == DownMovementMode.moveToword)
        {
            float DownSpeed = distance / DownDuration;

            CancelMove(DownRoutine);

            DownRoutine = StartCoroutine(DownMove(distance, DownSpeed, DownMode));
        }

        if (DownMode == DownMovementMode.SmoothDamp)
        {
            DownRoutine = StartCoroutine(DownMove(distance, DownDuration, DownMode));
        }
    }

    IEnumerator DownMove(float distance, float DownSpeed, DownMovementMode DownMode = DownMovementMode.moveToword)
    {
        //SmoothDamp를 위한 vecter 선언(왜 쓰는지 모름)
        Vector2 SDR = Vector2.zero;

        while (true)
        {
            if (DownMode == DownMovementMode.moveToword)
            {
                //Debug.Log("배경모드 : moveToword");
                transform.position = Vector2.MoveTowards(transform.position, TargetPos, DownSpeed * Time.deltaTime);

                if (transform.position == TargetPos || GameManager.instance.player.isDead)
                {
                    //Debug.Log("배경 도착");
                    //IsDown = false;
                    yield break;
                }
            }

            if (DownMode == DownMovementMode.SmoothDamp)
            {
                //Debug.Log("배경모드 : SmoothDamp");
                transform.position = Vector2.SmoothDamp(transform.position, TargetPos, ref SDR, DownSpeed * Time.deltaTime); // ref? 이해는 되는데 외 쓰는지는 잘 모르겠다.

                if (transform.position == TargetPos || GameManager.instance.player.isDead)
                {
                    // Debug.Log(Time.time - StartTime);
                    yield break;
                }
            }

            yield return null;
        }
    }

    void Reposition()
    {
        float P = transform.parent.transform.childCount;
        
        Vector3 offset = new Vector3(0, height * P, 0);
        transform.position += offset;
        TargetPos += offset;
    }
}
