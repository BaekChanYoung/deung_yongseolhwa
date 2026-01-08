using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TimerBarController : MonoBehaviour
{
    [ReadOnly]
    [Header("Timer Setting")]
    [Tooltip("타이머 총 시간 (초)")]
    public float maxTime;

    [ReadOnly]
    public float StartTime;

    [Header("UI Setting")]
    [Tooltip("채워질 진행바 이미지")]
    public Image fillBar;

    [Tooltip("타이머 텍스트 (선택사항)")]
    public TextMeshProUGUI timerText;

    [Header("옵션")]
    [Tooltip("시작 시 자동으로 타이머 시작")]
    public bool autoStart = true;

    [Tooltip("타이머 종료 시 자동으로 재시작")]
    public bool loop = false;

    [Tooltip("시간에 따라 색상 변화")]
    public bool useColorChange = true;

    // 현재 남은 시간
    [ReadOnly]
    [SerializeField]
    float currentTime;

    // 시간 배속 값
    private float timerScale;

    // 타이머 동작 여부
    private bool isRunning = false;


    void Start()
    {

        maxTime = GameManager.instance.MaxTime;

        StartTime = GameManager.instance.startTime;

        currentTime = StartTime;
        // 초기 시간 설정

        if (maxTime < currentTime)
            currentTime = maxTime;
            

        // Fill Bar가 할당되지 않았다면 자동으로 찾기 시도
        if (fillBar == null)
        {
            fillBar = transform.Find("FillBar").GetComponent<Image>();

            if (fillBar == null)
            {
                Debug.LogError("FillBar Image를 찾을 수 없습니다!");
                return;
            }
        }

        // 초기 진행상태 반영
        UpdateFillBar();

        // 자동 시작 옵션이 켜져 있으면 타이머 시작
        if (autoStart)
        {
            StartTimer();
        }
    }

    void Update()
    {
        if (isRunning)
        {
            // 시간 감소
            //currentTime -= Time.deltaTime;

            // hit effet 때문에 Time.unscaledDeltaTime 사용

            float _currentTime = Time.unscaledDeltaTime * timerScale;

            currentTime -= _currentTime;

            // 시간이 0 이하로 내려가면
            if (currentTime <= 0)
            {
                currentTime = 0;
                OnTimerComplete();
            }

            // UI 상태 업데이트
            UpdateFillBar();
        }
    }

    /// <summary>
    /// 채우기 바의 크기를 업데이트하는 메서드
    /// </summary>
    void UpdateFillBar()
    {
        if (fillBar == null) return;

        // 현재 시간 비율 (0.0 ~ 1.0)
        float fillAmount = currentTime / maxTime;

        // RectTransform의 Width를 조절하는 방식
        RectTransform rectTransform = fillBar.GetComponent<RectTransform>();

        // 부모의 가로 길이 가져오기
        RectTransform parentRect = transform.GetComponent<RectTransform>();
        float maxWidth = parentRect.rect.width;

        // 현재 비율에 맞게 가로 길이 설정
        rectTransform.sizeDelta = new Vector2(maxWidth * fillAmount, rectTransform.sizeDelta.y);

        // 색상 변화 옵션이 켜져 있으면 색 변경
        if (useColorChange)
        {
            // 색상 변화: 여유(녹색) → 중간(노랑) → 부족(빨강)
            if (fillAmount > 0.5f)
            {
                fillBar.color = Color.green;
            }
            else if (fillAmount > 0.2f)
            {
                fillBar.color = Color.yellow;
            }
            else
            {
                fillBar.color = Color.red;
            }
        }
        // 타이머 텍스트 업데이트
        if (timerText != null)
        {
            timerText.text = currentTime.ToString("F0") + "초";
        }
    }

    /// <summary>
    /// 타이머를 시작하는 메서드
    /// </summary>
    public void StartTimer()
    {
        isRunning = true;
        currentTime = maxTime;
        timerScale = 1f;
        Debug.Log("타이머 시작!");
    }

    /// <summary>
    /// 타이머를 일시정지하는 메서드
    /// </summary>
    public void PauseTimer()
    {
        isRunning = false;
        Debug.Log("타이머 일시정지!");
    }

    /// <summary>
    /// 타이머를 재개하는 메서드
    /// </summary>
    public void ResumeTimer()
    {
        isRunning = true;
        Debug.Log("타이머 재개!");
    }

    /// <summary>
    /// 타이머를 리셋하는 메서드
    /// </summary>
    public void ResetTimer()
    {
        currentTime = maxTime;
        isRunning = false;
        timerScale = 1f;
        UpdateFillBar();
        Debug.Log("타이머 리셋!");
    }

    /// <summary>
    /// 타이머가 완료되었을 때 호출되는 메서드
    /// </summary>
    void OnTimerComplete()
    {
        isRunning = false;
        Debug.Log("타이머 완료!");

        // 여기에 타이머 완료 시 후속 동작 작성
        // 예: 게임 오버 처리, 다음 씬으로 이동 등

        GameManager.instance.Dead();

        // 루프 옵션이 켜져 있으면 재시작
        if (loop)
        {
            StartTimer();
        }
    }

    /// <summary>
    /// 남은 시간을 반환하는 메서드
    /// </summary>
    public float GetRemainingTime()
    {
        return currentTime;
    }

    /// <summary>
    /// 남은 시간 비율(0.0 ~ 1.0)을 반환하는 메서드
    /// </summary>
    public float GetTimeRatio()
    {
        return currentTime / maxTime;
    }


    public void AddTime(float AddSecond)
    {
        currentTime += AddSecond;
        if (currentTime > maxTime)
            currentTime = maxTime;
    }

    public void TimerScale(float scale)
    {
        timerScale = scale;
    }
    
    public bool IsTimerActive()
    {
        return isRunning;
    }
}
