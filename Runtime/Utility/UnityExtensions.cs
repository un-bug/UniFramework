using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 对 Unity 的扩展方法。
/// </summary>
public static class UnityExtensions
{
    private static readonly List<Transform> s_CachedTransforms = new List<Transform>();

    /// <summary>
    /// 获取或增加组件。
    /// </summary>
    /// <typeparam name="T">要获取或增加的组件。</typeparam>
    /// <param name="gameObject">目标对象。</param>
    /// <returns>获取或增加的组件。</returns>
    public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
    {
        T component = gameObject.GetComponent<T>();
        if (component == null)
        {
            component = gameObject.AddComponent<T>();
        }

        return component;
    }

    /// <summary>
    /// 递归设置游戏对象的层次。
    /// </summary>
    /// <param name="gameObject"><see cref="GameObject" /> 对象。</param>
    /// <param name="layer">目标层次的编号。</param>
    public static void SetLayerRecursively(this GameObject gameObject, int layer)
    {
        gameObject.GetComponentsInChildren(true, s_CachedTransforms);
        for (int i = 0; i < s_CachedTransforms.Count; i++)
        {
            s_CachedTransforms[i].gameObject.layer = layer;
        }

        s_CachedTransforms.Clear();
    }

    /// <summary>
    /// 在指定延迟时间后调用回调函数。
    /// </summary>
    /// <param name="monoBehaviour">扩展方法的目标 MonoBehaviour，用于启动协程。</param>
    /// <param name="delay">延迟时间（秒）。</param>
    /// <param name="callback">延迟后执行的回调方法。</param>
    public static void DelayedCall(this MonoBehaviour monoBehaviour, float delay, Action callback)
    {
        if (monoBehaviour)
        {
            monoBehaviour.StartCoroutine(DelayCoroutine());
        }

        IEnumerator DelayCoroutine()
        {
            yield return new WaitForSeconds(delay);
            callback?.Invoke();
        }
    }

    /// <summary>
    /// 递归查找指定名称的子物体（深度优先）
    /// </summary>
    public static Transform FindDeepChild(this Transform parent, string childName)
    {
        if (parent == null || string.IsNullOrEmpty(childName))
        {
            return null;
        }

        foreach (Transform child in parent)
        {
            if (child.name == childName)
            {
                return child;
            }

            Transform found = child.FindDeepChild(childName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }

    /// <summary>
    /// 添加指定数量的子物体。
    /// </summary>
    /// <param name="transform"></param>
    /// <param name="num">要添加的子物体数量。</param>
    /// <param name="childPrefab">要添加物体的预制体。</param>
    public static void AddChildren(this Transform transform, int num, GameObject childPrefab)
    {
        for (int i = 0; i < num; i++)
        {
            Transform child = transform.GetChild(i);
            if (child == null)
            {
                child = UnityEngine.Object.Instantiate(childPrefab, transform).transform;
            }

            child.gameObject.SetActive(true);
        }

        for (int i = num; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child)
            {
                child.gameObject.SetActive(false);
            }
        }
    }
}