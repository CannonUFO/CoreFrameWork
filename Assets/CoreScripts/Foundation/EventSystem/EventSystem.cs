#if UNITY_EDITOR
//#define DEBUG_EVENTSYSTEM
//#define USE_PROFILER
#endif  //UNITY_EDITOR
using ible.Foundation.ObjectPool;
using System;
using System.Collections.Generic;
using UnityEngine;
#if USE_PROFILER
using ible.Foundation.Utility;
#endif  //USE_PROFILER

namespace ible.Foundation.EventSystem
{
    /// <summary>
    /// 基礎註冊、配發的管理器
    /// partial 是因為 EventListener 比較複雜不想寫在 EventSystem 內, 實作拉到EventListener.cs完成
    /// </summary>
    public partial class EventSystem
    {
        protected struct EventHash
        {
            public Type type;
            public int tag;
            public object attachObject;

            public EventHash(Type type, int tag, object attachObject)
            {
                this.type = type;
                this.tag = tag;
                this.attachObject = attachObject;
            }
        }

        protected class EventHashCompare : IEqualityComparer<EventHash>
        {
            public bool Equals(EventHash x, EventHash y)
            {
                return (x.type == y.type && x.attachObject == y.attachObject && x.tag == y.tag);
            }

            public int GetHashCode(EventHash obj)
            {
                int hash_type_object = obj.type.GetHashCode() ^ obj.attachObject.GetHashCode() ^ obj.tag;
                return hash_type_object;
            }
        }

        private Queue<IEvent> _eventQueue = new Queue<IEvent>(32);

        private Dictionary<Type, HashSet<IEventListener>> _listenersMap =
            new Dictionary<Type, HashSet<IEventListener>>(67);

        //private Dictionary<EventHash, IEventListener> _listenersHashMap =
        //    new Dictionary<EventHash, IEventListener>();

        private Dictionary<object, HashSet<IEventListener>> _objectToListeners =
            new Dictionary<object, HashSet<IEventListener>>(67);

        private Dictionary<Type, Dictionary<int, HashSet<IEventListener>>> _typeToTagListeners =
            new Dictionary<Type, Dictionary<int, HashSet<IEventListener>>>(67);

        private IEventListener[] _buffer;

        /// <summary>
        ///     實際執行事件發送的函式
        /// </summary>
        public void ExecuteProcess()
        {
            //if(!_isProcessing)
            //    return;

            while (_eventQueue.Count > 0)
            {
                IEvent evt = _eventQueue.Dequeue();

                InvokeEvent(evt);
            }

            //_isProcessing = false;
        }

        /// <summary>
        ///     發送事件訊息
        /// </summary>
        /// <param name="evt"></param>
        /// <param name="forceNow">是否立刻發送事件</param>
        public void SendEvent(IEvent evt, bool forceNow = false)
        {
            if (forceNow)
            {
                InvokeEvent(evt);
                return;
            }

            _eventQueue.Enqueue(evt);
        }

        /// <summary>
        ///     註冊監聽事件訊息
        /// </summary>
        /// <typeparam name="TListen">欲監聽的事件類別</typeparam>
        /// <param name="listener">已經建立的監聽器物件, 反註冊, 或新增條件時使用</param>
        public void RegisterListener<TListen>(IEventListener<TListen> listener) where TListen : class, IEvent
        {
            HashSet<IEventListener> listeners;
            Type eventType = typeof(TListen);

            _listenersMap.TryGetValue(eventType, out listeners);
            if (listeners == null)
            {
                listeners = new HashSet<IEventListener>();
                _listenersMap.Add(eventType, listeners);
            }

            if (!listeners.Contains(listener))
            {
                listeners.Add(listener);
            }

            if (listener.EventTag.type != EventTagData.Type.None)
            {
                AddToTagDictionary<TListen>(listener, listener.EventTag);
            }
            if (listener.AttachObject != null)
            {
                AddToObjectDictionary(listener, listener.AttachObject);
            }
        }

        /// <summary>
        ///     註冊監聽事件訊息
        /// </summary>
        /// <typeparam name="TListen">欲監聽的事件類別</typeparam>
        /// <param name="callback">事件發生的callback</param>
        /// <returns>監聽器物件, 反註冊, 或新增條件時使用</returns>
        public IEventListener<TListen> RegisterListener<TListen>(Action<TListen> callback)
            where TListen : class, IEvent
        {
            return RegisterListener(callback, null, EventTagData.None);
        }

