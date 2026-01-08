#if UNITY_EDITOR
using System.Reflection;
using UnityEditor;
using UnityEngine;

// 모든 유니티 오브젝트를 대상으로 하여 범용성을 높입니다.
[CustomEditor(typeof(UnityEngine.Object), true)]
[CanEditMultipleObjects]
public class UniversalButtonEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI(); // 기존 변수들(인스펙터 내용)을 먼저 그립니다.

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Custom Commands", EditorStyles.boldLabel);

        foreach (var t in targets)
        {
            DrawButtons(t);
        }
    }

    private void DrawButtons(object targetObj)
    {
        var methods = targetObj.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (var method in methods)
        {
            var attr = method.GetCustomAttribute<ButtonAttribute>();
            if (attr == null) continue;

            // 파라미터가 있는 메서드는 버튼으로 만들기 까다로우므로 제외합니다.
            if (method.GetParameters().Length > 0) continue;

            string label = string.IsNullOrEmpty(attr.Label) 
                ? ObjectNames.NicifyVariableName(method.Name) 
                : attr.Label;

            // 버튼의 색상을 지정하여 가독성을 높일 수 있습니다.
            GUI.backgroundColor = new Color(0.7f, 1f, 0.7f); 

            if (GUILayout.Button(label, GUILayout.Height(25)))
            {
                // 실행 전 Undo를 기록하여 실수 시 되돌릴 수 있게 합니다.
                Undo.RecordObject((UnityEngine.Object)targetObj, $"Button: {method.Name}");
                method.Invoke(targetObj, null);
                
                // 실행 후 값 변화를 에디터에 알립니다.
                EditorUtility.SetDirty((UnityEngine.Object)targetObj);
            }
            
            GUI.backgroundColor = Color.white; // 색상 초기화
        }
    }
}
#endif