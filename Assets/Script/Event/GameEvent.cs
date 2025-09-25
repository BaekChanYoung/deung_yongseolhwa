using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(menuName = "Events/GameEvent")]
public class GameEvent : ScriptableObject
{
    private readonly System.Collections.Generic.List<UnityAction> listeners = new System.Collections.Generic.List<UnityAction>();
    public void Register(UnityAction listener) { listeners.Add(listener); }
    public void Unregister(UnityAction listener) { listeners.Remove(listener); }
    public void Raise() { for (int i = 0; i < listeners.Count; i++) listeners[i].Invoke(); }
}
