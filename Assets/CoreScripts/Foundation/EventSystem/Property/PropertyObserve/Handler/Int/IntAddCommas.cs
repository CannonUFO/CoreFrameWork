using ible.Foundation;
using ible.Foundation.Utility;
using UnityEngine;

namespace ible.Foundation.EventSystem.Property.PropertyObserve.Handler.Int
{
    /// <summary>
    /// Int To String With Commas As To UI Text Text Handler
    /// </summary>
    public class IntAddCommas : IBindingInfoHandler
    {
        private static readonly string HandleTypeName = IntHandleType.AddCommas.ToString();

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
                        info.Text.text = GameUtility.ConvertIntToStringWithCommas(value);
                    }
                });

            if (listener != null && info.Text != null)
            {
                info.Text.text = GameUtility.ConvertIntToStringWithCommas(listener.Value);
            }

            return listener;
        }
    }
}