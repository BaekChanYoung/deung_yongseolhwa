using System.Collections;
using System.Diagnostics.Tracing;
using UnityEngine;

[SerializeField]
[System.Serializable]
struct VibrationSetting
{
    // 움직일때 힘
    public float power;

    // 움직이는 방향
    public Vector2 direction;

    // 움직이는 시간
    public float shakeTime;

    [HideInInspector]
    // 시작 좌표 기억하기
    public Vector3 startPos;
}

[SerializeField]
[System.Serializable]
struct BooShakeSetting
{
        // 움직일때 힘
    public float power;

    // 움직이는 시간
    public float shakeTime;

    [HideInInspector]
    // 시작 좌표 기억하기
    public Vector3 startPos;
}

[SerializeField]
[System.Serializable]
struct ImpulseSetting
{
    // 움직일때 힘
    public float power;
    // 움직임 방향
    public Vector2[] direction;

    // 시작 좌표 기억하기
    public float recoveryTime;
}

[SerializeField]
[System.Serializable]
struct RandomImpulseSetting
{
    // 움직일때 힘
    public float power;

    // 시작 좌표 기억하기
    public float recoveryTime;
}

enum MoveMode
{
    vibration,
    BooSahak,
    Impulse,
    RandomImpulse
}

public class CameraMovement : MonoBehaviour
{
    [SerializeField]
    MoveMode mode;
    
    [SerializeField]
    VibrationSetting vibration ;
    
    [SerializeField]
    BooShakeSetting booShake ;

    [SerializeField]
    ImpulseSetting impulse;
    
    [SerializeField]
    RandomImpulseSetting randomImpulse;
    


    Vector3 startPos;

    Coroutine Move;

    void Start()
    {
        // 시작위치 저장
        startPos = transform.position;
    }

    public void ShakeMove()
    {
        CancelMove(Move);
        switch(mode)
        {
            case MoveMode.vibration:
                Move = StartCoroutine(vibrationCoroutine(startPos, vibration.direction, vibration.power, vibration.shakeTime));
                break;
            case MoveMode.BooSahak:
                Move = StartCoroutine(BooSahakCoroutine(startPos, booShake.power, booShake.shakeTime));
                break;
            case MoveMode.Impulse:
                int i = Random.Range(0,impulse.direction.Length);
                Move = StartCoroutine(ImpulseCoroutine(startPos, impulse.direction[i], impulse.power, impulse.recoveryTime));
                break;
            case MoveMode.RandomImpulse:
                Move = StartCoroutine(RandomImpulseCoroutine(startPos,randomImpulse.power, randomImpulse.recoveryTime));
            break;
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

    IEnumerator vibrationCoroutine(Vector3 StartPos, Vector3 direction, float power, float shakeTime)
    {
        //Debug.Log("흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들");
        float t = 0f;

        while(true)
        {
            transform.position = StartPos + (direction.normalized * power * Time.unscaledDeltaTime);

            power *= -1;

            t += Time.unscaledDeltaTime;

            if(t > shakeTime)
            {
                break;
            }
            
            yield return null;
        }

        transform.position = StartPos;

        yield break;
    }

    IEnumerator BooSahakCoroutine(Vector3 StartPos, float power, float shakeTime)
    {
        //Debug.Log("흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들");
        float t = 0f;

        while(true)
        {
            transform.position = StartPos + new Vector3(Random.Range(-1, 1f), Random.Range(-1, 1f), 0).normalized * power;

            power -= Time.unscaledDeltaTime;

            t += Time.unscaledDeltaTime;

            if(t > shakeTime)
            {
                break;
            }
            
            yield return null;
        }

        transform.position = StartPos;

        yield break;
    }

    IEnumerator ImpulseCoroutine(Vector3 StartPos, Vector2 direction , float power, float recoveryTime)
    {
        //Debug.Log("흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들");
        Vector3 SDR = Vector3.zero;

        float d = 0f;

        transform.position = StartPos + new Vector3(direction.x, direction.y, 0).normalized * power;

        float speed = Vector2.Distance(StartPos, transform.position) / recoveryTime;

        yield return null;
        
        while(true)
        {
            //transform.position = Vector3.SmoothDamp(transform.position, StartPos, ref SDR,  recoveryTime * Time.unscaledDeltaTime);
            transform.position = Vector3.MoveTowards(transform.position, StartPos, speed * Time.unscaledDeltaTime);

            d = Vector2.Distance(StartPos, transform.position);

            if(d < 0.0001f)
            {
                break;
            }

            yield return null;
        }

        transform.position = StartPos;

        yield break;
    }

    IEnumerator RandomImpulseCoroutine(Vector3 StartPos, float power, float recoveryTime)
    {
        //Debug.Log("흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들흔들");
        Vector3 SDR = Vector3.zero;

        float d = 0f;

        transform.position = StartPos + new Vector3(Random.Range(-1, 1f), Random.Range(-1, 1f), 0).normalized * power;

        float speed = Vector2.Distance(StartPos, transform.position) / recoveryTime;

        yield return null;
        
        while(true)
        {
            //transform.position = Vector3.SmoothDamp(transform.position, StartPos, ref SDR,  recoveryTime * Time.unscaledDeltaTime);
            transform.position = Vector3.MoveTowards(transform.position, StartPos, speed * Time.unscaledDeltaTime);

            d = Vector2.Distance(StartPos, transform.position);

            if(d < 0.0001f)
            {
                break;
            }

            yield return null;
        }

        transform.position = StartPos;

        yield break;
    }
}
