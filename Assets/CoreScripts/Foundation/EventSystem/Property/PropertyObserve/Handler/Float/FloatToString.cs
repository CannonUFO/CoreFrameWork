using UnityEngine;

namespace ible.Foundation.EventSystem.Property.PropertyObserve.Handler.Float
{
    /// <summary>
    /// Float To UI Text Handler
    /// </summary>
    public class FloatToString : IBindingInfoHandler
    {
        private static readonly string HandleTypeName = FloatHandleType.ToText.ToString();

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
                        info.Text.text = value.ToString();
                    }
                });

            if (listener != null && info.Text != null)
            {
                info.Text.text = listener.Value.ToString();
            }

            return listener;
        }
    }
}