using System;
using System.Collections.Generic;

namespace ProjectABC.Core
{
    public static class ContextEventBroadcaster
    {
        private static readonly Dictionary<Type, List<HandlerEntry>> HandlerEntries = new Dictionary<Type, List<HandlerEntry>>();

        private class HandlerEntry
        {
            public readonly object Target;
            private readonly Action<IContextEvent> _callback;

            private HandlerEntry(object target, Action<IContextEvent> callback)
            {
                Target = target;
                _callback = callback;
            }

            public static HandlerEntry GetSubscriberHandle<T>(IContextEventListener<T> subscriber) where T : class, IContextEvent
            {
                return new HandlerEntry(subscriber, e => subscriber.OnEvent((T)e));
            }

            public void Publish(IContextEvent contextEvent)
            {
                _callback.Invoke(contextEvent);
            }
        }
        
        public static void Subscribe<T>(IContextEventListener<T> listener) where T : class, IContextEvent
        {
            Type contextEventType = typeof(T);

            if (!HandlerEntries.ContainsKey(contextEventType))
            {
                HandlerEntries.Add(contextEventType, new List<HandlerEntry>());
            }

            if (HandlerEntries[contextEventType].Exists(entry => ReferenceEquals(entry.Target, listener)))
            {
                return;
            }
            
            HandlerEntry entry = HandlerEntry.GetSubscriberHandle(listener);
            HandlerEntries[contextEventType].Add(entry);
        }

        public static void Unsubscribe<T>(IContextEventListener<T> listener) where T : class, IContextEvent
        {
            Type contextEventType = typeof(T);

            if (!HandlerEntries.TryGetValue(contextEventType, out var entries))
            {
                return;
            }

            entries.RemoveAll(entry => ReferenceEquals(entry.Target, listener));
            if (entries.Count == 0)
            {
                HandlerEntries.Remove(contextEventType);
            }
        }

        public static void Publish<T>(this T contextEvent) where T : class, IContextEvent
        {
            Type eventContextType = contextEvent.GetType();
            
            if (!HandlerEntries.TryGetValue(eventContextType, out var entries))
            {
                return;
            }

            foreach (HandlerEntry entry in entries)
            {
                entry.Publish(contextEvent);
            }
        }
    }

    public static class ContextEventRegister
    {
        public static void StartListening<T>(this IContextEventListener<T> caller) where T : class, IContextEvent
        {
            ContextEventBroadcaster.Subscribe(caller);
        }

        public static void StopListening<T>(this IContextEventListener<T> caller) where T : class, IContextEvent
        {
            ContextEventBroadcaster.Unsubscribe(caller);
        }
    }

    public interface IContextEventListener<in T> where T : class, IContextEvent
    {
        public void OnEvent(T contextEvent);
    }
}