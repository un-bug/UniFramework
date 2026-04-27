using System;

public abstract class ConfigTableRow
{
    public abstract int ID { get; }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ConfigTableAttribute : Attribute
{
    public readonly string Name;

    public ConfigTableAttribute(string name)
    {
        Name = name;
    }
}
