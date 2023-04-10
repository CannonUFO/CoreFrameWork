using UnityEngine;

namespace ible.Foundation.EventSystem.Property.PropertyObserve.Handler.ULong
{
    /// <summary>
    /// ULong To UI Text Handler
    /// </summary>
    public class ULongToStringHandler : IBindingInfoHandler
    {
        private static readonly string HandleTypeName = ULongHandleType.ToText.ToString();

        public PropertyType PropertyType
        {
            get { return PropertyType.ULong; }
        }

        public string HandleType
        {
            get { return HandleTypeName; }
        }

        public IPropertyListener HandleBindingInfo(GameObject gameObject, PropertyBindInfo info)
        {
            IPropertyListener<ulong> listener = PropertyManager.Instance.Subscribe<ulong>(info.propertyKey,
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