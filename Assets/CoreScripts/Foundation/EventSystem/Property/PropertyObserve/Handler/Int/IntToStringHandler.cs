using UnityEngine;

namespace ible.Foundation.EventSystem.Property.PropertyObserve.Handler.Int
{
    /// <summary>
    /// Int To UI Text Handler
    /// </summary>
    public class IntToStringHandler : IBindingInfoHandler
    {
        private static readonly string HandleTypeName = IntHandleType.ToText.ToString();

        public PropertyType PropertyType
        {
            get { return PropertyType.Int; }
        }

        public string HandleType
        {
            get { return HandleTypeName; }
        }

        public IPropertyListener HandleBindingInfo(GameObject gameObject, PropertyBindInfo info)
        {
            IPropertyListener<int> listener = PropertyManager.Instance.Subscribe<int>(info.propertyKey,
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