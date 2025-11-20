using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    [SerializeField]
    GameObject[] attackEffect;
    
    [SerializeField]
    ParticleSystem[] windEffect;

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
            Instantiate(attackEffect[i], spawnPos.position, e, parent);
        }
    }

    public void windFX()
    {
        Quaternion e = Quaternion.Euler(0f, 0f, angle);

        for(int i = 0; i < attackEffect.Length; i++)
        {
            Instantiate(windEffect[i], spawnPos.position, e, parent);
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
