using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class ConfigTable<T> : ConfigTableBase, IEnumerable<T> where T : ConfigTableRow
{
    [SerializeField]
    private List<T> m_Data;
    
    public int Count => m_Data != null ? m_Data.Count : 0;
    
    public bool IsEmpty => Count == 0;

    public T this[int index] => m_Data[index];

    public IReadOnlyList<T> Rows => m_Data;

    public bool TryGetById(int id, out T row)
    {
        if (m_Data != null)
        {
            for (int i = 0; i < m_Data.Count; i++)
            {
                T item = m_Data[i];
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
        return (m_Data ?? EmptyData()).GetEnumerator();
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