
using ible.Foundation;
using ible.Foundation.UI;
using ible.GameModule.UI.ChangeSceneUI;

namespace ible.GameModule.UI.BackpackUI
{
    public class BackpackUIData : UIData<BackpackUIData>
    {
        //public BackpackUIDataInfo Info { get; private set; }

        public BackpackUIData() : base(UIName.BackpackUI)
        {

        }
    }
    
#if UNITY_EDITOR
    [ible.Foundation.UI.AutoGen.UINameAttribute(TypeName = "BackpackUI.BackpackUI")]
#endif
    public class BackpackUI : BackpackUITemplate
    {
        //protected Button showHero;
        //protected Button showMessage;
        private bool _switchChangeScnenButton;
        
        protected override void OnShow()
        {
            base.OnShow();

            BackpackUIData customData = Data as BackpackUIData;
        }
        
        protected override void OnClickShowHero()
        {
            UIManager.Instance.Show(UIName.HeroUI);
        }
        protected override void OnClickShowChangeScene()
        {
            if (!_switchChangeScnenButton)
                UIManager.Instance.Show(ChangeSceneUIData.Allocate(3));
            else
                UIManager.Instance.Hide(UIName.ChangeSceneUI);
            _switchChangeScnenButton = !_switchChangeScnenButton;
        }
    }
}
