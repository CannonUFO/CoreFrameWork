using UnityEngine;

namespace ible.Foundation.EventSystem.Property.PropertyObserve.Handler.DateTime
{
    /// <summary>
    /// DateTime To UI Text Handler
    /// </summary>
    internal class DateTimeToString : IBindingInfoHandler
    {
        private static readonly string HandleTypeName = DateTimeHandleType.ToText.ToString();

        public PropertyType PropertyType
        {
            get
            {
                return PropertyType.DateTime;
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
            IPropertyListener<System.DateTime> listener = PropertyManager.Instance.Subscribe<System.DateTime>(info.propertyKey,
                (lastValue, value) =>
                {
                    if (info.Text != null)
                    {
                        if (info.isUniversal)
                        {
                            info.Text.text = value.ToUniversalTime().ToString();
                        }
                        else
                        {
                            info.Text.text = value.ToString();
                        }
                    }
                });

            if (listener != null && info.Text != null)
            {
                if (info.isUniversal)
                {
                    info.Text.text = listener.Value.ToUniversalTime().ToString();
                }
                else
                {
                    info.Text.text = listener.Value.ToString();
                }
            }

            return listener;
        }
    }
}