using ible.Foundation;
using ible.Foundation.Utility;
using UnityEngine;

namespace ible.Foundation.EventSystem.Property.PropertyObserve.Handler.ULong
{
    /// <summary>
    /// ULong To String With Commas As To UI Text Text Handler
    /// </summary>
    public class ULongAddCommas : IBindingInfoHandler
    {
        private static readonly string HandleTypeName = ULongHandleType.AddCommas.ToString();

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