using System.ComponentModel;
using UnityEngine;

namespace ible.Foundation.UI
{
    public class BaseUIComponent : MonoBehaviour//, IUIComponent
    {
        [HideInInspector]
        public bool IsCombineArray;

        private bool _isInitTrigger;
        //private UIViewTriggerBase _selfUIViewTrigger;

        public virtual void Show()
        {
            if (!_isInitTrigger)
            {
                _isInitTrigger = true;
                //_selfUIViewTrigger = GetComponent<UIViewTriggerBase>();
            }

            if (!gameObject.activeSelf)
                gameObject.SetActive(true);

            //if (_selfUIViewTrigger != null)
            //{
            //    _selfUIViewTrigger.Show();
            //}
        }

        public virtual void Hide()
        {
            if (!_isInitTrigger)
            {
                _isInitTrigger = true;
                //_selfUIViewTrigger = GetComponent<UIViewTriggerBase>();
            }

            //if (_selfUIViewTrigger != null)
            //{
            //    _selfUIViewTrigger.Hide();
            //    _selfUIViewTrigger.OnAllViewHideEnd += () => { gameObject.SetActive(false); };
            //}
            //else
            //{
                if (gameObject.activeSelf)
                    gameObject.SetActive(false);
            //}
        }

        public bool IsVisible()
        {
            return gameObject.activeSelf;
        }

        //protected override void OnDestroyFromBase()
        //{
        //    base.OnDestroyFromBase();

        //    _selfUIViewTrigger = null;

        //    IconResourceCollection.Instance.Release(this);
        //}
    }
}