
using ible.Foundation.UI;

namespace ible.GameModule.UI.ClanUI
{
    public class ClanUIData : UIData<ClanUIData>
    {
        //public ClanUIDataInfo Info { get; private set; }

        public ClanUIData() : base(UIName.ClanUI)
        {

        }
    }
    
#if UNITY_EDITOR
    [ible.Foundation.UI.AutoGen.UINameAttribute(TypeName = "ClanUI.ClanUI")]
#endif
    public class ClanUI : ClanUITemplate
    {
        //protected Button hideAll;
        //protected Button hideClan;

        protected override void OnShow()
        {
            base.OnShow();

            ClanUIData customData = Data as ClanUIData;
        }
        
        protected override void OnClickHideAll()
        {
            UIManager.Instance.HideAll();
        }
        protected override void OnClickHideClan()
        {
            Hide();
        }
    }
}
