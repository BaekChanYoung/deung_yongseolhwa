using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Spine ��� ��ȭ �ý��� �Ŵ���
/// Skip: ��ȭ ���� �� ���� �� �̵�
/// </summary>
public class DialogueManager : MonoBehaviour
{
    [Header("References")]
    public GoogleSheetsLoader sheetsLoader;
    public TypingEffect typingEffect;
    public TextMeshProUGUI characterNameText;

    [Header("Spine Character Stands")]
    public SpineCharacterStand leftCharacter;
    public SpineCharacterStand rightCharacter;

    [Header("UI References")]
    public GameObject dialoguePanel;
    public Button skipButton;

    [Header("Scene Transition")]
    [Tooltip("��ȭ ���� �� �̵��� �� �̸� (��������� SceneService ���)")]
    public string nextSceneName = "";
    [Tooltip("SceneService ��� ����")]
    public bool useSceneService = true;

    private List<DialogueData> dialogues;
    private int currentDialogueIndex = 0;
    private bool isDialogueActive = false;
    private ISceneService sceneService;

    private string currentLeftCharacter = "";
    private string currentRightCharacter = "";

    void Start()
    {
        // SceneService ��������
        if (useSceneService)
        {
            sceneService = ServiceLocator.Resolve<ISceneService>();
        }

        sheetsLoader.onDataLoaded += OnDialogueDataLoaded;
        sheetsLoader.LoadDialogueData();

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        // Skip ��ư ����
        SetupSkipButton();
    }

    /// <summary>
    /// Skip ��ư �ʱ� ����
    /// </summary>
    public void SetupSkipButton()
    {
        if (skipButton != null)
        {
            // ��ư Ŭ�� �̺�Ʈ ����
            skipButton.onClick.AddListener(OnSkipButtonClicked);
            Debug.Log("[DialogueManager] Skip button setup complete");
        }
        else
        {
            Debug.LogWarning("[DialogueManager] Skip button not assigned!");
        }
    }

