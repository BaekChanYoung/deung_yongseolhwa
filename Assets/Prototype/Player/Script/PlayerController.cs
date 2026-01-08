using System.Collections;
using Spine.Unity;
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

    Coroutine waiiRoutine;

    [SerializeField]
    public GameObject SpriteAni;
    private Animator SpriteAnimation;

    [SerializeField]
    GameObject SpineAnimation;
    private SkeletonAnimation SpineSkeleton;

    bool IsOnSpine;

    

    enum PlayerAniClip
    {
        idle = 0,
        slash,
        stab,
        Length
    }

    void Start()
    {
        SpriteAnimation = SpriteAni.GetComponent<Animator>();
        SpineSkeleton = SpineAnimation.GetComponent<SkeletonAnimation>();
        IsOnSpine = true;

        SpineSkeleton.AnimationState.SetAnimation(1,"base",true);

        OnSpine();
    }

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

    }

    public void moveToPos(Vector2 targetPos, float arrivalTime)
    {
        float distance;
        float moveSpeed;
        if (!IsDown)
        {
            distance = Vector2.Distance(transform.position, targetPos); // 공격시 이동 거리 계산

            moveSpeed = distance / arrivalTime; // 거리와 이동 수행 시간을 기준로 이동 속도 계산
        }
        else
        {
            IsDown = false;
            //Pos = target.GetComponent<EnemyController>().TargtPos;

            distance = Vector2.Distance(transform.position, targetPos); // 공격시 이동 거리 계산

            moveSpeed = distance / arrivalTime; // 거리와 이동 수행 시간을 기준로 이동 속도 계산
        }

        CancelMove(moveRoutine);

        moveRoutine = StartCoroutine(FollowPos(targetPos, moveSpeed));
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
    IEnumerator FollowPos(Vector3 targetPos, float moveSpeed)
    {
        //float StartTime = Time.time;
        while (true)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

            if (transform.position == targetPos)
            {
                //IsAttack = false;
                //Debug.Log(Time.time - StartTime);
                //GameManager.instance.AttackSuccess();
                Dead();

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

    #region Animation
    public void AnimationCalculationProcessing(SwipeDirection Direction)
    {
        IsOnSpine = false;

        OnSpine();

        switch(Direction)
        {
            case SwipeDirection.Left:
                SpriteAnimation.SetTrigger("GoLeft");
                break;
            case SwipeDirection.Up:
                SpriteAnimation.SetTrigger("GoUp");
                break;
            case SwipeDirection.Right:
                SpriteAnimation.SetTrigger("GoRight");
                break;
        }

        int Attack = Random.Range(1, (int)PlayerAniClip.Length);
        
        switch (Attack)
        {       
            case (int)PlayerAniClip.stab:
                SpriteAnimation.SetTrigger("Stab");
                break;
                
            case (int)PlayerAniClip.slash:
                SpriteAnimation.SetTrigger("Slash");
                break;
        }
    }


    // 공격이 적중 시 호출
    public void AttackIsHit(bool IsHit)
    {
        if (!IsHit)
            return;
        else
        {
            SpriteAnimation.SetTrigger("Hit");
            waitAni();
        }
    }

    void OnSpine()
    {
        SpineAnimation.SetActive(IsOnSpine);
        SpriteAni.SetActive(!IsOnSpine);
        SpriteAnimation.SetBool("OnSpine",IsOnSpine);
    }

    void waitAni()
    {
        CancelMove(waiiRoutine);

        waiiRoutine = StartCoroutine(WaitAnimation());
    }

    IEnumerator WaitAnimation()
    {
        yield return new WaitForSecondsRealtime(GameManager.instance.hitSlowEffect.HitStopTime);
        yield return new WaitForSecondsRealtime(GameManager.instance.hitSlowEffect.HitSlowTime);
        IsOnSpine = true;
        OnSpine();
    }

    #endregion

    public void Dead()
    {
        //Debug.Log("start Dead1");
        CancelMove(moveRoutine);

        moveRoutine = StartCoroutine(DeadMove());
    }

    IEnumerator DeadMove()
    {
        //Debug.Log("start Dead2");
        yield return new WaitForSecondsRealtime(1f);

        //SpriteAnimationController.SetTrigger("Dead");

        //yield return new WaitForSecondsRealtime(0.5f);

        //GameManager.instance.Dead();

        deadDrop();

        yield break;
    }

    void deadDrop()
    {
        CancelMove(moveRoutine);

        moveRoutine = StartCoroutine(deadDropMove());
    }

    IEnumerator deadDropMove()
    {
        float timer = 0;
        Vector2 Pos = (Vector2)transform.position + Vector2.down * 100;
        while(true)
        {
            transform.position = Vector2.MoveTowards(transform.position, Pos, 50 * Time.deltaTime);

            if (timer > GameManager.instance.DeadFallTime)
            {
                GameManager.instance.Dead();
                yield break;
            }

            timer += Time.deltaTime;
            yield return null;
        }
    }
}