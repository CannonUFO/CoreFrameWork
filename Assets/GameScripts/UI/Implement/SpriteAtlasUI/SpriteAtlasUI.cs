
using ible.Foundation.UI;
using UnityEngine.U2D;
using UnityEngine;
using Debug = ible.Foundation.Utility.Debug;
using ible.Foundation.Atlas;

namespace ible.GameModule.UI.SpriteAtlasUI
{
    public class SpriteAtlasUIData : UIData<SpriteAtlasUIData>
    {
        //public SpriteAtlasUIData Data { get; private set; }

        public SpriteAtlasUIData() : base(UIName.SpriteAtlasUI)
        {

        }

        public static new SpriteAtlasUIData Allocate(/*SpriteAtlasUIData data*/)
        {
            var ret = UIData<SpriteAtlasUIData>.Allocate();
            //ret.Data = data;
            return ret;
        }

        public override void Reset()
        {
            base.Reset();
            //Data = null;
        }
    }
    
#if UNITY_EDITOR
    [ible.Foundation.UI.AutoGen.UINameAttribute(TypeName = "SpriteAtlasUI.SpriteAtlasUI")]
#endif
    public class SpriteAtlasUI : SpriteAtlasUITemplate
    {
        //protected UnityEngine.UI.Image[] images;
        protected override void OnClickButton()
        {
            Hide();
        }

        protected override void OnShow()
        {
            base.OnShow();

            SpriteAtlasUIData customData = Data as SpriteAtlasUIData;
            Debug.Log("SpriteAtlasUI.OnShow!");

            for (int i = 0; i < lateImages.Length; i++)
            {
                Sprite sprite = AtlasManager.Instance.GetSprite($"boss_skill_100{2 + i}");
                //Sprite sprite = AtlasManager.Instance.GetSprite($"ui_tech_00{1+i}");

                if (sprite != null)
                {
                    lateImages[i].sprite = sprite;
                    lateImages[i].gameObject.SetActive(true);
                }
            }

            for (int i = 0; i < preImages.Length; i++)
            {
                preImages[i].sprite.GetBindPoses();
            }
        }
    }
}
