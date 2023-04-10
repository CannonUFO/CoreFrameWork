using UnityEngine;

namespace ible.Foundation.EventSystem.Property.PropertyObserve.Handler.Bool
{
    /// <summary>
    /// Bool To Toggle Handler
    /// </summary>
    public class BoolToToggle : IBindingInfoHandler
    {
        private static readonly string HandleTypeName = BoolHandleType.ToToggle.ToString();

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
                    if (info.toggle != null)
                    {
                        info.toggle.isOn = value;
                    }
                });

            if (listener != null && info.toggle != null)
            {
                info.toggle.isOn = listener.Value;
            }

            return listener;
        }
    }
}