using ible.Foundation.Utility;
using System;
using UnityEngine;

namespace ible.Foundation.EventSystem.Property
{
    /// <summary>
    /// 可以監聽數值變化的Field
    /// </summary>
    public class SubscribableField<T> : ISubscribableField<T>
    {
        [SerializeField, ReadOnly] private T _lastValue;

        [SerializeField, ReadOnly] private T _value;

        private EventSystem _eventSystem;

        public Type ValueType
        {
            get { return typeof(T); }
        }

        /// <summary>
        ///     讀寫屬性
        /// </summary>
        public T Value
        {
            get { return _value; }
            set
            {
                LastValue = _value;
                _value = value;

                NotifyValueChanged();
            }
        }

        /// <summary>
        ///     取得上一次屬性值
        /// </summary>
        public T LastValue
        {
            get { return _lastValue; }
            private set { _lastValue = value; }
        }

        public SubscribableField(EventSystem eventSystem)
        {
            _eventSystem = eventSystem;
            _value = default(T);
        }

        public SubscribableField(EventSystem eventSystem, T initialValue)
        {
            _eventSystem = eventSystem;
            _value = initialValue;
        }

        public void SetValueWithoutNotify(T value)
        {
            LastValue = _value;
            _value = value;
        }

        /// <summary>
        ///     如果T為自定義物件(class), 使用者自行修改內容之後
        ///     必須呼叫此函式來通知監聽者屬性變化
        /// </summary>
        public void NotifyValueChanged()
        {
            SubscribableFieldEvent<T> evt = SubscribableFieldEvent<T>.Allocate(this);
            evt.EventTag = EventTagData.Tag(this);
            _eventSystem.SendEvent(evt);
        }

        /// <summary>
        ///     監聽屬性變化, 第一個object是LastValue, 第二個object指Value, 監聽者必須自行轉型
        /// </summary>
        /// <param name="callback"></param>
        /// <returns></returns>
        public IEventListener<SubscribableFieldEvent<T>> AddValueChangeListener(Action<T, T> callback)
        {
            if (callback == null)
            {
                return null;
            }

            IEventListener<SubscribableFieldEvent<T>> listener =
                _eventSystem.RegisterListenerWithTag(
                    (SubscribableFieldEvent<T> evt) =>
                    {
                        callback?.Invoke(evt.LastValue, evt.Value);
                    },
                    EventTagData.Tag(this));
            return listener;
        }
    }

    [Serializable]
    public class IntSubscribableField : SubscribableField<int>
    {
        public IntSubscribableField(EventSystem eventSystem) : base(eventSystem)
        {
        }

        public IntSubscribableField(EventSystem eventSystem, int initialValue) : base(eventSystem, initialValue)
        {
        }
    }

    [Serializable]
    public class FloatSubscribableField : SubscribableField<float>
    {
        public FloatSubscribableField(EventSystem eventSystem) : base(eventSystem)
        {
        }

        public FloatSubscribableField(EventSystem eventSystem, float initialValue) : base(eventSystem, initialValue)
        {
        }
    }

    [Serializable]
    public class StringSubscribableField : SubscribableField<string>
    {
        public StringSubscribableField(EventSystem eventSystem) : base(eventSystem)
        {
        }

        public StringSubscribableField(EventSystem eventSystem, string initialValue) : base(eventSystem, initialValue)
        {
        }
    }
}