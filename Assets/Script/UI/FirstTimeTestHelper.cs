using UnityEngine;

/// <summary>
/// FirstTimeManager 테스트 헬퍼
/// TitleScene에 추가하여 Play Mode에서 테스트
/// </summary>
public class FirstTimeTestHelper : MonoBehaviour
{
    [Header("GUI 설정")]
    [Tooltip("테스트 버튼 표시 여부")]
    public bool showTestButtons = true;

    [Tooltip("버튼 크기")]
    public Vector2 buttonSize = new Vector2(200, 50);

    [Tooltip("버튼 간격")]
    public float buttonSpacing = 10f;

    void OnGUI()
    {
        if (!showTestButtons) return;

        float x = 10;
        float y = 10;
        float width = buttonSize.x;
        float height = buttonSize.y;

        // 1. Reset 버튼
        if (GUI.Button(new Rect(x, y, width, height), "Delete All PlayerPrefs"))
        {
            FirstTimeManager.DeleteAllPlayerPrefs();
        }

        y += height + buttonSpacing;

        // 2. Status 버튼
        if (GUI.Button(new Rect(x, y, width, height), "Check Status"))
        {
            FirstTimeManager.CheckStatus();
        }

        y += height + buttonSpacing;

        // 3. Mark Completed 버튼
        if (GUI.Button(new Rect(x, y, width, height), "Mark Completed"))
        {
            FirstTimeManager.MarkAsCompleted();
        }

        y += height + buttonSpacing + 10;

        // 현재 상태 표시
        string statusText = FirstTimeManager.IsFirstTime()
            ? "상태: 최초 실행"
            : "상태: 재실행";

        GUI.Label(new Rect(x, y, width, 30), statusText);
    }
}