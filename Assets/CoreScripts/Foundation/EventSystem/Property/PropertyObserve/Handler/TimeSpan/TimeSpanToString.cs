using UnityEngine;

namespace ible.Foundation.EventSystem.Property.PropertyObserve.Handler.TimeSpan
{
    /// <summary>
    /// Set TimeSpan To UI Text Handler
    /// </summary>
    public class TimeSpanToString : IBindingInfoHandler
    {
        private static readonly string HandleTypeName = TimeSpanHandleType.ToText.ToString();

        public PropertyType PropertyType
        {
            get
            {
                return PropertyType.TimeSpan;
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
            IPropertyListener<System.TimeSpan> listener = PropertyManager.Instance.Subscribe<System.TimeSpan>(info.propertyKey,
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