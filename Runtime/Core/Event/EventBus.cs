using System;
using System.Collections.Generic;
using System.Threading;

namespace UniFramework
{
    /// <summary>
    /// 标记值类型事件负载。
    /// </summary>
    public interface IEvent
    {
    }

    /// <summary>
    /// 提供项目级类型化事件的订阅与发布。
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, IEventStream> m_EventStreams = new Dictionary<Type, IEventStream>();
        private static readonly object m_Lock = new object();

        /// <summary>
        /// 订阅指定类型的事件。
        /// </summary>
        public static IDisposable Subscribe<TEvent>(Action<TEvent> handler) where TEvent : struct, IEvent
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            return GetStream<TEvent>().Subscribe(handler);
        }

        /// <summary>
        /// 取消订阅指定类型的事件。
        /// </summary>
        public static void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : struct, IEvent
        {
            if (handler == null)
            {
                return;
            }

            GetStream<TEvent>().Unsubscribe(handler);
        }

        /// <summary>
        /// 向指定类型事件的所有订阅者发布消息。
        /// </summary>
        public static void Publish<TEvent>(TEvent evt) where TEvent : struct, IEvent
        {
            GetStream<TEvent>().Publish(evt);
        }

        /// <summary>
        /// 清空所有事件订阅。
        /// </summary>
        public static void Clear()
        {
            lock (m_Lock)
            {
                foreach (var stream in m_EventStreams.Values)
                {
                    stream.Clear();
                }

                m_EventStreams.Clear();
            }
        }

        private static EventStream<TEvent> GetStream<TEvent>() where TEvent : struct, IEvent
        {
            lock (m_Lock)
            {
                var eventType = typeof(TEvent);
                if (!m_EventStreams.TryGetValue(eventType, out var stream))
                {
                    var newStream = new EventStream<TEvent>();
                    m_EventStreams[eventType] = newStream;
                    return newStream;
                }

                return (EventStream<TEvent>)stream;
            }
        }

        private interface IEventStream
        {
            void Clear();
        }

        private sealed class EventStream<TEvent> : IEventStream where TEvent : struct, IEvent
        {
            private readonly object m_StreamLock = new object();
            private Action<TEvent> m_Handlers;

            public IDisposable Subscribe(Action<TEvent> handler)
            {
                lock (m_StreamLock)
                {
                    m_Handlers += handler;
                }

                return new Subscription(this, handler);
            }

            public void Unsubscribe(Action<TEvent> handler)
            {
                lock (m_StreamLock)
                {
                    m_Handlers -= handler;
                }
            }

            public void Publish(TEvent evt)
            {
                Action<TEvent> snapshot;
                lock (m_StreamLock)
                {
                    snapshot = m_Handlers;
                }

                snapshot?.Invoke(evt);
            }

            public void Clear()
            {
                lock (m_StreamLock)
                {
                    m_Handlers = null;
                }
            }

            private sealed class Subscription : IDisposable
            {
                private EventStream<TEvent> m_Stream;
                private Action<TEvent> m_Handler;
                private int m_Disposed;

                public Subscription(EventStream<TEvent> stream, Action<TEvent> handler)
                {
                    m_Stream = stream;
                    m_Handler = handler;
                }

                public void Dispose()
                {
                    if (Interlocked.Exchange(ref m_Disposed, 1) != 0)
                    {
                        return;
                    }

                    var stream = m_Stream;
                    var handler = m_Handler;
                    m_Stream = null;
                    m_Handler = null;

                    if (stream != null && handler != null)
                    {
                        stream.Unsubscribe(handler);
                    }
                }
            }
        }
    }
}
