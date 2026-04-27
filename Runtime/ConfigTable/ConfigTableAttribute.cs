using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Class)]
public class ConfigTableAttribute : PropertyAttribute
{
    public readonly string Name;

    public ConfigTableAttribute(string name)
    {
        Name = name;
    }
}
