using System;

public interface ISceneService
{
    void LoadSceneWithLoading(string targetSceneName, Action onComplete = null);
    string GetCurrentSceneName();
    bool IsLoading { get; }
}
