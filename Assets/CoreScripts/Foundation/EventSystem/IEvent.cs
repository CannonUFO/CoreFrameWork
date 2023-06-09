﻿using System;

namespace ible.Foundation.EventSystem
{
    /// <summary>
    /// 事件介面
    /// </summary>
    public interface IEvent
    {
        /// <summary>
        /// 針對特定tag發送事件
        /// </summary>
        EventTagData EventTag { get; }

        /// <summary>
        /// 事件的實體型別
        /// </summary>
        Type EventType { get; }

        /// <summary>
        /// 事件的實體型別
        /// </summary>
        Type GetType();
    }
}