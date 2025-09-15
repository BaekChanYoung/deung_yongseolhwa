using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // 게임매니저 자기자신
    public static GameManager instance;

    [SerializeField]
    GameObject Enemies;

    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("씬에 두개 이상의 게임 매니저가 존재합니다!");
            Destroy(gameObject);
        }
    }


    // Update is called once per frame
    void Update()
    {

    }

    public void allEnemyMoveToDown()
    {
        Enemies.GetComponent<Enemy_Spawn>().enemySpawn();
        
        Debug.Log("enemyDown");

        int count = Enemies.transform.childCount;

        Debug.Log("Enemy :  " + count);

        for (int i = 0; i < count; i++)
        {
            Debug.Log("enemyDown : " + i);
            Enemies.transform.GetChild(i).GetComponent<Enemy>().moveToDown();
        }
    }
    
    
}
