using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [ReadOnly]
    [SerializeField]
    Vector2 AttackDirection; // 공격시 방향백터 저장

    //bool IsAttack;

    [HideInInspector]
    public bool IsDown;

    Coroutine moveRoutine;

    Coroutine reboundRoutine;

    void CancelMove(Coroutine ct)
    {
        if (ct != null)
        {
            StopCoroutine(ct);
            ct = null;
        }
    }

    public void moveToTarget(GameObject target, float arrivalTime)
    {
        //IsAttack = true;

        AttackDirection = (target.transform.position - transform.position).normalized; // 공격 방향 벡터 정규화 후 저장
        float distance;
        float moveSpeed;

        Vector2 Pos;
        if (!IsDown)
        {
            Pos = target.transform.position;

            distance = Vector2.Distance(transform.position, Pos); // 공격시 이동 거리 계산

            moveSpeed = distance / arrivalTime; // 거리와 이동 수행 시간을 기준로 이동 속도 계산
        }
        else
        {
            IsDown = false;
            Pos = target.GetComponent<EnemyController>().TargtPos;

            distance = Vector2.Distance(transform.position, target.GetComponent<EnemyController>().TargtPos); // 공격시 이동 거리 계산

            moveSpeed = distance / arrivalTime; // 거리와 이동 수행 시간을 기준로 이동 속도 계산
        }

        CancelMove(moveRoutine);

        moveRoutine = StartCoroutine(FollowObject(Pos, moveSpeed));

        CancelMove(reboundRoutine);
    }


    IEnumerator FollowObject(Vector3 targetPos, float moveSpeed)
    {
        //float StartTime = Time.time;
        while (true)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

            if (transform.position == targetPos)
            {
                //IsAttack = false;
                //Debug.Log(Time.time - StartTime);
                GameManager.instance.AttackSuccess();

                yield break;
            }

            yield return null;
        }
    }

    public void moveToDown(float distance, float downArrivalTime, DownMovementMode DownMode = DownMovementMode.moveToword)
    {
        IsDown = true;

        Vector3 TargtPos = transform.position + Vector3.down * distance;

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

    // 리바운드 사용시(근대 안쓸듯?)
    public void moveToDown(float distance, float downArrivalTime, float reboundPower, DownMovementMode DownMode = DownMovementMode.moveToword)
    {
        IsDown = true;

        Vector3 MovePos = transform.position + Vector3.down * distance;

        float moveSpeed = distance / downArrivalTime;

        CancelMove(moveRoutine);

        moveRoutine = StartCoroutine(DownMove(MovePos, moveSpeed));

        CancelMove(reboundRoutine);

        //reboundRoutine = StartCoroutine(reboundMove(reboundPower));
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
                    IsDown = false;
                    // Debug.Log(Time.time - StartTime);
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

                if (transform.position == Pos)
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

    IEnumerator reboundMove(float reboundPower)
    {
       while (true)
        {
            transform.position = Vector2.MoveTowards(transform.position, (Vector2)transform.position + AttackDirection, reboundPower * Time.deltaTime);

            AttackDirection += Vector2.down * 0.98f * Time.deltaTime;

            yield return null;
        }
    }

}
