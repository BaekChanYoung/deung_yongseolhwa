using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundManager : MonoBehaviour
{


    private void Awake()
    {

    }

    private void Update()
    {

    }

    // 위치를 리셋하는 메서드
    public void MovetoBackground(float Movedistance, float DownDuration)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Debug.Log("배경 내리기");
            transform.GetChild(i).GetComponent<Background>().MoveToDown(Movedistance, DownDuration);
        }
    }
}
