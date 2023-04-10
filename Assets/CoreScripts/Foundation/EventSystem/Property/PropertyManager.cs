using ible.Foundation.Utility;
using System;
using System.Collections.Generic;

namespace ible.Foundation.EventSystem.Property
{
    /// <summary>
    ///     隔離屬性(資料)提供者與監聽者的相依性,
    ///     提供者向PropertyManager使用AddProperty公開自身的屬性
    ///     監聽者利用Subscribe監聽屬性變化
    /// </summary>
    public partial class PropertyManager
    {
        private partial class PropertyListener<T> : IPropertyListener<T>
        {
        }

        private static volatile PropertyManager s_instance;
        private static object s_syncRoot = new ();

        private Dictionary<string, Type> _nameToTypeDic;
        private Dictionary<string, ISubscribableField> _nameToPropertyDic;
        private Dictionary<string, HashSet<IPropertyListener>> _nameToUnsubscribedListeners;

        public static PropertyManager Instance
        {
            get
            {
                if (s_instance == null)
                {
                    lock (s_syncRoot)
                    {
                        if (s_instance == null)
                        {
                            s_instance = new PropertyManager();

#if UNITY_EDITOR
                            // 開始或結束要把instance釋放掉, 不然如果Asset有改變 這邊不會重新讀入
                            UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeStateChange;
#endif
                        }
                    }
                }
                return s_instance;
            }
        }

        private PropertyManager()
        {
            _nameToTypeDic = new Dictionary<string, Type>();
            _nameToPropertyDic = new Dictionary<string, ISubscribableField>();
            _nameToUnsubscribedListeners = new Dictionary<string, HashSet<IPropertyListener>>();
        }

        public void Reset()
        {
            _nameToTypeDic.Clear();
            _nameToPropertyDic.Clear();
            _nameToUnsubscribedListeners.Clear();
        }

        /// <summary>
        /// 取得公開屬性的註冊點，方便修改property進行修改觸發
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public ISubscribableField<T> GetProperty<T>(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return null;
            }

            ISubscribableField field;
            if (_nameToPropertyDic.TryGetValue(propertyName, out field))
            {
                return field as ISubscribableField<T>;
            }

            return null;
        }

        /// <summary>
        /// 公開屬性, 方便任意對象可以監聽屬性變化
        /// </summary>
        /// <typeparam name="T">屬性本體類別</typeparam>
        /// <param name="propertyName">屬性名稱</param>
        /// <param name="property">屬性包裝物件</param>
        /// <returns></returns>
        public bool AddProperty<T>(string propertyName, SubscribableField<T> property)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return false;
            }
            if (property == null)
            {
                Debug.LogErrorFormat("Add Property Fail Property {0} SubscribableField Is Null", propertyName);
                return false;
            }
#if UNITY_EDITOR
            Debug.LogFormat("Add Property {0} : {1}", propertyName, property.ValueType.Name);
