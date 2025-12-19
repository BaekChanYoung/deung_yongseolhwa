using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// DialogueScene 초기화 및 완료 처리
/// DialogueManager와 연동하여 최초 실행 플래그 관리
/// </summary>
public class DialogueSceneController : MonoBehaviour
{
    [Header("References")]
    [Tooltip("DialogueManager (자동 감지 가능)")]
    public DialogueManager dialogueManager;

    [Header("Events")]
    public UnityEvent onDialogueWillComplete;

    [Header("설정")]
    [Tooltip("다이얼로그 완료 후 이동할 씬")]
    public string nextSceneName = SceneNames.START;

    [Tooltip("자동으로 최초 실행 완료 표시 (권장: true)")]
    public bool autoMarkFirstTimeComplete = true;

    private bool isCompleted = false;

    void Awake()
    {
        // DialogueManager 자동 감지
        if (dialogueManager == null)
        {
            dialogueManager = FindObjectOfType<DialogueManager>();
        }

        if (dialogueManager == null)
        {
            Debug.LogError("[DialogueSceneController] DialogueManager를 찾을 수 없습니다!");
        }
    }

    void Start()
    {
        // DialogueManager에 다음 씬 설정
        if (dialogueManager != null)
        {
            dialogueManager.SetNextScene(nextSceneName);
            Debug.Log($"[DialogueSceneController] DialogueManager에 다음 씬 설정: {nextSceneName}");
        }

        Debug.Log("[DialogueSceneController] 초기화 완료");
    }

    void OnEnable()
    {
        // DialogueManager가 씬 전환을 시작하기 전에 최초 실행 완료 표시
        if (dialogueManager != null)
        {
            // DialogueManager의 MoveToNextScene() 호출 전에 실행되도록 설정
            // 방법: DialogueManager를 약간 수정하거나, 여기서 직접 체크
        }
    }

    /// <summary>
    /// 다이얼로그 완료 처리 (DialogueManager에서 호출 가능)
    /// </summary>
    public void OnDialogueComplete()
    {
        if (isCompleted) return;

        isCompleted = true;

        Debug.Log("========================================");
        Debug.Log("[DialogueSceneController] 다이얼로그 완료!");
        Debug.Log("========================================");

        // 최초 실행 완료 표시
        if (autoMarkFirstTimeComplete)
        {
            FirstTimeManager.MarkAsCompleted();
        }

        Debug.Log("[DialogueSceneController] 씬 전환 준비 완료");

        onDialogueWillComplete?.Invoke();

        // 최초 실행 완료 표시
        if (autoMarkFirstTimeComplete)
        {
            FirstTimeManager.MarkAsCompleted();
        }
    }

    /// <summary>
    /// 수동으로 다이얼로그 완료 처리 (외부 호출용)
    /// </summary>
    public void CompleteDialogue()
    {
        OnDialogueComplete();

        // DialogueManager를 통해 씬 전환
        if (dialogueManager != null)
        {
            dialogueManager.EndDialogueAndMoveToNextScene();
        }
    }
}