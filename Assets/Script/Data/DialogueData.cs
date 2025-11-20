using System;

/// <summary>
/// 대화 데이터 구조
/// Google Sheets CSV에서 로드되는 데이터
/// </summary>
[Serializable]
public class DialogueData
{
    public int id;
    public string characterName;
    public string expression;
    public string dialogueText;
    public string position;
}