using System;
using System.Collections.Generic;

public static class ServiceLocator
{
    static readonly Dictionary<Type, object> services = new Dictionary<Type, object>();

    public static void Register<T>(T service) where T : class
    {
        var t = typeof(T);
        if (services.ContainsKey(t)) services[t] = service;
        else services.Add(t, service);
    }

    public static void Unregister<T>() where T : class
    {
        var t = typeof(T);
        if (services.ContainsKey(t)) services.Remove(t);
    }

    public static T Resolve<T>() where T : class
    {
        var t = typeof(T);
        if (services.TryGetValue(t, out var s)) return s as T;
        return null;
    }
}
