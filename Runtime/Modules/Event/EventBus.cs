using System;
using System.Collections.Generic;
using UnityEngine;

namespace UniFramework.Runtime
{
    public interface IEvent { }

    public static class EventBus
    {
        private static readonly Dictionary<int, Delegate> m_SignalTable = new Dictionary<int, Delegate>();
        private static readonly object m_Lock = new object();

        public static void Subscribe(int id, Action handler) => Add(id, handler);

        public static void Subscribe<T>(int id, Action<T> handler) where T : struct, IEvent => Add(id, handler);

        public static void Unsubscribe(int id, Action handler) => Remove(id, handler);

        public static void Unsubscribe<T>(int id, Action<T> handler) where T : struct, IEvent => Remove(id, handler);

        public static void Fire(int id)
        {
            Delegate d;
            lock (m_Lock)
            {
                if (!m_SignalTable.TryGetValue(id, out d))
                {
                    return;
                }
            }

            if (d is Action action)
            {
                action.Invoke();
            }
            else
            {
                Debug.LogError($"[EventBus] id {id} type mismatch (Expected: Action)");
            }
        }

        public static void Fire<T>(int id, T arg) where T : struct, IEvent
        {
            Delegate d;
            lock (m_Lock)
            {
                if (!m_SignalTable.TryGetValue(id, out d))
                {
                    return;
                }
            }

            if (d is Action<T> action)
            {
                action.Invoke(arg);
            }
            else
            {
                Debug.LogError($"[EventBus] id {id} type mismatch (Expected: Action<{typeof(T).Name}>)");
            }
        }

        public static void Clear()
        {
            lock (m_Lock)
            {
                m_SignalTable.Clear();
            }
        }

        private static void Add(int id, Delegate handler)
        {
            lock (m_Lock)
            {
                if (m_SignalTable.TryGetValue(id, out var existing))
                {
                    if (existing != null && existing.GetType() != handler.GetType())
                    {
                        Debug.LogError($"[EventBus] id {id} attempted to register callback with a different type!");
                        return;
                    }
                    m_SignalTable[id] = Delegate.Combine(existing, handler);
                }
                else
                {
                    m_SignalTable[id] = handler;
                }
            }
        }

        private static void Remove(int id, Delegate handler)
        {
            lock (m_Lock)
            {
                if (m_SignalTable.TryGetValue(id, out var existing))
                {
                    var newDel = Delegate.Remove(existing, handler);
                    if (newDel == null)
                    {
                        m_SignalTable.Remove(id);
                    }
                    else
                    {
                        m_SignalTable[id] = newDel;
                    }
                }
            }
        }
    }
}