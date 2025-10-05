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

    public void moveToDown(float distance, float downArrivalTime)
    {
        IsDown = true;

        Vector3 MovePos = transform.position + Vector3.down * distance;

        float moveSpeed = distance / downArrivalTime;

        CancelMove(moveRoutine);

        moveRoutine = StartCoroutine(DownMove(MovePos, moveSpeed));
    }

    public void moveToDown(float distance, float downArrivalTime, float reboundPower)
    {
        IsDown = true;

        Vector3 MovePos = transform.position + Vector3.down * distance;

        float moveSpeed = distance / downArrivalTime;

        CancelMove(moveRoutine);

        moveRoutine = StartCoroutine(DownMove(MovePos, moveSpeed));

        CancelMove(reboundRoutine);

        reboundRoutine = StartCoroutine(reboundMove(reboundPower));
    }

    IEnumerator DownMove(Vector3 Pos, float moveSpeed)
    {
        //float StartTime = Time.time;
        while (true)
        {
            transform.position = Vector2.MoveTowards(transform.position, Pos, moveSpeed * Time.deltaTime);

            if (transform.position == Pos)
            {
                IsDown = false;
                // Debug.Log(Time.time - StartTime);
                yield break;
            }
            if (GameManager.instance.isDead)
            {
                yield break;
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
