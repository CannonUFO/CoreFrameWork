using ible.Foundation.ObjectPool;
using System;

namespace ible.Foundation.EventSystem
{
    /// <summary>
    /// 基礎事件類型，使用者必須繼承此物件來客製化事件類型
    /// </summary>
    public abstract class EventBase<T> : ShareObject<T>, IEvent
        where T : EventBase<T>, new()
    {
        private static Type s_type = typeof(T);

        /// <summary>
        ///     針對特定tag發送事件
        /// </summary>
        public EventTagData EventTag { get; set; }

        /// <summary>
        ///     事件的實體型別
        /// </summary>
        public Type EventType
        {
            get { return s_type; }
        }

        /// <summary>
        ///     事件的實體型別
        /// </summary>
        public new Type GetType()
        {
            return s_type;
        }

        public T SetEventTag(EventTagData tag)
        {
            EventTag = tag;
            return this as T;
        }
    }
}