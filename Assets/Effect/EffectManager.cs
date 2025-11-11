using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EffectManager : MonoBehaviour
{
    [SerializeField]
    ParticleSystem[] effect;

    [SerializeField]
    public Transform spawnPos;

    [HideInInspector]
    private float angle;

    public void SpawnSlashFX()
    {
        effect[0].Play(true);
    }
    
    public void LockTarget(Vector3 targetPos)
    {
        Vector3 prefectAngle = targetPos - spawnPos.position;

        float angle = Mathf.Atan2(prefectAngle.y, prefectAngle.x) * Mathf.Rad2Deg;
    
        Debug.Log("Target : " + targetPos);
        Debug.Log("spawnPos : " + spawnPos.position);
        Debug.Log("angle : " + angle);

        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}
