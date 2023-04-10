using ible.Foundation.ObjectPool;
using System;
using System.Collections.Generic;

namespace ible.Foundation.EventSystem
{
    public partial class EventSystem
    {
        /// <summary>
        /// 實作IEventListener
        /// </summary>
        private class EventListener<TListen> : ShareObject<EventListener<TListen>>,
            IEventListener<TListen>
            where TListen : class, IEvent
        {
            private List<Func<TListen, bool>> _conditions = new List<Func<TListen, bool>>();
            private Action<TListen> _listener;
            private EventSystem _eventSystem;

            public bool IsOnce { get; set; }

            public object AttachObject { get; private set; }
            public EventTagData EventTag { get; private set; }

            public string CacheName { get; set; }

            private Type EventType { get; set; }

            private bool IsRegistered { get; set; }

            static EventListener()
            {
                //SetReserveSize(50);
            }

            public static EventListener<TListen> Allocate(EventSystem eventSystem, object obj, EventTagData tag, Action<TListen> l)
            {
                EventListener<TListen> listener = Allocate();
                listener._eventSystem = eventSystem;
                listener._listener = l;
                listener.IsOnce = false;
                listener.AttachObject = obj;
                listener.EventTag = tag;
                listener._conditions.Clear();
                listener.IsRegistered = true;
                listener.CacheName = obj != null ? obj.ToString() : null;
                listener.EventType = typeof(TListen);

                return listener;
            }

            public override void Release()
            {
                base.Release();

                if (IsRegistered)
                {
                    throw new InvalidOperationException("should not call Release() before call Unregister() !!!");
                }
            }

            public void AddCondition(Func<TListen, bool> conditionFunc)
            {
                _conditions.Add(conditionFunc);
            }

            public void Invoke(IEvent evt)
            {
                TListen realEvent = evt as TListen;
                if (realEvent == null)
                {
                    return;
                }

                for (int i = 0; i < _conditions.Count; i++)
                {
                    Func<TListen, bool> condition = _conditions[i];
                    if (!condition.Invoke(realEvent))
                    {
                        return;
                    }
                }

                _listener.Invoke(realEvent);

                if (IsOnce)
                    Unregister();
            }

            public void Unregister()
            {
                if (IsRegistered)
                {
                    IsRegistered = false;
                    _eventSystem.Unregister(this);
                }
            }

            public Type GetEventType()
            {
                return EventType;
            }

            public override void Reset()
            {
                
            }
        }
    }
}