using System;
using UnityEngine;

namespace ible.Foundation.EventSystem.Property.PropertyObserve.Handler.Float
{
    /// <summary>
    /// Float To Be Floor UI Text Handler
    /// </summary>
    public class FloatFloor : IBindingInfoHandler
    {
        private static readonly string HandleTypeName = FloatHandleType.Floor.ToString();

        public PropertyType PropertyType
        {
            get
            {
                return PropertyType.Float;
            }
        }

        public string HandleType
        {
            get
            {
                return HandleTypeName;
            }
        }

        public IPropertyListener HandleBindingInfo(GameObject gameObject, PropertyBindInfo info)
        {
            IPropertyListener<float> listener = PropertyManager.Instance.Subscribe<float>(info.propertyKey,
                (lastValue, value) =>
                {
                    if (info.Text != null)
                    {
                        info.Text.text = Math.Floor(value).ToString();
                    }
                });

            if (listener != null && info.Text != null)
            {
                info.Text.text = Math.Floor(listener.Value).ToString();
            }

            return listener;
        }
    }
}