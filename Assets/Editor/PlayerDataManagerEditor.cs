#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(PlayerDataManager))]
[CanEditMultipleObjects]
public class PlayerDataManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 기본 인스펙터 먼저 그리기
        DrawDefaultInspector();

        EditorGUILayout.Space();

        // 대상 오브젝트들(멀티 선택 고려)
        foreach (var t in targets)
        {
            DrawButtonsForTarget(t);
        }
    }

    private void DrawButtonsForTarget(object targetObj)
    {
        var targetType = targetObj.GetType();
        var methods = targetType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (var method in methods)
        {
            // 메서드에 붙은 ButtonAttribute 찾기
            var buttonAttr = method.GetCustomAttribute<ButtonAttribute>();
            if (buttonAttr == null)
                continue;

            // 파라미터 있는 메서드나 void가 아닌 메서드는 무시
            if (method.GetParameters().Length > 0 || method.ReturnType != typeof(void))
                continue;

            // 버튼 텍스트
            string label = string.IsNullOrEmpty(buttonAttr.Label)
                ? ObjectNames.NicifyVariableName(method.Name)
                : buttonAttr.Label;

            if (GUILayout.Button(label))
            {
                // Undo 기록 남기고(선택)
                Undo.RecordObject((UnityEngine.Object)targetObj, $"Invoke {method.Name}");

                method.Invoke(targetObj, null);

                // 값이 바뀌었을 수 있으니 에디터 갱신
                EditorUtility.SetDirty((UnityEngine.Object)targetObj);
            }
        }
    }
}
#endif
