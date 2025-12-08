using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Spine 기반 대화 시스템 매니저
/// Skip: 대화 종료 후 다음 씬 이동
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

    [Tooltip("오른쪽 그룹 캐릭터 스탠드 (예: 빌런들)")]
    public SpineCharacterStand rightGroupCharacter;

    [Header("UI References")]
    public GameObject dialoguePanel;
    public Button skipButton;

    [Header("Cutscene & Background")]
    public CutsceneManager cutsceneManager;

    [Header("Scene Transition")]
    [Tooltip("대화 종료 후 이동할 씬 이름 (비어있으면 SceneService 사용)")]
    public string nextSceneName = "";
    [Tooltip("SceneService 사용 여부")]
    public bool useSceneService = true;

    private List<DialogueData> dialogues;
    private int currentDialogueIndex = 0;
    private bool isDialogueActive = false;
    private ISceneService sceneService;

    private string currentLeftCharacter = "";
    private string currentRightCharacter = "";

    private SpineCharacterStand currentRightStand = null;

    private bool isCutscenePlaying = false;

    void Start()
    {
        // SceneService 가져오기
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

        // Skip 버튼 설정
        SetupSkipButton();
    }

    /// <summary>
    /// Skip 버튼 초기 설정
    /// </summary>
    public void SetupSkipButton()
    {
        if (skipButton != null)
        {
            // 버튼 클릭 이벤트 연결
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

        if (isCutscenePlaying) return;

        // ========== UI 위에서 클릭 시 무시 ==========
        if (IsPointerOverUI())
        {
            return; // UI 버튼 클릭은 여기서 처리 안 함
        }
        // ==========================================

        // 마우스 클릭이나 스페이스바로 다음 대화 진행
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            HandleDialogueProgress();
        }
    }

    /// <summary>
    /// 마우스가 UI 위에 있는지 확인
    /// </summary>
    public bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        // 모바일
        if (Input.touchCount > 0)
        {
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);
        }

        // PC (마우스)
        return EventSystem.current.IsPointerOverGameObject();
    }

    /// <summary>
    /// Skip 버튼 클릭 시: 대화 전체 스킵하고 다음 씬으로
    /// </summary>
    public void OnSkipButtonClicked()
    {
        Debug.Log("[DialogueManager] Skip button clicked! Ending dialogue and moving to next scene...");

        if (cutsceneManager != null && isCutscenePlaying)
        {
            cutsceneManager.SkipCutscene();
            isCutscenePlaying = false;
        }

        SkipDialogueAndMoveToNextScene();
    }

    /// <summary>
    /// 대화 진행 처리 (타이핑 스킵 or 다음 대화)
    /// </summary>
    public void HandleDialogueProgress()
    {
        if (typingEffect != null && typingEffect.IsTyping)
        {
            // 타이핑 중이면 즉시 완료
            typingEffect.SkipTyping();
        }
        else
        {
            // 타이핑 완료면 다음 대화
            ShowNextDialogue();
        }
    }

    public void OnDialogueDataLoaded(List<DialogueData> loadedDialogues)
    {
        dialogues = loadedDialogues;
        Debug.Log($"[DialogueManager] Loaded {dialogues.Count} dialogues");

        // 자동으로 대화 시작
        StartDialogue();
    }

    /// <summary>
    /// 대화 시작
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

        // Skip 버튼 활성화
        if (skipButton != null)
        {
            skipButton.gameObject.SetActive(true);
        }

        ShowCurrentDialogue();
    }

    /// <summary>
    /// 현재 대화 표시
    /// </summary>
    public void ShowCurrentDialogue()
    {
        if (currentDialogueIndex >= dialogues.Count)
        {
            EndDialogueAndMoveToNextScene();
            return;
        }

        DialogueData currentData = dialogues[currentDialogueIndex];
        Debug.Log($"[DialogueManager] Showing dialogue #{currentData.id}");

        // 컷신 확인 (최우선 처리)
        if (currentData.HasCutscene())
        {
            string cutsceneName = currentData.GetCutsceneName();
            Debug.Log($"[DialogueManager] Cutscene detected: {cutsceneName}");

            PlayCutsceneAndProceed(cutsceneName);
            return; // 컷신 재생 후 자동으로 다음 대화로 진행
        }

        // 배경 변경 확인
        if (currentData.HasBackgroundChange())
        {
            string backgroundName = currentData.GetBackgroundName();
            Debug.Log($"[DialogueManager] Background change detected: {backgroundName}");

            if (cutsceneManager != null)
            {
                cutsceneManager.ChangeBackgroundWithFade(backgroundName);
            }
        }

        // 효과음 재생 확인
        if (currentData.HasSound())
        {
            string soundName = currentData.GetSoundName();
            Debug.Log($"[DialogueManager] Sound detected: {soundName}");

            // TODO: 사운드 매니저 연동
            // SoundManager.Instance.PlaySFX(soundName);
        }

        // CharacterName이 empty면 이름 숨김
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

        // DialogueText가 empty면 타이핑 스킵
        if (typingEffect != null)
        {
            string cleanText = currentData.GetCleanDialogueText();

            if (currentData.dialogueText.ToLower() == "empty")
            {
                typingEffect.StartTyping(""); // 빈 텍스트
            }
            else
            {
                typingEffect.StartTyping(currentData.dialogueText);
            }
        }

        // Spine 캐릭터 스탠드 표시 및 애니메이션 설정
        UpdateSpineCharacterStand(currentData);
    }

    /// <summary>
    /// 컷신 재생 후 자동으로 다음 대화 진행
    /// </summary>
    private void PlayCutsceneAndProceed(string cutsceneName)
    {
        if (cutsceneManager == null)
        {
            Debug.LogWarning("[DialogueManager] CutsceneManager not assigned!");
            ShowNextDialogue(); // 컷신 없이 다음으로
            return;
        }

        // 대화창 숨기기 (선택사항)
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        // 캐릭터 숨기기
        if (leftCharacter != null) leftCharacter.Hide();
        if (currentRightStand != null)
        {
            currentRightStand.Hide();
        }
        else if (rightCharacter != null)
        {
            rightCharacter.Hide();
        }
        currentLeftCharacter = "";
        currentRightCharacter = "";
        currentRightStand = null;

        isCutscenePlaying = true;

        // 컷신 재생 (완료 후 콜백)
        cutsceneManager.PlayCutscene(cutsceneName, () =>
        {
            isCutscenePlaying = false;

            // 대화창 다시 표시
            if (dialoguePanel != null)
            {
                dialoguePanel.SetActive(true);
            }

            // 다음 대화로 자동 진행
            ShowNextDialogue();
        });
    }


    /// <summary>
    /// Spine 캐릭터 스탠드 업데이트
    /// </summary>
    public void UpdateSpineCharacterStand(DialogueData data)
    {
        string position = data.position.ToLower();
        string characterName = data.characterName;
        string expression = data.expression;

        // ===== Position이 "empty"일 때 =====
        if (position == "empty")
        {
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
                // 현재 표시 중인 오른쪽 스탠드 숨김
                if (currentRightStand != null)
                {
                    currentRightStand.Hide();
                }
                else if (rightCharacter != null)
                {
                    rightCharacter.Hide();
                }

                currentRightCharacter = "";
                currentRightStand = null;
                Debug.Log("[DialogueManager] Right character hidden (empty)");
            }

            return;
        }

        // ===== CharacterName이 "empty"일 때 =====
        if (characterName.ToLower() == "empty")
        {
            if (position.Contains("left"))
            {
                if (leftCharacter != null)
                {
                    leftCharacter.Hide();
                }
                currentLeftCharacter = "";
                Debug.Log("[DialogueManager] Left character hidden (empty name)");
            }

            if (position.Contains("right"))
            {
                // 현재 표시 중인 오른쪽 스탠드 숨김
                if (currentRightStand != null)
                {
                    currentRightStand.Hide();
                }
                else if (rightCharacter != null)
                {
                    rightCharacter.Hide();
                }

                currentRightCharacter = "";
                currentRightStand = null;
                Debug.Log("[DialogueManager] Right character hidden (empty name)");
            }

            return;
        }

        // ===== 왼쪽 캐릭터 처리 =====
        if (position.Contains("left"))
        {
            if (leftCharacter != null)
            {
                if (currentLeftCharacter == characterName)
                {
                    Debug.Log($"[DialogueManager] Left character '{characterName}' already shown, changing expression only");
                    leftCharacter.SetExpression(expression);
                }
                else
                {
                    Debug.Log($"[DialogueManager] Showing new left character: {characterName}");
                    leftCharacter.Show(position);
                    leftCharacter.SetExpression(expression);
                    currentLeftCharacter = characterName;
                }
            }
        }

        // ===== 오른쪽 캐릭터 처리 (동적 전환) =====
        if (position.Contains("right"))
        {
            // 적절한 스탠드 선택
            SpineCharacterStand targetStand = DetermineRightCharacterStand(characterName);

            if (targetStand == null)
            {
                Debug.LogWarning($"[DialogueManager] No stand found for character: {characterName}");
                return;
            }

            // 이전 캐릭터와 다른 스탠드면 교체
            if (currentRightStand != targetStand)
            {
                // 이전 스탠드 숨김
                if (currentRightStand != null)
                {
                    currentRightStand.Hide();
                    Debug.Log($"[DialogueManager] Hiding previous right stand: {currentRightStand.GetCharacterName()}");
                }

                // 새 스탠드 표시
                Debug.Log($"[DialogueManager] Showing new right character: {characterName} on {targetStand.name}");
                targetStand.Show(position);
                targetStand.SetExpression(expression);

                currentRightStand = targetStand;
                currentRightCharacter = characterName;
            }
            // 같은 캐릭터면 표정만 변경
            else if (currentRightCharacter == characterName)
            {
                Debug.Log($"[DialogueManager] Right character '{characterName}' already shown, changing expression only");
                targetStand.SetExpression(expression);
            }
            // 같은 스탠드지만 다른 캐릭터 (오류 방지)
            else
            {
                Debug.Log($"[DialogueManager] Showing character '{characterName}' on same stand");
                targetStand.Show(position);
                targetStand.SetExpression(expression);
                currentRightCharacter = characterName;
            }
        }
    }

    /// <summary>
    /// 캐릭터 이름에 따라 적절한 오른쪽 스탠드 결정
    /// </summary>
    private SpineCharacterStand DetermineRightCharacterStand(string characterName)
    {
        // 빌런들인 경우 rightGroupCharacter 사용
        if (characterName == "빌런들" || characterName.Contains("빌런"))
        {
            if (rightGroupCharacter != null)
            {
                Debug.Log($"[DialogueManager] Using rightGroupCharacter for: {characterName}");
                return rightGroupCharacter;
            }
        }

        // 그 외에는 기본 rightCharacter 사용
        if (rightCharacter != null)
        {
            Debug.Log($"[DialogueManager] Using rightCharacter for: {characterName}");
            return rightCharacter;
        }

        return null;
    }

    /// <summary>
    /// 다음 대화로 진행
    /// </summary>
    public void ShowNextDialogue()
    {
        currentDialogueIndex++;
        ShowCurrentDialogue();
    }

    /// <summary>
    /// 대화 종료 후 다음 씬으로 이동
    /// </summary>
    public void EndDialogueAndMoveToNextScene()
    {
        Debug.Log("[DialogueManager] All dialogues complete. Moving to next scene...");

        // 대화 종료
        EndDialogue();

        // 다음 씬으로 이동
        MoveToNextScene();
    }

    /// <summary>
    /// Skip 버튼으로 대화 스킵하고 다음 씬으로
    /// </summary>
    public void SkipDialogueAndMoveToNextScene()
    {
        Debug.Log("[DialogueManager] Skipping all dialogues...");

        // 대화 종료
        EndDialogue();

        // 다음 씬으로 이동
        MoveToNextScene();
    }

    /// <summary>
    /// 대화 종료 (UI 정리)
    /// </summary>
    public void EndDialogue()
    {
        isDialogueActive = false;

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        // Skip 버튼 비활성화
        if (skipButton != null)
        {
            skipButton.gameObject.SetActive(false);
        }

        if (leftCharacter != null)
        {
            leftCharacter.Hide();
        }

        // 오른쪽 캐릭터 동적 숨김
        if (currentRightStand != null)
        {
            currentRightStand.Hide();
        }
        else if (rightCharacter != null)
        {
            rightCharacter.Hide();
        }

        // 그룹 캐릭터도 명시적으로 숨김
        if (rightGroupCharacter != null)
        {
            rightGroupCharacter.Hide();
        }

        Debug.Log("[DialogueManager] Dialogue ended");
    }

    /// <summary>
    /// 다음 씬으로 이동
    /// </summary>
    public void MoveToNextScene()
    {
        // SceneService 사용
        if (useSceneService && sceneService != null)
        {
            Debug.Log("[DialogueManager] Using SceneService to load next scene");

            // SceneService의 LoadNextScene() 또는 특정 씬 로드
            // 예: sceneService.LoadScene("NextSceneName");

            // 만약 SceneService에 LoadNextScene() 같은 메서드가 있다면:
            // sceneService.LoadNextScene();

            // 또는 특정 씬 이름으로 로드:
            if (!string.IsNullOrEmpty(nextSceneName))
            {
                Debug.Log($"[DialogueManager] Loading scene: {nextSceneName}");
                // sceneService.LoadSceneWithLoading(nextSceneName);

                // SceneService가 없다면 직접 로드:
                sceneService.LoadSceneWithLoading(nextSceneName);
            }
            else
            {
                Debug.LogWarning("[DialogueManager] Next scene name is not set!");
            }
        }
        // SceneService 없이 직접 씬 로드
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
    /// 외부에서 다음 씬 이름 설정
    /// </summary>
    public void SetNextScene(string sceneName)
    {
        nextSceneName = sceneName;
        Debug.Log($"[DialogueManager] Next scene set to: {nextSceneName}");
    }
}