        /// <summary>
        ///     註冊監聽事件訊息
        /// </summary>
        /// <typeparam name="TListen">欲監聽的事件類別</typeparam>
        /// <param name="callback">事件發生的callback</param>
        /// <param name="tag">給監聽器的標記, 可以方便發送者指定特定標記發送事件</param>
        /// <returns></returns>
        public IEventListener<TListen> RegisterListenerWithTag<TListen>(Action<TListen> callback, EventTagData tag)
            where TListen : class, IEvent
        {
            return RegisterListener(callback, null, tag);
        }

        /// <summary>
        ///     註冊監聽事件訊息
        /// </summary>
        /// <typeparam name="TListen">欲監聽的事件類別</typeparam>
        /// <param name="callback">事件發生的callback</param>
        /// <param name="obj">監聽器綁定於特定物件, 方便針對特定物件反註冊所有綁定於此物件的監聽器</param>
        /// <returns></returns>
        public IEventListener<TListen> RegisterListenerWithObject<TListen>(Action<TListen> callback, object obj)
            where TListen : class, IEvent
        {
            return RegisterListener(callback, obj, EventTagData.None);
        }

        /// <summary>
        ///     註冊監聽事件訊息
        /// </summary>
        /// <typeparam name="TListen">欲監聽的事件類別</typeparam>
        /// <param name="callback">事件發生的callback</param>
        /// <param name="obj">監聽器綁定於特定物件, 方便針對特定物件反註冊所有綁定於此物件的監聽器</param>
        /// <param name="tag">給監聽器的標記, 可以方便發送者指定特定標記發送事件</param>
        /// <returns></returns>
        public IEventListener<TListen> RegisterListenerWithTagAndObject<TListen>(Action<TListen> callback,
            object obj, EventTagData tag)
            where TListen : class, IEvent
        {
            return RegisterListener(callback, obj, tag);
        }

        /// <summary>
        ///     針對特定物件反註冊所有監聽器
        /// </summary>
        /// <param name="obj">監聽器綁定的特定物件, 針對特定物件反註冊所有綁定於此物件的監聽器</param>
        public void UnregisterAll(object obj)
        {
            if (obj == null)
            {
                return;
            }

            HashSet<IEventListener> listeners;
            _objectToListeners.TryGetValue(obj, out listeners);
            _objectToListeners.Remove(obj);

            if (listeners == null)
            {
                return;
            }

            foreach (IEventListener listener in listeners)
            {
                listener.Unregister();
            }
        }

        /// <summary>
        ///     取消監聽 ( 這是給EventListener呼叫的函式 )
        /// </summary>
        /// <param name="listener"></param>
        private void Unregister(IEventListener listener)
        {
            HashSet<IEventListener> listeners = null;
            var type = listener.GetEventType();
            if (type != null)
            {
                _listenersMap.TryGetValue(type, out listeners);
            }
            if (listeners != null)
                listeners.Remove(listener);
            object obj = listener.AttachObject;
            if (obj != null)
            {
                _objectToListeners.TryGetValue(obj, out listeners);
                if (listeners != null)
                    listeners.Remove(listener);
            }

            EventTagData tag = listener.EventTag;
            if (tag.type != EventTagData.Type.None)
            {
                Type eventType = listener.GetEventType();
                Dictionary<int, HashSet<IEventListener>> tagToListeners;
                _typeToTagListeners.TryGetValue(eventType, out tagToListeners);
                if (tagToListeners != null)
                    tagToListeners.TryGetValue(tag.hashCode, out listeners);
                if (listeners != null)
                    listeners.Remove(listener);
            }

            listener.Release();
        }

        private IEventListener<TListen> RegisterListener<TListen>(Action<TListen> callback, object obj,
            EventTagData tag)
            where TListen : class, IEvent
        {
            if (callback == null)
            {
                return null;
            }

            IEventListener<TListen> listener = AddToTypeDictionary(callback, obj, tag);

            if (tag.type != EventTagData.Type.None)
            {
                AddToTagDictionary<TListen>(listener, tag);
            }
            if (obj != null)
            {
                AddToObjectDictionary(listener, obj);
            }

            return listener;
        }

