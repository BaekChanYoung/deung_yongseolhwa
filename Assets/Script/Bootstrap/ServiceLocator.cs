using System;
using System.Collections.Generic;
using UnityEngine;

public static class ServiceLocator
{
    static readonly Dictionary<Type, object> services = new Dictionary<Type, object>();

    // 개선 1: 로깅 옵션
    public static bool EnableLogging = true;

    // 개선 2: 덮어쓰기 방지 옵션
    public static bool AllowOverwrite = false;

    /// <summary>
    /// 서비스 등록 (개선 버전)
    /// </summary>
    public static bool Register<T>(T service) where T : class
    {
        if (service == null)
        {
            LogError($"Cannot register null service for type {typeof(T).Name}");
            return false;
        }

        var type = typeof(T);

        if (services.ContainsKey(type))
        {
            if (!AllowOverwrite)
            {
                LogError($"Service {type.Name} is already registered! Set AllowOverwrite=true to replace.");
                return false;
            }

            LogWarning($"Overwriting existing service {type.Name}");
            services[type] = service;
        }
        else
        {
            services.Add(type, service);
            Log($"✓ Registered service: {type.Name}");
        }

        return true;
    }

    /// <summary>
    /// 서비스 해제 (개선 버전)
    /// </summary>
    public static bool Unregister<T>() where T : class
    {
        var type = typeof(T);

        if (!services.ContainsKey(type))
        {
            LogWarning($"Cannot unregister {type.Name} - not registered");
            return false;
        }

        services.Remove(type);
        Log($"✓ Unregistered service: {type.Name}");
        return true;
    }

    /// <summary>
    /// 서비스 검색 (개선 버전)
    /// </summary>
    public static T Resolve<T>() where T : class
    {
        var type = typeof(T);

        if (services.TryGetValue(type, out var service))
        {
            return service as T;
        }

        LogWarning($"Service {type.Name} not found! Returning null.");
        return null;
    }

    /// <summary>
    /// 서비스 존재 확인
    /// </summary>
    public static bool IsRegistered<T>() where T : class
    {
        return services.ContainsKey(typeof(T));
    }

    /// <summary>
    /// 모든 서비스 초기화 (테스트/디버깅용)
    /// </summary>
    public static void Clear()
    {
        services.Clear();
        Log("All services cleared");
    }

    /// <summary>
    /// 등록된 서비스 목록 출력 (디버깅용)
    /// </summary>
    public static void PrintRegisteredServices()
    {
        if (services.Count == 0)
        {
            Debug.Log("[ServiceLocator] No services registered");
            return;
        }

        Debug.Log($"[ServiceLocator] Registered Services ({services.Count}):");
        foreach (var kvp in services)
        {
            Debug.Log($"  - {kvp.Key.Name} → {kvp.Value.GetType().Name}");
        }
    }

    // 로깅 헬퍼
    static void Log(string message)
    {
        if (EnableLogging)
            Debug.Log($"[ServiceLocator] {message}");
    }

    static void LogWarning(string message)
    {
        if (EnableLogging)
            Debug.LogWarning($"[ServiceLocator] {message}");
    }

    static void LogError(string message)
    {
        Debug.LogError($"[ServiceLocator] {message}");
    }
}