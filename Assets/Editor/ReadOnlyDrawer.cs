using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    // 필드의 실제 높이를 계산해서 반환해야 겹치지 않습니다.
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        GUI.enabled = false; 
        // 마지막 인자인 'true'는 자식 속성(구조체 내부 필드 등)을 포함한다는 의미입니다.
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true; 
    }
}