using UnityEngine;

namespace ible.Foundation.EventSystem.Property.PropertyObserve.Handler.Long
{
    /// <summary>
    /// Long To UI Text Handler
    /// </summary>
    public class LongToStringHandler : IBindingInfoHandler
    {
        private static readonly string HandleTypeName = LongHandleType.ToText.ToString();

        public PropertyType PropertyType
        {
            get { return PropertyType.Long; }
        }

        public string HandleType
        {
            get { return HandleTypeName; }
        }

        public IPropertyListener HandleBindingInfo(GameObject gameObject, PropertyBindInfo info)
        {
            IPropertyListener<long> listener = PropertyManager.Instance.Subscribe<long>(info.propertyKey,
                (lastValue, value) =>
                {
                    if (info.Text != null)
                    {
                        info.Text.text = value.ToString(info.toStringForamt);
                    }
                });

            if (listener != null && info.Text != null)
            {
                info.Text.text = listener.Value.ToString(info.toStringForamt);
            }

            return listener;
        }
    }
}