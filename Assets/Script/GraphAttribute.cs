using UnityEngine;

/// <summary>
/// 리스트/배열(float) 필드에 붙여 인스펙터에서 선 그래프를 그립니다.
/// 예: [Graph(height=140, color="#00D1FF", autoY=true, showGrid=true)]
/// </summary>
public class GraphAttribute : PropertyAttribute
{
    /// <summary>그래프 높이(px)</summary>
    public float height = 140f;

    /// <summary>그래프 색상(HTML Hex). 예: "#00D1FF", "#FF5C5C"</summary>
    public string color = "#00D1FF";

    /// <summary>Y 범위를 데이터에서 자동 계산</summary>
    public bool autoY = true;

    /// <summary>autoY=false일 때 사용되는 최소/최대값</summary>
    public float yMin = 0f;
    public float yMax = 1f;

    /// <summary>배경 격자 표시</summary>
    public bool showGrid = true;

    /// <summary>격자 분할 수</summary>
    public int gridDiv = 4;

    /// <summary>선 두께</summary>
    public float thickness = 2f;

    /// <summary>컴팩트(값 리스트를 펼치지 않고 그래프만 표시)</summary>
    public bool compact = true;

    /// <summary>그래프 아래에 values 배열/리스트를 함께 노출할지</summary>
    public bool drawList = false;

    /// <summary>영역 채우기(가벼운 방식)</summary>
    public bool fill = false;

    /// <summary>채우기 불투명도(0~1)</summary>
    public float fillAlpha = 0.15f;

    public GraphAttribute() { }
}