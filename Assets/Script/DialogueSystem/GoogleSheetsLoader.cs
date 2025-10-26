using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class GoogleSheetsLoader : MonoBehaviour
{
    [Header("Google Sheets Settings")]
    [Tooltip("Google Sheets CSV export URL")]
    public string sheetURL = "YOUR_GOOGLE_SHEETS_CSV_URL";

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
            Debug.LogError("Failed to load data: " + www.error);
            yield break;
        }

        string csvData = www.downloadHandler.text;
        List<DialogueData> dialogues = ParseCSV(csvData);

        onDataLoaded?.Invoke(dialogues);
    }

    private List<DialogueData> ParseCSV(string csvData)
    {
        List<DialogueData> dialogues = new List<DialogueData>();
        string[] lines = csvData.Split('\n');

        // 첫 번째 줄은 헤더이므로 스킵
        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;

            string[] fields = lines[i].Split('\t');

            if (fields.Length >= 5)
            {
                DialogueData data = new DialogueData
                {
                    id = int.Parse(fields[0].Trim()),
                    characterName = fields[1].Trim(),
                    expression = fields[2].Trim(),
                    dialogueText = fields[3].Trim(),
                    position = fields[4].Trim()
                };

                dialogues.Add(data);
            }
        }

        return dialogues;
    }
}
