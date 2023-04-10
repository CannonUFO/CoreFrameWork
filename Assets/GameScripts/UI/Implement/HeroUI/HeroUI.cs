
using ible.Foundation.UI;
using System;
using UnityEngine;

namespace ible.GameModule.UI.HeroUI
{
    public class HeroUIData : UIData<HeroUIData>
    {
        public int Index;

        //public HeroUIData Data { get; private set; }

        public HeroUIData() : base(UIName.HeroUI)
        {

        }

        public static HeroUIData Allocate(int index)
        {
            var ret = Allocate();
            ret.Index = index;
            return ret;
        }
    }
    
#if UNITY_EDITOR
    [ible.Foundation.UI.AutoGen.UINameAttribute(TypeName = "HeroUI.HeroUI")]
#endif
    public class HeroUI : HeroUITemplate
    {
        //protected Button hideHero;
        //protected Button showClan;
        //protected Text textTMP;

        protected override void OnShow()
        {
            base.OnShow();

            HeroUIData customData = Data as HeroUIData;
        }

        protected override void OnClickHideHero()
        {
            Hide();
        }

        protected override void OnClickShowClan()
        {
            UIManager.Instance.Show(UIName.ClanUI);
        }
    }
}
