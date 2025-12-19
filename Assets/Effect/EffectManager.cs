using UnityEngine;

[System.Serializable]
[SerializeField]
struct effectDate
{
    public ParticleSystem effect;
    public Vector3 Offset;
}

public class EffectManager : MonoBehaviour
{
    [SerializeField]
    effectDate[] attackEffect;
    
    [SerializeField]
    effectDate[] windEffect;

    [SerializeField]
    public Transform spawnPos;

    [SerializeField]
    Transform parent;

    [HideInInspector]
    private float angle;

    public void AttackSlashFX()
    {
        //Debug.Log("이펙트 생성 시작");

        Quaternion e = Quaternion.Euler(0f, 0f, angle);

        for(int i = 0; i < attackEffect.Length; i++)
        {
            Instantiate(attackEffect[i].effect, spawnPos.position + attackEffect[i].Offset, e, parent);
        }
    }

    public void windFX()
    {
        Quaternion e = Quaternion.Euler(0f, 0f, angle);

        for(int i = 0; i < windEffect.Length; i++)
        {
            Instantiate(windEffect[i].effect, spawnPos.position + windEffect[i].Offset, e, parent);
        }
    }

    public void targetAngle()
    {
        Vector2 targetPos = GameManager.instance.enemy.TargetEnemy.transform.position;

        Vector2 startPos = spawnPos.position;

        Vector2 v = targetPos - startPos;

        angle = Mathf.Atan2(v.y, v.x) * Mathf.Rad2Deg;
    
        // Debug.Log("Target : " + targetPos);
        // Debug.Log("spawnPos : " + spawnPos.position);
        // Debug.Log("angle : " + angle);

        Debug.Log("targetAngle : " + angle);
    }
}
