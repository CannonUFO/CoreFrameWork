using ible.Foundation.Utility;
using UnityEngine;

namespace ible.Foundation.EventSystem.Property.PropertyObserve.Handler.Int
{
    /// <summary>
    /// Int To UI Red Hint Handler
    /// </summary>
    public class IntRedHint : IBindingInfoHandler
    {
        private static readonly string HandleTypeName = IntHandleType.RedHint.ToString();

        private static readonly int MaxHintValue = 99;

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
                    if (info.gameObject != null)
                        info.gameObject.SetActiveSafe(value > 0);

                    if (info.Text != null)
                    {
                        if (value > MaxHintValue)
                            info.Text.text = MaxHintValue.ToString();
                        else
                            info.Text.text = value.ToString();
                        
                        if (info.postFix != null)
                        {
                            info.postFix.gameObject.SetActiveSafe(value > MaxHintValue);
                            info.postFix.text = "+";
                        }
                    }
                });

            if (info.gameObject != null)
                info.gameObject.SetActiveSafe(listener.Value > 0);

            if (listener != null && info.Text != null)
            {
                if (listener.Value > MaxHintValue)
                    info.Text.text = MaxHintValue.ToString();
                else
                    info.Text.text = listener.Value.ToString();

                if (info.postFix != null)
                {
                    info.postFix.gameObject.SetActiveSafe(listener.Value > MaxHintValue);
                    info.postFix.text = "+";
                }
            }

            return listener;
        }
    }
}