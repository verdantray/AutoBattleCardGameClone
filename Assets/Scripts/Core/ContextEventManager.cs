using System;
using System.Collections.Generic;
using System.Linq;

namespace ProjectABC.Core
{
    public static class ContextEventManager
    {
        private static readonly Dictionary<Type, List<IContextEventListener>> SubscribersMap = new Dictionary<Type, List<IContextEventListener>>();

        public static void Subscribe<T>(IContextEventListener listener) where T : class, IContextEvent
        {
            Type contextEventType = typeof(T);

            if (!SubscribersMap.ContainsKey(contextEventType))
            {
                SubscribersMap[contextEventType] = new List<IContextEventListener>();
            }

            if (SubscriberExists(contextEventType, listener))
            {
                return;
            }
            
            SubscribersMap[contextEventType].Add(listener);
        }

        public static void Unsubscribe<T>(IContextEventListener listener) where T : class, IContextEvent
        {
            Type contextEventType = typeof(T);

            if (!SubscriberExists(contextEventType, listener))
            {
                return;
            }
            
            SubscribersMap[contextEventType].Remove(listener);
        }

        private static bool SubscriberExists(Type contextEventType, IContextEventListener listener)
        {
            return SubscribersMap.TryGetValue(contextEventType, out var subscribers)
                   && subscribers.Contains(listener);
        }

        public static void TriggerEvent<T>(this T contextEvent) where T : class, IContextEvent
        {
            if (!SubscribersMap.TryGetValue(typeof(T), out var subscribers))
            {
                return;
            }

            foreach (var listener in subscribers.Cast<IContextEventListener<T>>())
            {
                listener.OnEvent(contextEvent);
            }
        }
    }

    public interface IContextEventListener
    {
        
    }

    public interface IContextEventListener<in T> : IContextEventListener where T : class, IContextEvent
    {
        public void RegisterHandler()
        {
            ContextEventManager.Subscribe<T>(this);
        }

        public void UnregisterHandler()
        {
            ContextEventManager.Unsubscribe<T>(this);
        }

        public void OnEvent(T contextEvent);
    }
}