using ible.Foundation.EventSystem.Property.PropertyObserve.Handler.Bool;
using ible.Foundation.EventSystem.Property.PropertyObserve.Handler.Color;
using ible.Foundation.EventSystem.Property.PropertyObserve.Handler.DateTime;
using ible.Foundation.EventSystem.Property.PropertyObserve.Handler.Float;
using ible.Foundation.EventSystem.Property.PropertyObserve.Handler.Int;
using ible.Foundation.EventSystem.Property.PropertyObserve.Handler.Long;
using ible.Foundation.EventSystem.Property.PropertyObserve.Handler.Quaternion;
using ible.Foundation.EventSystem.Property.PropertyObserve.Handler.String;
using ible.Foundation.EventSystem.Property.PropertyObserve.Handler.TimeSpan;
using ible.Foundation.EventSystem.Property.PropertyObserve.Handler.ULong;
using ible.Foundation.EventSystem.Property.PropertyObserve.Handler.Vector2;
using ible.Foundation.EventSystem.Property.PropertyObserve.Handler.Vector3;
using System.Collections.Generic;
using UnityEngine;

namespace ible.Foundation.EventSystem.Property.PropertyObserve
{
    public class PropertyBindingHandler
    {
        private static PropertyBindingHandler s_instance;

        private List<IBindingInfoHandler> _handlerList;

        private PropertyBindingHandler()
        {
            _handlerList = new List<IBindingInfoHandler>();

            // 加入各類型資料的處理方式

            _handlerList.Add(new IntToStringHandler());
            _handlerList.Add(new IntAddCommas());
            _handlerList.Add(new IntRedHint());

            _handlerList.Add(new LongToStringHandler());
            _handlerList.Add(new LongAddCommas());

            _handlerList.Add(new ULongToStringHandler());
            _handlerList.Add(new ULongAddCommas());

            _handlerList.Add(new FloatCeil());
            _handlerList.Add(new FloatFloor());
            _handlerList.Add(new FloatRound());
            _handlerList.Add(new FloatToString());
            _handlerList.Add(new FloatSliderPercentage());
            _handlerList.Add(new FloatImageFillAmount());

            _handlerList.Add(new StringToImageSprite());
            _handlerList.Add(new StringToString());

            _handlerList.Add(new BoolToString());
            _handlerList.Add(new BoolToToggle());
            _handlerList.Add(new BoolToVisible());
            _handlerList.Add(new BoolToSelectable());

            _handlerList.Add(new TimeSpanToString());

            _handlerList.Add(new DateTimeToString());

            _handlerList.Add(new ColorToImageColor());

            _handlerList.Add(new QuaternionToLocalRotation());

            _handlerList.Add(new Vector2ToAnchorPosition());

            _handlerList.Add(new Vector3ToLocalPosition());
            _handlerList.Add(new LongToTimeTextRedHint());
        }

        /// <summary>
        ///     處理在Inspector上編輯的屬性資料, 並反映到目標物件
        /// </summary>
        /// <param name="gameObject">PropertyObserver或PropertysObserver物件</param>
        /// <param name="info">Inspector上編輯的屬性綁定資料</param>
        /// <returns></returns>
        public static IPropertyListener HandleBindingInfo(GameObject gameObject, PropertyBindInfo info)
        {
            if (s_instance == null)
            {
                s_instance = new PropertyBindingHandler();
            }

            return s_instance.DoHandleBindingInfo(gameObject, info);
        }

        private IPropertyListener DoHandleBindingInfo(GameObject gameObject, PropertyBindInfo info)
        {
            List<IBindingInfoHandler>.Enumerator iter = _handlerList.GetEnumerator();
            while (iter.MoveNext())
            {
                IBindingInfoHandler handler = iter.Current;
                if (handler == null)
                {
                    continue;
                }
                if (handler.PropertyType.ToString() == info.propertyType
                   && handler.HandleType == info.handleType)
                {
                    return handler.HandleBindingInfo(gameObject, info);
                }
            }
            iter.Dispose();

            Debug.LogWarningFormat("Property( type:{0}, name:{1}, handle:{2} ) don't handle!",
                info.propertyType, info.propertyKey, info.handleType);

            return null;
        }
    }
}