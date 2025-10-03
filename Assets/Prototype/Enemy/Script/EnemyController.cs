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



    void Awake()
    {
        TargtPos = transform.position;
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

    public void MovetoDown(float distance, float downArrivalTime)
    {
        IsDown = true;

        TargtPos += Vector3.down * distance;

        float moveSpeed = distance / downArrivalTime;

        CancelMove(moveRoutine);

        moveRoutine = StartCoroutine(DownMove(TargtPos, moveSpeed));
    }

    IEnumerator DownMove(Vector3 Pos, float moveSpeed)
    {
        //float StartTime = Time.time;
        while (true)
        {
            transform.position = Vector2.MoveTowards(transform.position, Pos, moveSpeed * Time.deltaTime);

            if (transform.position == Pos)
            {
                // Debug.Log(Time.time - StartTime);
                IsDown = false;

                yield break;
            }

            if (GameManager.instance.isDead)
            {
                yield break;
            }

            yield return null;
        }
    }
    

}
