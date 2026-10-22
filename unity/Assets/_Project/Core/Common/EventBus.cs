using System;
using System.Collections.Generic;

namespace FootbalSim.Core.Common
{
    /// <summary>Synchronous typed pub/sub. Events are structs to avoid per-publish allocation.</summary>
    public interface IEventBus
    {
        void Subscribe<T>(Action<T> handler) where T : struct;
        void Unsubscribe<T>(Action<T> handler) where T : struct;
        void Publish<T>(T evt) where T : struct;
        void Clear();
    }

    public sealed class EventBus : IEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> _handlers = new Dictionary<Type, List<Delegate>>();

        public void Subscribe<T>(Action<T> handler) where T : struct
        {
            if (handler == null) return;
            if (!_handlers.TryGetValue(typeof(T), out List<Delegate> list))
            {
                list = new List<Delegate>(4);
                _handlers[typeof(T)] = list;
            }
            if (!list.Contains(handler)) list.Add(handler);
        }

        public void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            if (handler == null) return;
            if (_handlers.TryGetValue(typeof(T), out List<Delegate> list))
                list.Remove(handler);
        }

        public void Publish<T>(T evt) where T : struct
        {
            if (!_handlers.TryGetValue(typeof(T), out List<Delegate> list) || list.Count == 0)
                return;

            // Copy so handlers may unsubscribe while publishing.
            var snapshot = list.ToArray();
            for (int i = 0; i < snapshot.Length; i++)
                (snapshot[i] as Action<T>)?.Invoke(evt);
        }

        public void Clear() => _handlers.Clear();
    }
}
