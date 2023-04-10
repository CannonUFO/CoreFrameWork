using UnityEngine;
using UnityEngine.Serialization;

namespace ible.Foundation.EventSystem.Property.PropertyObserve
{
    public class PropertyObserver : MonoBehaviour
    {
        [FormerlySerializedAs("_bindingInfo")]
        [HideInInspector]
        public PropertyBindInfo bindingInfo = new PropertyBindInfo();

        private IPropertyListener _listener;

        private void Awake()
        {
            _listener = PropertyBindingHandler.HandleBindingInfo(gameObject, bindingInfo);
        }

        private void OnDestroy()
        {
            if (_listener != null)
            {
                _listener.Unsubscribe();
            }
        }
    }
}