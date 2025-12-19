using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyIllusionCreater : MonoBehaviour
{
    [SerializeField]
    GameObject illusion;
    
    [SerializeField]
    [Tooltip("환영 생성시 좌우 거리")]
    float Distance;

    [Range(0f,100f)]
    [SerializeField]
    float spawnChancePercent;

    void Awake()
    {
        if(Random.Range(0f, 100f) < spawnChancePercent)
        {
            int i = Random.Range(0, 2);
            if(i == 0) 
                CreatIllusion(Vector2.left);
            if(i == 1)
                CreatIllusion(Vector2.right);
        }
    }


    void CreatIllusion()
    {
        CreatIllusion(Vector2.zero);
    }


    void CreatIllusion(Vector2 Pos)
    {
        Vector2 addPos =  Pos.normalized;
        
        Instantiate(illusion, transform.position + ((Vector3)addPos * Distance), Quaternion.identity, transform);
    }


}

