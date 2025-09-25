using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Background : MonoBehaviour
{
    bool isMove;

    Vector2 ToMovePos;

    float DownSpeed;

    float height;
    private void Awake()
    {
        // 가로 길이를 측정하는 처리
        BoxCollider2D BackgroundCollider = GetComponent<BoxCollider2D>();

        height = BackgroundCollider.size.y;
    }

    void Start()
    {
        ToMovePos = transform.position;
    }

    void Update()
    {
        if (transform.position.y <= -height * 10)
        {
            Reposition();
        }
    }

    public IEnumerator Move()
    {
        yield return null;
        isMove = true;

        while (true)
        {
            yield return null;
            transform.position = Vector2.MoveTowards(transform.position, ToMovePos, DownSpeed * Time.deltaTime);
            if ((Vector2)transform.position == ToMovePos)
            {
                isMove = false;
                yield break;
            }

            if (GameManager.instance.isDead)
                yield break;
        }
    }

    public void MoveToDown(float Movedistance, float DownDuration)
    {
        
        ToMovePos += Vector2.down * Movedistance;

        float distance = Vector2.Distance((Vector2)transform.position, ToMovePos);

        DownSpeed = distance / DownDuration;

        if (!isMove)
        {
            StartCoroutine(Move());
        }
    }

    public void Reposition()
    {
        float P = transform.parent.transform.childCount;

        Vector2 offset = new Vector2(0, height * P * 10);
        transform.position += (Vector3)offset;
        ToMovePos += offset;
    }

}
