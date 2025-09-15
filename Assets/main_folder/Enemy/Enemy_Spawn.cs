using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Spawn : MonoBehaviour
{
    Transform leftLine;
    Transform upLine;
    Transform rightLine;



    [SerializeField]
    GameObject enemyline;

    [SerializeField]
    GameObject Enemy;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void enemySpawn()
    {
        Debug.Log("소환");
        int Line = Random.Range(0, 3);

        Instantiate(Enemy, enemyline.transform.GetChild(Line).position, Quaternion.identity, transform);
    }
}
