#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(GraphAttribute))]
public class GraphAttributeDrawer : PropertyDrawer
{
    const float k_Pad = 4f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        var attr = (GraphAttribute)attribute;

        float h = EditorGUIUtility.singleLineHeight + k_Pad; // 라벨 높이
        h += Mathf.Max(40f, attr.height) + k_Pad;            // 그래프 영역

        if (!attr.compact && attr.drawList && property.isArray)
        {
            h += EditorGUI.GetPropertyHeight(property, true) + k_Pad;
        }
        return h;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var attr = (GraphAttribute)attribute;

        // 1) 라벨
        var labelRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.LabelField(labelRect, label);

        // 2) 그래프 사각형
        var graphRect = new Rect(position.x, labelRect.yMax + k_Pad, position.width, Mathf.Max(40f, attr.height));

        // 3) 데이터 검사 (float[] 또는 List<float> 여야 함)
        if (!property.isArray || property.arraySize == 0)
        {
            DrawGraphBackground(graphRect, attr, true, "No data (float[] / List<float>)");
        }
        else
        {
            var first = property.GetArrayElementAtIndex(0);
            if (first.propertyType != SerializedPropertyType.Float && first.propertyType != SerializedPropertyType.Integer)
            {
                DrawGraphBackground(graphRect, attr, true, "Only float[] / List<float> supported");
            }
            else
            {
                DrawGraph(graphRect, property, attr);
            }
        }

        // 4) 배열/리스트 편집 영역
        if (!attr.compact && attr.drawList && property.isArray)
        {
            var listRect = new Rect(position.x, graphRect.yMax + k_Pad, position.width,
                EditorGUI.GetPropertyHeight(property, true));
            EditorGUI.PropertyField(listRect, property, new GUIContent(property.displayName), true);
        }
    }

    void DrawGraphBackground(Rect r, GraphAttribute attr, bool drawText, string text = null)
    {
        EditorGUI.DrawRect(r, new Color(0.12f, 0.12f, 0.12f, 1f));

        if (attr.showGrid)
        {
            var gCol = new Color(1f, 1f, 1f, 0.06f);
            int div = Mathf.Max(1, attr.gridDiv);
            for (int i = 0; i <= div; i++)
            {
                float y = Mathf.Lerp(r.yMax, r.yMin, i / (float)div);
                EditorGUI.DrawRect(new Rect(r.x, y - 0.5f, r.width, 1f), gCol);

                float x = Mathf.Lerp(r.x, r.xMax, i / (float)div);
                EditorGUI.DrawRect(new Rect(x - 0.5f, r.y, 1f, r.height), gCol);
            }
        }

        // Border
        var b = new Color(0, 0, 0, 0.5f);
        EditorGUI.DrawRect(new Rect(r.x, r.y, r.width, 1), b);
        EditorGUI.DrawRect(new Rect(r.x, r.yMax - 1, r.width, 1), b);
        EditorGUI.DrawRect(new Rect(r.x, r.y, 1, r.height), b);
        EditorGUI.DrawRect(new Rect(r.xMax - 1, r.y, 1, r.height), b);

        if (drawText && !string.IsNullOrEmpty(text))
        {
            var style = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
            style.alignment = TextAnchor.MiddleCenter;
            EditorGUI.LabelField(r, text, style);
        }
    }

    void DrawGraph(Rect r, SerializedProperty arrayProp, GraphAttribute attr)
    {
        DrawGraphBackground(r, attr, false);

        int n = arrayProp.arraySize;
        if (n < 2) return;

        // 1) 값 읽기/최솟값/최댓값
        float min = float.PositiveInfinity, max = float.NegativeInfinity;
        if (attr.autoY)
        {
            for (int i = 0; i < n; i++)
            {
                var el = arrayProp.GetArrayElementAtIndex(i);
                float v = (el.propertyType == SerializedPropertyType.Integer) ? el.intValue : el.floatValue;
                if (v < min) min = v;
                if (v > max) max = v;
            }
            if (float.IsInfinity(min) || float.IsInfinity(max))
            {
                min = 0f; max = 1f;
            }
            if (Mathf.Approximately(min, max))
            {
                min -= 0.5f; max += 0.5f;
            }
            // 시각적 여백 5%
            float pad = (max - min) * 0.05f;
            min -= pad; max += pad;
        }
        else
        {
            min = attr.yMin; max = Mathf.Max(attr.yMin + 1e-6f, attr.yMax);
        }

        // 2) 다운샘플 (성능)
        int maxPts = Mathf.Clamp(Mathf.RoundToInt(r.width), 8, 4096);
        int step = Mathf.Max(1, Mathf.CeilToInt(n / (float)maxPts));
        int ptsCount = Mathf.CeilToInt(n / (float)step);

        Vector3[] pts = new Vector3[ptsCount];
        float denom = (max - min);
        if (Mathf.Abs(denom) < 1e-6f) denom = 1f;

        int idx = 0;
        for (int i = 0; i < n; i += step)
        {
            var el = arrayProp.GetArrayElementAtIndex(i);
            float v = (el.propertyType == SerializedPropertyType.Integer) ? el.intValue : el.floatValue;

            float t = (i / (float)(n - 1));
            float x = Mathf.Lerp(r.x, r.xMax, t);

            float ny = Mathf.InverseLerp(min, max, v);
            float y = Mathf.Lerp(r.yMax, r.yMin, ny);

            pts[idx++] = new Vector3(x, y, 0f);
        }

        // 3) 채우기(옵션: 수직 막대 방식)
        if (attr.fill && pts.Length > 1)
        {
            var fillCol = ParseHtmlColor(attr.color, 1f);
            fillCol.a = Mathf.Clamp01(attr.fillAlpha);
            for (int i = 0; i < pts.Length; i++)
            {
                float x = pts[i].x;
                float y = pts[i].y;
                var rr = Rect.MinMaxRect(x, y, x + 1, r.yMax);
                EditorGUI.DrawRect(rr, fillCol);
            }
        }

        // 4) 라인
        Handles.BeginGUI();
        var prev = Handles.color;
        Handles.color = ParseHtmlColor(attr.color, 1f);
        Handles.DrawAAPolyLine(Mathf.Max(1f, attr.thickness), pts);
        Handles.color = prev;
        Handles.EndGUI();
    }

    static Color ParseHtmlColor(string html, float defaultAlpha)
    {
        if (ColorUtility.TryParseHtmlString(html, out var c))
        {
            if (Mathf.Approximately(c.a, 0f)) c.a = defaultAlpha;
            return c;
        }
        return new Color(0f, 0.82f, 1f, defaultAlpha); // fallback
    }
}
#endif
