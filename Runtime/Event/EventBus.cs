using System;
using System.Collections.Generic;
using System.Threading;

namespace UniFramework
{
    public interface IEvent { }

    /// <summary>
    /// 事件总线。
    /// </summary>
    public static class EventBus
    {
        private static readonly List<IEventStream> m_EventStreams = new List<IEventStream>();
        private static readonly object m_Lock = new object();
        private static Action<Type, Delegate, Exception> m_HandlerExceptionReporter;

        /// <summary>
        /// 获取或设置处理器异常报告器。报告器接收事件类型、失败的处理器和异常。
        /// </summary>
        public static Action<Type, Delegate, Exception> HandlerExceptionReporter
        {
            get { return Volatile.Read(ref m_HandlerExceptionReporter); }
            set { Volatile.Write(ref m_HandlerExceptionReporter, value); }
        }

        /// <summary>
        /// 订阅指定类型的事件。
        /// </summary>
        public static IDisposable Subscribe<TEvent>(Action<TEvent> handler) where TEvent : struct, IEvent
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            if (handler.GetInvocationList().Length != 1)
            {
                throw new ArgumentException("Multicast handlers are not supported.", nameof(handler));
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
                foreach (var stream in m_EventStreams)
                {
                    stream.Clear();
                }
            }
        }

        private static EventStream<TEvent> GetStream<TEvent>() where TEvent : struct, IEvent
        {
            return EventStreamCache<TEvent>.Instance;
        }

        private static EventStream<TEvent> RegisterStream<TEvent>() where TEvent : struct, IEvent
        {
            var stream = new EventStream<TEvent>();

            lock (m_Lock)
            {
                m_EventStreams.Add(stream);
            }

            return stream;
        }

        private static class EventStreamCache<TEvent> where TEvent : struct, IEvent
        {
            internal static readonly EventStream<TEvent> Instance = RegisterStream<TEvent>();
        }

        private interface IEventStream
        {
            void Clear();
        }

        private sealed class EventStream<TEvent> : IEventStream where TEvent : struct, IEvent
        {
            private static readonly Action<TEvent>[] s_EmptyHandlers = new Action<TEvent>[0];

            private readonly object m_StreamLock = new object();
            private Action<TEvent>[] m_Handlers = s_EmptyHandlers;
            private Dictionary<Action<TEvent>, Subscription> m_Subscriptions;

            public IDisposable Subscribe(Action<TEvent> handler)
            {
                lock (m_StreamLock)
                {
                    Subscription existingSubscription;
                    if (m_Subscriptions != null && m_Subscriptions.TryGetValue(handler, out existingSubscription))
                    {
                        throw new InvalidOperationException($"Handler '{handler.Method.Name}' is already subscribed to event '{typeof(TEvent).FullName}'.");
                    }

                    var subscription = new Subscription(this, handler);
                    if (m_Subscriptions == null)
                    {
                        m_Subscriptions = new Dictionary<Action<TEvent>, Subscription>();
                    }

                    m_Subscriptions.Add(handler, subscription);

                    var handlers = m_Handlers;
                    var newHandlers = new Action<TEvent>[handlers.Length + 1];
                    Array.Copy(handlers, newHandlers, handlers.Length);
                    newHandlers[handlers.Length] = handler;
                    Volatile.Write(ref m_Handlers, newHandlers);

                    return subscription;
                }
            }

            public void Unsubscribe(Action<TEvent> handler)
            {
                lock (m_StreamLock)
                {
                    if (m_Subscriptions == null || !m_Subscriptions.Remove(handler))
                    {
                        return;
                    }

                    RemoveHandler(handler);
                }
            }

            public void Publish(TEvent evt)
            {
                var snapshot = Volatile.Read(ref m_Handlers);
                List<Exception> unhandledExceptions = null;

                for (var i = 0; i < snapshot.Length; i++)
                {
                    var handler = snapshot[i];
                    try
                    {
                        handler(evt);
                    }
                    catch (Exception exception)
                    {
                        var reporter = Volatile.Read(ref m_HandlerExceptionReporter);
                        if (reporter == null)
                        {
                            if (unhandledExceptions == null)
                            {
                                unhandledExceptions = new List<Exception>();
                            }

                            unhandledExceptions.Add(exception);
                            continue;
                        }

                        try
                        {
                            reporter(typeof(TEvent), handler, exception);
                        }
                        catch (Exception reporterException)
                        {
                            if (unhandledExceptions == null)
                            {
                                unhandledExceptions = new List<Exception>();
                            }

                            unhandledExceptions.Add(exception);
                            unhandledExceptions.Add(reporterException);
                        }
                    }
                }

                if (unhandledExceptions != null)
                {
                    throw new AggregateException($"One or more handlers failed while publishing event '{typeof(TEvent).FullName}'.", unhandledExceptions);
                }
            }

            public void Clear()
            {
                lock (m_StreamLock)
                {
                    Volatile.Write(ref m_Handlers, s_EmptyHandlers);
                    m_Subscriptions = null;
                }
            }

            private void Unsubscribe(Action<TEvent> handler, Subscription subscription)
            {
                lock (m_StreamLock)
                {
                    Subscription currentSubscription;
                    if (m_Subscriptions == null ||
                        !m_Subscriptions.TryGetValue(handler, out currentSubscription) ||
                        !ReferenceEquals(currentSubscription, subscription))
                    {
                        return;
                    }

                    m_Subscriptions.Remove(handler);

                    RemoveHandler(handler);
                }
            }

            private void RemoveHandler(Action<TEvent> handler)
            {
                var handlers = m_Handlers;
                var index = Array.IndexOf(handlers, handler);
                if (index < 0)
                {
                    return;
                }

                if (handlers.Length == 1)
                {
                    Volatile.Write(ref m_Handlers, s_EmptyHandlers);
                    return;
                }

                var newHandlers = new Action<TEvent>[handlers.Length - 1];
                if (index > 0)
                {
                    Array.Copy(handlers, 0, newHandlers, 0, index);
                }

                if (index < handlers.Length - 1)
                {
                    Array.Copy(handlers, index + 1, newHandlers, index, handlers.Length - index - 1);
                }

                Volatile.Write(ref m_Handlers, newHandlers);
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
                        stream.Unsubscribe(handler, this);
                    }
                }
            }
        }
    }
}