#endif
            if (_nameToPropertyDic.ContainsKey(propertyName))
            {
                Debug.LogErrorFormat("Property( {0} ) has existed !!!", propertyName);
                return false;
            }

            Type propertyType = typeof(T);
            Type recordType;
            _nameToTypeDic.TryGetValue(propertyName, out recordType);
            if (recordType != null && recordType != propertyType)
            {
                // 監聽者的型別與屬性型別不相符
                Debug.LogErrorFormat(
                    "Some Listener's propertyName doesn't match the propertyType !!!!, name: {0}, type : {1}, realType: {2}",
                    propertyName, recordType.DeclaringType, propertyType.DeclaringType);
            }

            _nameToPropertyDic.Add(propertyName, property);
            if (recordType == null)
            {
                _nameToTypeDic.Add(propertyName, propertyType);
            }

            HashSet<IPropertyListener> unsubscribeListener;
            _nameToUnsubscribedListeners.TryGetValue(propertyName, out unsubscribeListener);

            if (unsubscribeListener == null)
            {
                return true;
            }

            // 把屬性提供者綁定到 屬性提供出現前就註冊的監聽者
            HashSet<IPropertyListener>.Enumerator enumerator = unsubscribeListener.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Current == null)
                {
                    continue;
                }
                enumerator.Current.Subscribe(property);
            }
            enumerator.Dispose();

            // 因為之前註冊時還沒有屬性提供者
            // 通知上面這些Listener有新Value
            property.NotifyValueChanged();

            return true;
        }

        /// <summary>
        /// 移除公開的屬性
        /// </summary>
        /// <param name="propertyName">屬性名稱</param>
        /// <returns></returns>
        public bool RemoveProperty(string propertyName)
        {
            return _nameToPropertyDic.Remove(propertyName);
        }

        /// <summary>
        /// 訂閱某屬性, 變化時會通知
        /// </summary>
        /// <typeparam name="T">屬性本體類別</typeparam>
        /// <param name="propertyName">屬性名稱</param>
        /// <param name="onPropertyChange">屬性變化時的回調函式, 第一個參數是前一次的值, 第二個參數是當前的值</param>
        /// <returns></returns>
        public IPropertyListener<T> Subscribe<T>(string propertyName, Action<T, T> onPropertyChange)
        {
            if (string.IsNullOrEmpty(propertyName))
            {
                return null;
            }

            if (onPropertyChange == null)
            {
                return null;
            }

            Type subscribeType = typeof(T);
            Type propertyType;
            _nameToTypeDic.TryGetValue(propertyName, out propertyType);
            if (propertyType != null && subscribeType != propertyType)
            {
                // 屬性名稱與已存在的屬性類別不相符
                Debug.LogErrorFormat("PropertyName doesn't match the propertyType !!!!, name: {0}, type : {1}",
                    propertyName, propertyType.DeclaringType);
                return null;
            }

            IPropertyListener<T> listener = new PropertyListener<T>(propertyName, onPropertyChange);

            ISubscribableField property;
            _nameToPropertyDic.TryGetValue(propertyName, out property);

            if (property == null)
            {
                // 屬性提供者尚未出現, 這邊先記起來, 等待提供者出現
                HashSet<IPropertyListener> listeners;
                _nameToUnsubscribedListeners.TryGetValue(propertyName, out listeners);
                if (listeners == null)
                {
                    listeners = new HashSet<IPropertyListener>();
                    _nameToUnsubscribedListeners.Add(propertyName, listeners);
                }
                listeners.Add(listener);
            }
            else
            {
                listener.Subscribe(property);
                if (propertyType == null)
                {
                    _nameToTypeDic.Add(propertyName, typeof(T));
                }

                // 存起來 如果提供者先移除, 那監聽者可以等待下次提供者出現時 再次監聽
                HashSet<IPropertyListener> listeners;
                _nameToUnsubscribedListeners.TryGetValue(propertyName, out listeners);
                if (listeners == null)
                {
                    listeners = new HashSet<IPropertyListener>();
                    _nameToUnsubscribedListeners.Add(propertyName, listeners);
                }
                listeners.Add(listener);
            }

            return listener;
        }

        /// <summary>
        /// 提供給PropertyListener取消訂閱時, 如果提供者還沒出現, 要來從dictionary中清除
        /// </summary>
        /// <param name="listener"></param>
        private void Unsubscribe(IPropertyListener listener)
        {
            HashSet<IPropertyListener> unsubscribeListener;
            _nameToUnsubscribedListeners.TryGetValue(listener.PropertyName, out unsubscribeListener);

            if (unsubscribeListener == null)
            {
                return;
            }

            unsubscribeListener.Remove(listener);

            if (unsubscribeListener.Count == 0)
            {
                _nameToUnsubscribedListeners.Remove(listener.PropertyName);
            }
        }

#if UNITY_EDITOR

        private static void OnPlayModeStateChange(UnityEditor.PlayModeStateChange mode)
        {
            switch (mode)
            {
                case UnityEditor.PlayModeStateChange.ExitingEditMode:
                case UnityEditor.PlayModeStateChange.ExitingPlayMode:
                    s_instance = null;
                    UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeStateChange;
                    break;
            }
        }

#endif
    }
}