using System;

namespace ible.Foundation.EventSystem.Property
{
    /// <summary>
    /// 可訂閱Field介面
    /// </summary>
    public partial class PropertyManager
    {
        private partial class PropertyListener<T>
        {
            private IEventListener<SubscribableFieldEvent<T>> _eventListener;

            private Action<T, T> _onPropertyChange;
            private ISubscribableField<T> _subscribableField;

            public T LastValue
            {
                get { return _subscribableField != null ? _subscribableField.LastValue : default(T); }
            }

            public T Value
            {
                get { return _subscribableField != null ? _subscribableField.Value : default(T); }
            }

            public bool IsUnsubscribed { get; private set; }

            public string PropertyName { get; private set; }

            public PropertyListener(string propertyName, Action<T, T> onPropertyChange)
            {
                IsUnsubscribed = false;
                PropertyName = propertyName;
                _onPropertyChange = onPropertyChange;
            }

            public void Unsubscribe()
            {
                IsUnsubscribed = true;

                if (_eventListener != null)
                {
                    _eventListener.Unregister();
                    _eventListener = null;
                }

                _onPropertyChange = null;
                _subscribableField = null;

                Instance.Unsubscribe(this);
            }

            public void Subscribe(ISubscribableField field)
            {
                Subscribe(field as ISubscribableField<T>);
            }

            public void Subscribe(ISubscribableField<T> field)
            {
                if (field == null)
                {
                    return;
                }

                if (IsUnsubscribed)
                {
                    return;
                }

                if (_onPropertyChange == null)
                {
                    return;
                }

                if (_subscribableField != null)
                {
                    // 重新綁定 field
                    _eventListener.Unregister();
                    _subscribableField = null;
                    _eventListener = null;
                }

                _subscribableField = field;

                _eventListener = field.AddValueChangeListener(
                    OnValueChagned);
            }

            private void OnValueChagned(T pre, T cur)
            {
                if (_onPropertyChange == null)
                {
                    return;
                }

                _onPropertyChange.Invoke(pre, cur);
            }
        }
    }
}