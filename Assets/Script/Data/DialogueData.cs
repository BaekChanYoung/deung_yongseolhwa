using System;
using UnityEngine;

[System.Serializable]
public class DialogueData
{
    public int id;
    public string characterName;
    public string expression;
    public string dialogueText;
    public string position; // "Left", "Right", "Center"
}

[System.Serializable]
public class DialogueDataList
{
    public DialogueData[] dialogues;
}