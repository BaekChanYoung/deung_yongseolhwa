using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GoogleSheetsLoader sheetsLoader;
    [SerializeField] private TypingEffect typingEffect;
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private CharacterStand leftCharacter;
    [SerializeField] private CharacterStand rightCharacter;

    [Header("UI References")]
    [SerializeField] private GameObject dialoguePanel;

    private List<DialogueData> dialogues;
    private int currentDialogueIndex = 0;
    private bool isDialogueActive = false;

    private void Start()
    {
        sheetsLoader.onDataLoaded += OnDialogueDataLoaded;
        sheetsLoader.LoadDialogueData();

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (!isDialogueActive) return;

        // 마우스 클릭이나 스페이스바로 다음 대화 진행
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (typingEffect.IsTyping)
            {
                typingEffect.SkipTyping();
            }
            else
            {
                ShowNextDialogue();
            }
        }
    }

    private void OnDialogueDataLoaded(List<DialogueData> loadedDialogues)
    {
        dialogues = loadedDialogues;
        Debug.Log($"Loaded {dialogues.Count} dialogues");

        // 자동으로 대화 시작
        StartDialogue();
    }

    public void StartDialogue()
    {
        if (dialogues == null || dialogues.Count == 0)
        {
            Debug.LogWarning("No dialogues loaded!");
            return;
        }

        currentDialogueIndex = 0;
        isDialogueActive = true;

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        ShowCurrentDialogue();
    }

    private void ShowCurrentDialogue()
    {
        if (currentDialogueIndex >= dialogues.Count)
        {
            EndDialogue();
            return;
        }

        DialogueData currentData = dialogues[currentDialogueIndex];

        // 캐릭터 이름 표시
        characterNameText.text = currentData.characterName;

        // 캐릭터 스탠드 표시 및 표정 설정
        UpdateCharacterStand(currentData);

        // 대사 타이핑 효과 시작
        typingEffect.StartTyping(currentData.dialogueText);
    }

    private void UpdateCharacterStand(DialogueData data)
    {
        switch (data.position.ToLower())
        {
            case "left":
                leftCharacter.Show(data.position);
                leftCharacter.SetExpression(data.expression);
                break;
            case "right":
                rightCharacter.Show(data.position);
                rightCharacter.SetExpression(data.expression);
                break;
        }
    }

    private void ShowNextDialogue()
    {
        currentDialogueIndex++;
        ShowCurrentDialogue();
    }

    private void EndDialogue()
    {
        isDialogueActive = false;

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        leftCharacter.Hide();
        rightCharacter.Hide();

        Debug.Log("Dialogue ended");
    }
}