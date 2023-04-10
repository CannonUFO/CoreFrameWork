using UnityEngine;

namespace ible.Foundation.EventSystem.Property.PropertyObserve.Handler.String
{
    /// <summary>
    /// Set String To UI Text Handler
    /// </summary>
    public class StringToString : IBindingInfoHandler
    {
        private static readonly string HandleTypeName = StringHandleType.ToText.ToString();

        public PropertyType PropertyType
        {
            get
            {
                return PropertyType.String;
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
            IPropertyListener<string> listener = PropertyManager.Instance.Subscribe<string>(info.propertyKey,
                (lastValue, value) =>
                {
                    if (info.Text != null)
                        info.Text.text = value;
                });

            if (listener != null && info.Text != null)
            {
                info.Text.text = listener.Value;
            }

            return listener;
        }
    }
}