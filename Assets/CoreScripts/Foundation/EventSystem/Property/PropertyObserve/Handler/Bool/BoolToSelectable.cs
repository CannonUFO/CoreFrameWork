using UnityEngine;

namespace ible.Foundation.EventSystem.Property.PropertyObserve.Handler.Bool
{
    /// <summary>
    /// Bool To Selectable Handler
    /// </summary>
    internal class BoolToSelectable : IBindingInfoHandler
    {
        private static readonly string HandleTypeName = BoolHandleType.IsInteractable.ToString();

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
                    if (info.selectable != null)
                    {
                        info.selectable.interactable = value;
                    }
                });

            if (listener != null && info.selectable != null)
            {
                info.selectable.interactable = listener.Value;
            }

            return listener;
        }
    }
}