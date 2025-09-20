using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    float PlayerSpeed;

    bool isMovetoTarget;

    Vector2 enemeyTargetPos;

    void Start()
    {
        isMovetoTarget = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isMovetoTarget)
        {
            transform.position = Vector2.MoveTowards(transform.position, enemeyTargetPos, PlayerSpeed * Time.deltaTime);
            if ((Vector2)transform.position == enemeyTargetPos)
            {
                isMovetoTarget = false;
            }
        }
    }

    public void movetoTarget(GameObject E)
    {
        isMovetoTarget = true;
        enemeyTargetPos = E.transform.position;
    }

    public void moveToDown()
    {
        isMovetoTarget = true;
        enemeyTargetPos = (Vector2)transform.position + Vector2.down * 2f;
    }
}
