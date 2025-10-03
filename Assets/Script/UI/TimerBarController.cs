using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimerBarController : MonoBehaviour
{
    [Header("Ÿ�̸� ����")]
    [Tooltip("Ÿ�̸� �� �ð� (��)")]
    public float maxTime = 60f;

    [Header("UI ����")]
    [Tooltip("ä���� ������ �� Image")]
    public Image fillBar;

    [Tooltip("Ÿ�̸� �ؽ�Ʈ (���û���)")]
    public TextMeshProUGUI timerText;

    [Header("�ɼ�")]
    [Tooltip("���� �� �ڵ����� Ÿ�̸� ����")]
    public bool autoStart = true;

    [Tooltip("Ÿ�̸� ���� �� �ڵ����� �ٽ� ����")]
    public bool loop = false;

    [Tooltip("�ð��� ���� ���� ��ȭ")]
    public bool useColorChange = true;

    // ���� ���� �ð�
    private float currentTime;

    // Ÿ�̸� ���� �� ����
    private bool isRunning = false;

    void Start()
    {
        // �ʱ� �ð� ����
        currentTime = maxTime;

        // Fill Bar�� �Ҵ���� �ʾҴٸ� �ڵ����� ã�� �õ�
        if (fillBar == null)
        {
            fillBar = transform.Find("FillBar").GetComponent<Image>();

            if (fillBar == null)
            {
                Debug.LogError("FillBar Image�� ã�� �� �����ϴ�!");
                return;
            }
        }

        // �ʱ� �������� �ִ�� ����
        UpdateFillBar();

        // �ڵ� ���� �ɼ��� ���������� Ÿ�̸� ����
        if (autoStart)
        {
            StartTimer();
        }
    }

    void Update()
    {
        if (isRunning)
        {
            // �ð� ����
            currentTime -= Time.deltaTime;

            // �ð��� 0 ���Ϸ� ��������
            if (currentTime <= 0)
            {
                currentTime = 0;
                OnTimerComplete();
            }

            // ������ �� ������Ʈ
            UpdateFillBar();
        }
    }

    /// <summary>
    /// ������ ���� ũ�⸦ ������Ʈ�ϴ� �޼���
    /// </summary>
    void UpdateFillBar()
    {
        if (fillBar == null) return;

        // ���� �ð��� ���� ��� (0.0 ~ 1.0)
        float fillAmount = currentTime / maxTime;

        // RectTransform�� Width�� �����ϴ� ���
        RectTransform rectTransform = fillBar.GetComponent<RectTransform>();

        // �θ��� �ʺ� ��������
        RectTransform parentRect = transform.GetComponent<RectTransform>();
        float maxWidth = parentRect.rect.width;

        // ���� ������ �°� �ʺ� ����
        rectTransform.sizeDelta = new Vector2(maxWidth * fillAmount, rectTransform.sizeDelta.y);

        // ���� ��ȭ �ɼ��� ���������� ���� ����
        if (useColorChange)
        {
            // ���� ��ȭ: �ʷ� �� ��� �� ����
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
        // Ÿ�̸� �ؽ�Ʈ ������Ʈ
        if (timerText != null)
        {
            timerText.text = currentTime.ToString("F0") + "��";
        }
    }

    /// <summary>
    /// Ÿ�̸Ӹ� �����ϴ� �޼���
    /// </summary>
    public void StartTimer()
    {
        isRunning = true;
        currentTime = maxTime;
        Debug.Log("Ÿ�̸� ����!");
    }

    /// <summary>
    /// Ÿ�̸Ӹ� �Ͻ������ϴ� �޼���
    /// </summary>
    public void PauseTimer()
    {
        isRunning = false;
        Debug.Log("Ÿ�̸� �Ͻ�����!");
    }

    /// <summary>
    /// Ÿ�̸Ӹ� �簳�ϴ� �޼���
    /// </summary>
    public void ResumeTimer()
    {
        isRunning = true;
        Debug.Log("Ÿ�̸� �簳!");
    }

    /// <summary>
    /// Ÿ�̸Ӹ� �����ϴ� �޼���
    /// </summary>
    public void ResetTimer()
    {
        currentTime = maxTime;
        isRunning = false;
        UpdateFillBar();
        Debug.Log("Ÿ�̸� ����!");
    }

    /// <summary>
    /// Ÿ�̸Ӱ� �Ϸ�Ǿ��� �� ȣ��Ǵ� �޼���
    /// </summary>
    void OnTimerComplete()
    {
        isRunning = false;
        Debug.Log("Ÿ�̸� ����!");

        // ���⿡ Ÿ�̸� ���� �� ������ �ڵ� �ۼ�
        // ��: ���� ����, ���� �������� �̵� ��

        // ���� �ɼ��� ���������� �ٽ� ����
        if (loop)
        {
            StartTimer();
        }
    }

    /// <summary>
    /// ���� �ð��� �������� �޼���
    /// </summary>
    public float GetRemainingTime()
    {
        return currentTime;
    }

    /// <summary>
    /// ���� �ð��� ������ �������� �޼��� (0.0 ~ 1.0)
    /// </summary>
    public float GetTimeRatio()
    {
        return currentTime / maxTime;
    }
}