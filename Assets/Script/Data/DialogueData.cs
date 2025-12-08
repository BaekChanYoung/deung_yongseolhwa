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

    /// <summary>
    /// 컷신 명령어가 있는지 확인
    /// </summary>
    public bool HasCutscene()
    {
        return dialogueText.Contains("[CUTSCENE:");
    }

    /// <summary>
    /// 컷신 이름 추출 (예: "[CUTSCENE:Fishing]" → "Fishing")
    /// </summary>
    public string GetCutsceneName()
    {
        if (!HasCutscene()) return "";

        int startIndex = dialogueText.IndexOf("[CUTSCENE:") + 10;
        int endIndex = dialogueText.IndexOf("]", startIndex);

        if (endIndex > startIndex)
        {
            return dialogueText.Substring(startIndex, endIndex - startIndex);
        }

        return "";
    }

    /// <summary>
    /// 배경 변경 명령어가 있는지 확인
    /// </summary>
    public bool HasBackgroundChange()
    {
        return dialogueText.Contains("[BACKGROUND:");
    }

    /// <summary>
    /// 배경 이름 추출
    /// </summary>
    public string GetBackgroundName()
    {
        if (!HasBackgroundChange()) return "";

        int startIndex = dialogueText.IndexOf("[BACKGROUND:") + 12;
        int endIndex = dialogueText.IndexOf("]", startIndex);

        if (endIndex > startIndex)
        {
            return dialogueText.Substring(startIndex, endIndex - startIndex);
        }

        return "";
    }

    /// <summary>
    /// 효과음 명령어가 있는지 확인
    /// </summary>
    public bool HasSound()
    {
        return dialogueText.Contains("[SOUND:");
    }

    /// <summary>
    /// 효과음 이름 추출
    /// </summary>
    public string GetSoundName()
    {
        if (!HasSound()) return "";

        int startIndex = dialogueText.IndexOf("[SOUND:") + 7;
        int endIndex = dialogueText.IndexOf("]", startIndex);

        if (endIndex > startIndex)
        {
            return dialogueText.Substring(startIndex, endIndex - startIndex);
        }

        return "";
    }

    /// <summary>
    /// 실제 대사 텍스트 반환 (명령어 제거)
    /// </summary>
    public string GetCleanDialogueText()
    {
        string text = dialogueText;

        // 모든 명령어 제거
        while (text.Contains("[") && text.Contains("]"))
        {
            int startIndex = text.IndexOf("[");
            int endIndex = text.IndexOf("]", startIndex);

            if (endIndex > startIndex)
            {
                text = text.Remove(startIndex, endIndex - startIndex + 1);
            }
            else
            {
                break;
            }
        }

        return text.Trim();
    }
}