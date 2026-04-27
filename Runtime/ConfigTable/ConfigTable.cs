using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class ConfigTable<T> : ConfigTableBase, IEnumerable<T> where T : ConfigTableRow
{
    public List<T> Data = new List<T>();
    
    public int Count => Data != null ? Data.Count : 0;
    
    public bool IsEmpty => Count == 0;

    public T this[int index] => Data[index];

    public IReadOnlyList<T> Rows => Data;

    public bool TryGetById(int id, out T row)
    {
        if (Data != null)
        {
            for (int i = 0; i < Data.Count; i++)
            {
                T item = Data[i];
                if (item != null && item.ID == id)
                {
                    row = item;
                    return true;
                }
            }
        }

        row = null;
        return false;
    }

    public T GetById(int id)
    {
        if (TryGetById(id, out T row))
        {
            return row;
        }

        Debug.LogError($"Config row not found: {typeof(T).Name}, ID: {id}");
        return null;
    } 
    
    public bool ContainsId(int id)
    {
        return TryGetById(id, out _);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return (Data ?? EmptyData()).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private static IReadOnlyList<T> EmptyData()
    {
        return System.Array.Empty<T>();
    }
}