using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager2 : MonoBehaviour
{
    // 게임매니저 자기자신
    public static GameManager2 instance;
    [SerializeField]
    GameObject InputSystem;


    [SerializeField]
    GameObject Enemies;

    GameObject TargetEnemy;

    [SerializeField]
    GameObject Player;

    [SerializeField]
    float killSpeed;

    bool iskill;

    [ReadOnly][SerializeField]
    public string answerDirection;



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
        //Enemies.GetComponent<Enemy_Spawn>().enemySpawn();
        allEnemyMoveToDown();

    }


    // Update is called once per frame
    void Update()
    {
        FindToTargetEnemy();

        float enemy_x = TargetEnemy.transform.position.x;
        float Player_x = Player.transform.position.x;

        // 정답 정하기
        if (enemy_x == Player_x)
        {
            answerDirection = "up";
        }

        if (enemy_x < Player_x)
        {
            answerDirection = "left";
        }

        if (enemy_x > Player_x)
        {
            answerDirection = "right";
        }

        if (iskill)
        {
            if (Player.transform.position == TargetEnemy.transform.position)
            {
                Destroy(TargetEnemy);
                allEnemyMoveToDown();
            }
        }

        if (InputSystem.GetComponent<Input_System2>().isMove)
        {
            if (Player.transform.position.y <= -4f)
            {
                InputSystem.GetComponent<Input_System2>().isMove = false;
            }
        }
    }

    void FindToTargetEnemy()
    {
        GameObject returnEnemy = Enemies.transform.GetChild(0).gameObject;
        for (int i = 1; i < Enemies.transform.childCount; i++)
        {
            if (returnEnemy.transform.position.y > Enemies.transform.GetChild(i).transform.position.y)
            {
                returnEnemy = Enemies.transform.GetChild(i).gameObject;
            }
        }

        TargetEnemy = returnEnemy;
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
            Enemies.transform.GetChild(i).GetComponent<Enemy2>().moveToDown();
        }
        Player.GetComponent<Player>().moveToDown();
    }

    public void killEnemy()
    {
        Player.GetComponent<Player>().movetoTarget(TargetEnemy);

        iskill = true;
    }
}
