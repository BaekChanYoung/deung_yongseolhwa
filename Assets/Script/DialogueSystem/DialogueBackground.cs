using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 배경의 정보를 담기 위한 구조체
/// </summary>
[SerializeField]
[System.Serializable]
struct dialogueBackgroundDate
{
    // 배경 이름
    [Tooltip("배경 이름")]
    public string name;

    // 배경 스프라이트 렌더러
    [Tooltip("배경 스프라이트 렌더러")]
    public SpriteRenderer background;

    // 배경읜 알파값
    [HideInInspector]
    public float alpah;
}

public class DialogueBackground : MonoBehaviour
{
    // 배경 정보 담을 공간 선언
    [SerializeField]
    dialogueBackgroundDate[] backgroundDate;

    // 배경 기본값(이 값을 변경하여서 배경변경)
    [Tooltip("선택한 배경\n(ChangeBackground(), FastChangeBackground()으로 변경 가능)")]
    [SerializeField]
    string selectBackground;
    
    // 배경 변할때 걸리는 시간
    [Tooltip("배경 변할때 걸리는 시간")]
    [SerializeField]
    float changeTime;

    void Start()
    {
        // 시작 시 배경 알파값 적용
        for (int i = 0; i < backgroundDate.Length; i++)
        {
            // 디폴트 배경 변경시 알파값 1 적용
            if(selectBackground == backgroundDate[i].name)
            {
                backgroundDate[i].alpah = 1;
            }
            //디폴트 배경이 아니면 알파값 0 적용
            else
            {
                backgroundDate[i].alpah = 0;
            }
        }
        
        // 알파값에 따른 배영 투명도 조정
        RenderAlpah();
    }

    void Update()
    {
        // changeTime를 이용하여 바뀌는 속도를 계산
        float changeSpeed = 1f / changeTime ;

        // 디폴트 배경 변경시 시간에 따른 변화
        for (int i = 0; i < backgroundDate.Length; i++)
        {
            // 디폴트 배경의 알파값을 올려줌
            if(selectBackground == backgroundDate[i].name)
            {
                backgroundDate[i].alpah += Time.deltaTime * changeSpeed;

                if(backgroundDate[i].alpah > 1)
                    backgroundDate[i].alpah = 1;
            }
            // 디폴트 배경이 아니면 알파값을 내려줌
            else
            {
                backgroundDate[i].alpah -= Time.deltaTime * changeSpeed;

                if(backgroundDate[i].alpah < 0)
                    backgroundDate[i].alpah = 0;
            }
        }

        // 알파값에 따른 배영 투명도 조정
        RenderAlpah();
    }

    //외부에서 호출하는 함수
    // 이 함수를 이용하여 디폴트 배경 변경
    public void ChangeBackground(string backgroundName)
    {
        for (int i = 0; i < backgroundDate.Length; i++)
        {
            if(backgroundName == backgroundDate[i].name)
            {
                selectBackground = backgroundName;
            }
        }
    }

    //외부에서 호출하는 함수
    // 이 함수를 이용하여 디폴트 배경 변경
    // 빠른 배경 전환이 필요할때 사용
    public void FastChangeBackground(string backgroundName)
    {
        for (int i = 0; i < backgroundDate.Length; i++)
        {
            if(backgroundName == backgroundDate[i].name)
            {
                selectBackground = backgroundName;
                backgroundDate[i].alpah = 1;
            }
            else
            {
                backgroundDate[i].alpah = 0;
            }
        }

        // 알파값에 따른 배영 투명도 조정
        RenderAlpah();
    }

    // 알파값에 따른 배영 투명도 조정
    void RenderAlpah()
    {
        for (int i = 0; i < backgroundDate.Length; i++)
        {
            backgroundDate[i].background.color = new Color(1,1,1,backgroundDate[i].alpah);
        }
    }
}
