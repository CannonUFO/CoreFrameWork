using ible.Foundation.UI;
using ible.GameModule.UI;
using UnityEngine;

namespace ible.GameModule
{
    public class ReOpenUIAction : MonoBehaviour, IUIActionProcessor
    {
        private IBaseUIView _reOpenUIRoot;
        public void DoOnCreate(IBaseUIView baseUIRoot)
        {
            
        }
        public void DoBeforeShow(IBaseUIView baseUIRoot)
        {
            if (_reOpenUIRoot != null) return;

            _reOpenUIRoot = baseUIRoot.Prev?.TraverseLast(GetSameTypeUI);
            if (_reOpenUIRoot != null) _reOpenUIRoot.Hide();
        }
        public void DoAfterHide(IBaseUIView baseUIRoot)
        {
            if (_reOpenUIRoot != null && baseUIRoot.Next == null)
                _reOpenUIRoot.Show();
        }

        private bool GetSameTypeUI(IBaseUIView uiView)
        {
            var baseView = uiView as BaseUIView;
            if (baseView.gameObject.activeSelf && baseView.IsActionExist(this))
                return true;
            return false;
        }
    }
}