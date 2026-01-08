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
        spawnChancePercent = GameManager.instance.EnemyIllusionChancePercent();
    }

    void Start()
    {
        if(Random.Range(0f, 100f) < spawnChancePercent)
        {
            CreatIllusion();
        }
    }


    void CreatIllusion()
    {
        float x = transform.position.x;

        if(x == 0f)
        {
            int i = Random.Range(0, 2);
            if(i == 0) 
                CreatIllusion(Vector2.left);
            if(i == 1)
                CreatIllusion(Vector2.right);
        }
        else
        {
            if(x > 0f)
            {
                CreatIllusion(Vector2.left);
            }
            if(x < 0f)
            {
                CreatIllusion(Vector2.right);
            }
        }
    }


    void CreatIllusion(Vector2 Pos)
    {
        Vector2 addPos =  Pos.normalized;
        
        Instantiate(illusion, transform.position + ((Vector3)addPos * Distance), Quaternion.identity, transform);
    }
}

