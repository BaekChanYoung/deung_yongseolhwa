using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
public class ButtonAttribute : PropertyAttribute
{
    public readonly string Label;

    public ButtonAttribute(string label = null)
    {
        Label = label;
    }
}
