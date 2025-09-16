using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy2 : MonoBehaviour
{

    bool isMove;

    Vector2 downTargetPos;

    [SerializeField]
    float DownSpeed;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("working");
        if (isMove)
        {
            transform.position = Vector2.MoveTowards(transform.position, downTargetPos, DownSpeed * Time.deltaTime);

            if (downTargetPos == (Vector2)transform.position)
            {
                isMove = false;
            }
        }
    }

    void Dead()
    {
        Destroy(gameObject);
    }

    public void moveToDown()
    {
        isMove = true;

        downTargetPos = new Vector2(transform.position.x, transform.position.y - 2f);
    }
}