        private IEventListener<TListen> AddToTypeDictionary<TListen>(Action<TListen> callback, object obj,
            EventTagData tag) where TListen : class, IEvent
        {
            HashSet<IEventListener> listeners;
            Type eventType = typeof(TListen);

            _listenersMap.TryGetValue(eventType, out listeners);
            if (listeners == null)
            {
                listeners = new HashSet<IEventListener>();
                _listenersMap.Add(eventType, listeners);
            }

            EventListener<TListen> listener = EventListener<TListen>.Allocate(this, obj, tag, callback);
            listeners.Add(listener);

            return listener;
        }

        private void AddToTagDictionary<TListen>(IEventListener listener, EventTagData tag)
            where TListen : class, IEvent
        {
            Type listenType = typeof(TListen);

            Dictionary<int, HashSet<IEventListener>> tagToListeners;
            _typeToTagListeners.TryGetValue(listenType, out tagToListeners);
            if (tagToListeners == null)
            {
                tagToListeners = new Dictionary<int, HashSet<IEventListener>>();
                _typeToTagListeners.Add(listenType, tagToListeners);
            }

            HashSet<IEventListener> listeners;
            tagToListeners.TryGetValue(tag.hashCode, out listeners);
            if (listeners == null)
            {
                listeners = new HashSet<IEventListener>();
                tagToListeners.Add(tag.hashCode, listeners);
            }

            if (!listeners.Contains(listener))
            {
                listeners.Add(listener);
            }
        }

        private void AddToObjectDictionary(IEventListener listener, object obj)
        {
            HashSet<IEventListener> listeners;
            _objectToListeners.TryGetValue(obj, out listeners);
            if (listeners == null)
            {
                listeners = new HashSet<IEventListener>();
                _objectToListeners.Add(obj, listeners);
            }
            if (!listeners.Contains(listener))
            {
                listeners.Add(listener);
            }
        }
#if USE_PROFILER
        private Dictionary<Type, string> _typeMapping = new Dictionary<Type, string>();
#endif  //USE_PROFILER
        private void InvokeEvent(IEvent evt)
        {
            do
            {
                HashSet<IEventListener> listeners;
                var type = evt.GetType();
#if USE_PROFILER
                if (_typeMapping.TryGetValue(type, out var key) == false)
                {
                    key = type.ToString();
                    _typeMapping.Add(type, key);
                }
#endif  //USE_PROFILER
                if (!_listenersMap.TryGetValue(type, out listeners))
                {
                    break;
                }

                EventTagData tag = evt.EventTag;

                if (tag.type == EventTagData.Type.None)
                {
#if USE_PROFILER
                    Profiler.BeginSample(key);
                    InvokeAll(evt, listeners);
                    Profiler.EndSample(key);
#else
                    InvokeAll(evt, listeners);
#endif  //USE_PROFILER
                }
                else
                {
                    // 針對特定tag發送
                    Type listenType = evt.EventType;

                    Dictionary<int, HashSet<IEventListener>> tagToListeners;
                    if (!_typeToTagListeners.TryGetValue(listenType, out tagToListeners))
                    {
                        break;
                    }

                    if (!tagToListeners.TryGetValue(tag.hashCode, out listeners))
                    {
                        break;
                    }
#if USE_PROFILER
                    Profiler.BeginSample("InvokeAllTag");
                    InvokeAll(evt, listeners);
                    Profiler.EndSample("InvokeAllTag");
#else
                    InvokeAll(evt, listeners);
#endif  //USE_PROFILER
                }
            } while (false);

            IShareObject refObj = evt as IShareObject;
            if (refObj != null)
            {
                refObj.Release();
            }
        }
#if DEBUG_EVENTSYSTEM
        private int _traceEventCount = 0;
#endif
        private void InvokeAll(IEvent evt, HashSet<IEventListener> listeners)
        {
            if (_buffer == null || _buffer.Length < listeners.Count)
            {
                _buffer = new IEventListener[listeners.Count + 100]; // 一次+100 減少產生垃圾
            }
            listeners.CopyTo(_buffer);
            int size = listeners.Count;
#if DEBUG_EVENTSYSTEM
            if (size > 10 && size > _traceEventCount)
            {
                Debug.Log($"InvokeAll of event {evt.EventType.Name} to {size} listeners", LogTag.EventSystem);
                _traceEventCount = size;
            }
            else
            {
                if (_traceEventCount >= 1)
                {
                    _traceEventCount -= 1;
                }
            }
#endif
            for (int i = 0; i < size; i++)
            {
                try
                {
                    _buffer[i].Invoke(evt);
                }
                catch (Exception e)
                {
                    Debug.LogErrorFormat("Invoke Event happened exception e: {0}", e);
                }
            }
        }
    }
}