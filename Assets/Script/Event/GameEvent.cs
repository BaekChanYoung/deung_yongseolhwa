using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/GameEvent")]
public class GameEvent : ScriptableObject
{
    [Header("Debug")]
    [Tooltip("이벤트 발생 시 로그 출력")]
    public bool enableLogging = false;

    [Tooltip("이벤트 발생 횟수 추적")]
    public bool trackInvocations = false;

    [SerializeField, ReadOnly]
    private int invocationCount = 0;

    private readonly List<UnityAction> listeners = new List<UnityAction>();

    /// <summary>
    /// 리스너 등록
    /// </summary>
    public void Register(UnityAction listener)
    {
        if (listener == null)
        {
            Debug.LogError($"[GameEvent:{name}] Cannot register null listener!");
            return;
        }

        if (listeners.Contains(listener))
        {
            Debug.LogWarning($"[GameEvent:{name}] Listener already registered!");
            return;
        }

        listeners.Add(listener);

        if (enableLogging)
            Debug.Log($"[GameEvent:{name}] ✓ Registered listener ({listeners.Count} total)");
    }

    /// <summary>
    /// 리스너 해제
    /// </summary>
    public void Unregister(UnityAction listener)
    {
        if (!listeners.Remove(listener))
        {
            Debug.LogWarning($"[GameEvent:{name}] Listener not found for removal!");
            return;
        }

        if (enableLogging)
            Debug.Log($"[GameEvent:{name}] Unregistered listener ({listeners.Count} remaining)");
    }

    /// <summary>
    /// 이벤트 발생
    /// </summary>
    public void Raise()
    {
        if (trackInvocations)
            invocationCount++;

        if (enableLogging)
            Debug.Log($"[GameEvent:{name}] Raised! (Listeners: {listeners.Count}, Count: {invocationCount})");

        // 역순 순회 (Remove 시 안전)
        for (int i = listeners.Count - 1; i >= 0; i--)
        {
            try
            {
                listeners[i]?.Invoke();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[GameEvent:{name}] Exception in listener: {e.Message}");
            }
        }
    }

    /// <summary>
    /// 모든 리스너 제거
    /// </summary>
    public void Clear()
    {
        listeners.Clear();

        if (enableLogging)
            Debug.Log($"[GameEvent:{name}] All listeners cleared");
    }

    /// <summary>
    /// 리스너 수 반환
    /// </summary>
    public int GetListenerCount() => listeners.Count;

    /// <summary>
    /// 통계 초기화 (디버깅용)
    /// </summary>
    [ContextMenu("Reset Statistics")]
    public void ResetStatistics()
    {
        invocationCount = 0;
        Debug.Log($"[GameEvent:{name}] Statistics reset");
    }
}

