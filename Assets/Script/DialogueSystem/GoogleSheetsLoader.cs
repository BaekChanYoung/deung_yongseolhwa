using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Google Sheets TSV 데이터 로더
/// 안전한 파싱 및 에러 처리 포함
/// </summary>
public class GoogleSheetsLoader : MonoBehaviour
{
    [Header("Google Sheets Settings")]
    [Tooltip("Google Sheets TSV export URL")]
    public string sheetURL = "YOUR_GOOGLE_SHEETS_TSV_URL";

    public delegate void OnDataLoaded(List<DialogueData> dialogues);
    public event OnDataLoaded onDataLoaded;

    public void LoadDialogueData()
    {
        StartCoroutine(LoadDataCoroutine());
    }

    private IEnumerator LoadDataCoroutine()
    {
        UnityWebRequest www = UnityWebRequest.Get(sheetURL);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"[GoogleSheetsLoader] Failed to load data: {www.error}");
            yield break;
        }

        string tsvData = www.downloadHandler.text;
        Debug.Log($"[GoogleSheetsLoader] Raw data length: {tsvData.Length} characters");

        List<DialogueData> dialogues = ParseTSV(tsvData);

        Debug.Log($"[GoogleSheetsLoader] Parsed {dialogues.Count} dialogues");

        onDataLoaded?.Invoke(dialogues);
    }

    /// <summary>
    /// TSV 데이터 파싱 (안전한 버전)
    /// </summary>
    private List<DialogueData> ParseTSV(string tsvData)
    {
        List<DialogueData> dialogues = new List<DialogueData>();

        // 줄 단위로 분리
        string[] lines = tsvData.Split('\n');

        Debug.Log($"[GoogleSheetsLoader] Total lines: {lines.Length}");

        // 첫 번째 줄은 헤더이므로 스킵
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim(); // 공백 제거

            // 빈 줄 스킵
            if (string.IsNullOrWhiteSpace(line))
            {
                Debug.Log($"[GoogleSheetsLoader] Line {i}: Empty line, skipping");
                continue;
            }

            // 탭으로 필드 분리
            string[] fields = line.Split('\t');

            // 필드 개수 확인 (최소 5개 필요)
            if (fields.Length < 5)
            {
                Debug.LogWarning($"[GoogleSheetsLoader] Line {i}: Not enough fields ({fields.Length}/5), skipping");
                Debug.LogWarning($"[GoogleSheetsLoader] Line content: '{line}'");
                continue;
            }

            try
            {
                // ID 파싱 (안전하게)
                string idString = fields[0].Trim();

                if (string.IsNullOrEmpty(idString))
                {
                    Debug.LogWarning($"[GoogleSheetsLoader] Line {i}: Empty ID, skipping");
                    continue;
                }

                // ID가 숫자인지 확인
                if (!int.TryParse(idString, out int id))
                {
                    Debug.LogWarning($"[GoogleSheetsLoader] Line {i}: Invalid ID '{idString}', skipping");
                    continue;
                }

                // DialogueData 생성
                DialogueData data = new DialogueData
                {
                    id = id,
                    characterName = fields[1].Trim(),
                    expression = fields[2].Trim(),
                    dialogueText = fields[3].Trim(),
                    position = fields[4].Trim()
                };

                dialogues.Add(data);

                Debug.Log($"[GoogleSheetsLoader] Line {i}: Parsed dialogue #{data.id} - {data.characterName}: '{data.dialogueText}'");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[GoogleSheetsLoader] Line {i}: Parse error - {e.Message}");
                Debug.LogError($"[GoogleSheetsLoader] Line content: '{line}'");
            }
        }

        return dialogues;
    }
}