﻿using UnityEngine;

namespace ible.Foundation.EventSystem.Property.PropertyObserve.Handler.Float
{
    /// <summary>
    /// Float To Be Slider Value Handler
    /// </summary>
    internal class FloatSliderPercentage : IBindingInfoHandler
    {
        private static readonly string HandleTypeName = FloatHandleType.SliderPercentage.ToString();

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
                    if (info.slider != null)
                    {
                        info.slider.value = value;
                    }
                });

            if (listener != null && info.slider != null)
            {
                info.slider.value = listener.Value;
            }

            return listener;
        }
    }
}