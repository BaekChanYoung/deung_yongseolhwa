using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Background : MonoBehaviour
{
    //bool IsDown;

    Vector2 TargetPos;

    float DownSpeed;

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


    
    
    public void rushToDown(float DownDuration)
    {
        float distance = Vector2.Distance((Vector2)transform.position, TargetPos);

        DownSpeed = distance / DownDuration;
        //IsDown = true;

        CancelMove(DownRoutine);

        DownRoutine = StartCoroutine(DownMove());
    }

    public void MoveToDown(float Movedistance, float DownDuration)
    {

        TargetPos += Vector2.down * Movedistance;

        float distance = Vector2.Distance((Vector2)transform.position, TargetPos);

        DownSpeed = distance / DownDuration;

        CancelMove(DownRoutine);

        DownRoutine = StartCoroutine(DownMove());
    }

    IEnumerator DownMove()
    {
        while (true)
        {
            transform.position = Vector2.MoveTowards(transform.position, TargetPos, DownSpeed * Time.deltaTime);

            if ((Vector2)transform.position == TargetPos)
            {
                //Debug.Log("배경 도착");
                //IsDown = false;
                yield break;
            }

            if (GameManager.instance.isDead)
            {
                //Debug.Log("왜죽었어");
                yield break;
            }

            yield return null;
        }
    }

    void Reposition()
    {
        float P = transform.parent.transform.childCount;
        
        Vector2 offset = new Vector2(0, height * P);
        transform.position += (Vector3)offset;
        TargetPos += offset;
    }
}
