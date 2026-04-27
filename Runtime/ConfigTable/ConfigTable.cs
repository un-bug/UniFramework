using UnityEngine;

[System.Serializable]
public abstract class ConfigTable<T> : ConfigTableBase
{
    [SerializeField]
    public T[] Data;
}
