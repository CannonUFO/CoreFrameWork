using ible.Foundation;
using ible.Foundation.Utility;
using UnityEngine;

namespace ible.Foundation.EventSystem.Property.PropertyObserve.Handler.Long
{
    /// <summary>
    /// Long To String With Commas As To UI Text Text Handler
    /// </summary>
    public class LongToTimeTextRedHint : IBindingInfoHandler
    {
        private static readonly string HandleTypeName = LongHandleType.ToTimeTextRedHint.ToString();

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
                       if (info.gameObject != null)
                           info.gameObject.SetActiveSafe(value > 0);

                       if (info.Text != null)
                       {
                           info.Text.text = GameUtility.SecondsToRemainingTimeString(value);
                       }
                   });

            if (info.gameObject != null)
                info.gameObject.SetActiveSafe(listener.Value > 0);

            if (listener != null && info.Text != null)
            {
                info.Text.text = GameUtility.SecondsToRemainingTimeString(listener.Value);
            }

            return listener;
        }
    }
}