    void Update()
    {
        if (!isDialogueActive) return;

        // ========== UI ������ Ŭ�� �� ���� ==========
        if (IsPointerOverUI())
        {
            return; // UI ��ư Ŭ���� ���⼭ ó�� �� ��
        }
        // ==========================================

        // ���콺 Ŭ���̳� �����̽��ٷ� ���� ��ȭ ����
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            HandleDialogueProgress();
        }
    }

    /// <summary>
    /// ���콺�� UI ���� �ִ��� Ȯ��
    /// </summary>
    public bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        // �����
        if (Input.touchCount > 0)
        {
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        }

        // PC (���콺)
        return EventSystem.current.IsPointerOverGameObject();
    }

    /// <summary>
    /// Skip ��ư Ŭ�� ��: ��ȭ ��ü ��ŵ�ϰ� ���� ������
    /// </summary>
    public void OnSkipButtonClicked()
    {
        Debug.Log("[DialogueManager] Skip button clicked! Ending dialogue and moving to next scene...");
        SkipDialogueAndMoveToNextScene();
    }

    /// <summary>
    /// ��ȭ ���� ó�� (Ÿ���� ��ŵ or ���� ��ȭ)
    /// </summary>
    public void HandleDialogueProgress()
    {
        if (typingEffect != null && typingEffect.IsTyping)
        {
            // Ÿ���� ���̸� ��� �Ϸ�
            typingEffect.SkipTyping();
        }
        else
        {
            // Ÿ���� �Ϸ�� ���� ��ȭ
            ShowNextDialogue();
        }
    }

    public void OnDialogueDataLoaded(List<DialogueData> loadedDialogues)
    {
        dialogues = loadedDialogues;
        Debug.Log($"[DialogueManager] Loaded {dialogues.Count} dialogues");

        // �ڵ����� ��ȭ ����
        StartDialogue();
    }

    /// <summary>
    /// ��ȭ ����
    /// </summary>
    public void StartDialogue()
    {
        if (dialogues == null || dialogues.Count == 0)
        {
            Debug.LogWarning("[DialogueManager] No dialogues loaded!");
            return;
        }

        currentDialogueIndex = 0;
        isDialogueActive = true;

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        // Skip ��ư Ȱ��ȭ
        if (skipButton != null)
        {
            skipButton.gameObject.SetActive(true);
        }

        ShowCurrentDialogue();
    }

    /// <summary>
    /// ���� ��ȭ ǥ��
    /// </summary>
    public void ShowCurrentDialogue()
    {
        if (currentDialogueIndex >= dialogues.Count)
        {
            EndDialogueAndMoveToNextScene();
            return;
        }

        DialogueData currentData = dialogues[currentDialogueIndex];

        // CharacterName�� empty�� �̸� ����
        if (characterNameText != null)
        {
            if (currentData.characterName.ToLower() == "empty")
            {
                characterNameText.text = "";
            }
            else
            {
                characterNameText.text = currentData.characterName;
            }
        }

        // DialogueText�� empty�� Ÿ���� ��ŵ
        if (typingEffect != null)
        {
            if (currentData.dialogueText.ToLower() == "empty")
            {
                typingEffect.StartTyping(""); // �� �ؽ�Ʈ
            }
            else
            {
                typingEffect.StartTyping(currentData.dialogueText);
            }
        }

        // Spine ĳ���� ���ĵ� ǥ�� �� �ִϸ��̼� ����
        UpdateSpineCharacterStand(currentData);
    }

    /// <summary>
    /// Spine ĳ���� ���ĵ� ������Ʈ
    /// </summary>
    public void UpdateSpineCharacterStand(DialogueData data)
    {
        string position = data.position.ToLower();
        string characterName = data.characterName;
        string expression = data.expression;

        if (position == "empty")
        {
            // ����/������ ���� ���� �� �� Ȯ��
            if (!string.IsNullOrEmpty(currentLeftCharacter))
            {
                if (leftCharacter != null)
                {
                    leftCharacter.Hide();
                }
                currentLeftCharacter = "";
                Debug.Log("[DialogueManager] Left character hidden (empty)");
            }

            if (!string.IsNullOrEmpty(currentRightCharacter))
            {
                if (rightCharacter != null)
                {
                    rightCharacter.Hide();
                }
                currentRightCharacter = "";
                Debug.Log("[DialogueManager] Right character hidden (empty)");
            }

            return;
        }

        if (characterName.ToLower() == "empty")
        {
            if (position == "left")
            {
                if (leftCharacter != null)
                {
                    leftCharacter.Hide();
                }
                currentLeftCharacter = "";
                Debug.Log("[DialogueManager] Left character hidden (empty name)");
            }
            else if (position == "right")
            {
                if (rightCharacter != null)
                {
                    rightCharacter.Hide();
                }
                currentRightCharacter = "";
                Debug.Log("[DialogueManager] Right character hidden (empty name)");
            }

            return;
        }

        switch (position)
        {
            case "left":
                if (leftCharacter != null)
                {
                    // ���� ĳ���Ͱ� �̹� ǥ�� ���̸� ���̵� �� ��ŵ
                    if (currentLeftCharacter == characterName)
                    {
                        // ǥ���� ����
                        Debug.Log($"[DialogueManager] Left character '{characterName}' already shown, changing expression only");
                        leftCharacter.SetExpression(expression);
                    }
                    else
                    {
                        // �� ĳ���� ����
                        Debug.Log($"[DialogueManager] Showing new left character: {characterName}");
                        leftCharacter.Show(position);
                        leftCharacter.SetExpression(expression);
                        currentLeftCharacter = characterName;
                    }
                }
                break;

            case "right":
                if (rightCharacter != null)
                {
                    // ���� ĳ���Ͱ� �̹� ǥ�� ���̸� ���̵� �� ��ŵ
                    if (currentRightCharacter == characterName)
                    {
                        // ǥ���� ����
                        Debug.Log($"[DialogueManager] Right character '{characterName}' already shown, changing expression only");
                        rightCharacter.SetExpression(expression);
                    }
                    else
                    {
                        // �� ĳ���� ����
                        Debug.Log($"[DialogueManager] Showing new right character: {characterName}");
                        rightCharacter.Show(position);
                        rightCharacter.SetExpression(expression);
                        currentRightCharacter = characterName;
                    }
                }
                break;
        }
    }

    /// <summary>
    /// ���� ��ȭ�� ����
    /// </summary>
    public void ShowNextDialogue()
    {
        currentDialogueIndex++;
        ShowCurrentDialogue();
    }

    /// <summary>
    /// ��ȭ ���� �� ���� ������ �̵�
    /// </summary>
    public void EndDialogueAndMoveToNextScene()
    {
        Debug.Log("[DialogueManager] All dialogues complete. Moving to next scene...");

        // ��ȭ ����
        EndDialogue();

        // ���� ������ �̵�
        MoveToNextScene();
    }

    /// <summary>
    /// Skip ��ư���� ��ȭ ��ŵ�ϰ� ���� ������
    /// </summary>
    public void SkipDialogueAndMoveToNextScene()
    {
        Debug.Log("[DialogueManager] Skipping all dialogues...");

        // ��ȭ ����
        EndDialogue();

        // ���� ������ �̵�
        MoveToNextScene();
    }

    /// <summary>
    /// ��ȭ ���� (UI ����)
    /// </summary>
    public void EndDialogue()
    {
        isDialogueActive = false;

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        // Skip ��ư ��Ȱ��ȭ
        if (skipButton != null)
        {
            skipButton.gameObject.SetActive(false);
        }

        if (leftCharacter != null)
        {
            leftCharacter.Hide();
        }

        if (rightCharacter != null)
        {
            rightCharacter.Hide();
        }

        Debug.Log("[DialogueManager] Dialogue ended");
    }

    /// <summary>
    /// ���� ������ �̵�
    /// </summary>
    public void MoveToNextScene()
    {
        // SceneService ���
        if (useSceneService && sceneService != null)
        {
            Debug.Log("[DialogueManager] Using SceneService to load next scene");

            // SceneService�� LoadNextScene() �Ǵ� Ư�� �� �ε�
            // ��: sceneService.LoadScene("NextSceneName");

            // ���� SceneService�� LoadNextScene() ���� �޼��尡 �ִٸ�:
            // sceneService.LoadNextScene();

            // �Ǵ� Ư�� �� �̸����� �ε�:
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                Debug.Log($"[DialogueManager] Loading scene: {nextSceneName}");
                // sceneService.LoadSceneWithLoading(nextSceneName);

                // SceneService�� ���ٸ� ���� �ε�:
                sceneService.LoadSceneWithLoading(nextSceneName);
            }
            else
            {
                Debug.LogWarning("[DialogueManager] Next scene name is not set!");
            }
        }
        // SceneService ���� ���� �� �ε�
        else if (!string.IsNullOrEmpty(nextSceneName))
        {
            Debug.Log($"[DialogueManager] Loading scene directly: {nextSceneName}");
            sceneService.LoadSceneWithLoading(nextSceneName);
        }
        else
        {
            Debug.LogWarning("[DialogueManager] Cannot load next scene: No scene name specified!");
        }
    }

    /// <summary>
    /// �ܺο��� ���� �� �̸� ����
    /// </summary>
    public void SetNextScene(string sceneName)
    {
        nextSceneName = sceneName;
        Debug.Log($"[DialogueManager] Next scene set to: {nextSceneName}");
    }
}