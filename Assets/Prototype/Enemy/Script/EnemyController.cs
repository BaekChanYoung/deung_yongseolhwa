using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [ReadOnly]
    public bool IsDown;

    [ReadOnly]
    public Vector3 TargtPos;

    Coroutine moveRoutine;

    Animator Ani;

    [SerializeField]
    GameObject deadFX;

    void Awake()
    {
        TargtPos = transform.position;
        Ani = GetComponent<Animator>();
    }

    void CancelMove(Coroutine ct)
    {
        if (ct != null)
        {
            StopCoroutine(ct);
            ct = null;
        }
    }

    public void rushToDown(float arrivalTime)
    {
        float distance = Vector2.Distance(transform.position, TargtPos);

        float moveSpeed = distance / arrivalTime;

        CancelMove(moveRoutine);

        moveRoutine = StartCoroutine(DownMove(TargtPos, moveSpeed));
    }

    public void moveToDown(float distance, float downArrivalTime, DownMovementMode DownMode = DownMovementMode.moveToword)
    {
        IsDown = true;

        TargtPos += Vector3.down * distance;

        if (DownMode == DownMovementMode.moveToword)
        {
            float moveSpeed = distance / downArrivalTime;

            CancelMove(moveRoutine);

            moveRoutine = StartCoroutine(DownMove(TargtPos, moveSpeed, DownMode));
        }

        if (DownMode == DownMovementMode.SmoothDamp)
        {
            moveRoutine = StartCoroutine(DownMove(TargtPos, downArrivalTime, DownMode));
        }
    }

    IEnumerator DownMove(Vector3 Pos, float moveSpeed, DownMovementMode DownMode = DownMovementMode.moveToword)
    {
        //SmoothDamp를 위한 vecter 선언(왜 쓰는지 모름)
        Vector2 SDR = Vector2.zero;

        //float StartTime = Time.time;
        while (true)
        {
            // MovwToWard 모드
            if (DownMode == DownMovementMode.moveToword)
            {
                transform.position = Vector2.MoveTowards(transform.position, Pos, moveSpeed * Time.deltaTime);

                if (transform.position == Pos)
                {
                    // Debug.Log(Time.time - StartTime);
                    IsDown = false;

                    yield break;
                }

                if (GameManager.instance.player.isDead)
                {
                    yield break;
                }
            }

            if (DownMode == DownMovementMode.SmoothDamp)
            {
                transform.position = Vector2.SmoothDamp(transform.position, Pos, ref SDR, moveSpeed * Time.deltaTime); // ref? 이해는 되는데 외 쓰는지는 잘 모르겠다.

                if (Vector2.Distance(transform.position, Pos) <= 0.1f)
                {
                    IsDown = false;
                    // Debug.Log(Time.time - StartTime);
                    yield break;
                }
                if (GameManager.instance.player.isDead)
                {
                    yield break;
                }
            }

            yield return null;
        }
    }

    public void Dead()
    {
        Debug.Log("적 죽음 실행");
        StartCoroutine(deadEffect());
    }

    IEnumerator deadEffect()
    {
        Debug.Log("적 죽음 코루틴 실행");

        Ani.SetTrigger("Hit");

        yield return new WaitForSecondsRealtime(GameManager.instance.hitSlowEffect.HitStopTime);
        
        yield return new WaitForSecondsRealtime(GameManager.instance.hitSlowEffect.HitSlowTime);

        Instantiate(deadFX, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
    

}
