
using UnityEngine;
using ible.Foundation.UI;
using DG.Tweening;

namespace ible.GameModule.UI.TickerUI
{
    public class TickerUIData : UIData<TickerUIData>
    {
        public string Content{ get; private set; }

        public TickerUIData() : base(UIName.TickerUI)
        {

        }

        public static TickerUIData Allocate(string content)
        {
            var ret = Allocate();
            ret.Content = content;
            return ret;
        }

        public override void Reset()
        {
            base.Reset();
            //Data = null;
        }
    }
    
#if UNITY_EDITOR
    [ible.Foundation.UI.AutoGen.UINameAttribute(TypeName = "TickerUI.TickerUI")]
#endif
    public class TickerUI : TickerUITemplate
    {
        //protected Text textTMP;

        [SerializeField]
        private int _moveDuration = 10;
        private Vector2 _defaultPos;
        private RectTransform _textRect;
        private Tweener _tweener;

        protected override void Awake()
        {
            base.Awake();
            _textRect = textTMP.transform as RectTransform;
            _defaultPos = new Vector2(-Screen.width, - Screen.height / 2 - _textRect.offsetMin.y);
        }
        protected override void OnShow()
        {
            base.OnShow();

            TickerUIData customData = Data as TickerUIData;
            textTMP.text = customData.Content;
            _textRect.localPosition = _defaultPos;
            _tweener = _textRect.DOLocalMoveX(Screen.width, _moveDuration, true);
            _tweener.onComplete = OnMoveComplete;
        }

        private void OnMoveComplete()
        {
            Hide();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _tweener.Kill();
        }
    }
}
