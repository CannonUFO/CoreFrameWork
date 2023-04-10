using UnityEngine;

namespace ible.Foundation.EventSystem.Property.PropertyObserve.Handler.Bool
{
    /// <summary>
    /// Bool To UI Text Handler
    /// </summary>
    public class BoolToString : IBindingInfoHandler
    {
        private static readonly string HandleTypeName = BoolHandleType.ToText.ToString();

        public PropertyType PropertyType
        {
            get
            {
                return PropertyType.Bool;
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
            IPropertyListener<bool> listener = PropertyManager.Instance.Subscribe<bool>(info.propertyKey,
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