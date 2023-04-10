using ible.Foundation.Utility;
using System.Text;
using Debug = ible.Foundation.Utility.Debug;

namespace ible.Foundation.UI
{
    /// <summary>
    /// UI管理
    /// </summary>
    public partial class UIManager : Singleton<UIManager>
    {
        private IBaseUIView _headUI;

        private IUILoader _UILoader;

        public override void Initialize()
        {
            base.Initialize();
            _UILoader = new UILoader();
        }

        public void Show(UIName name)
        {
            UIData uiData = UIData.Allocate();
            uiData.Name = name;
            DoShow(uiData);
        }

        public void Show(UIData uIData)
        {
            DoShow(uIData);
        }

        public void Hide(UIName name)
        {
            var uiRoot = _headUI?.GetLast(name);
            uiRoot?.Hide();
        }

        public void HideAll()
        {
            HideAll(_headUI);
        }

        private void DoShow(UIData uIData)
        {
            _UILoader.Load(uIData, OnLoadCompleted);
        }

        private IBaseUIView HideAll(IBaseUIView uIRoot)
        {
            if(uIRoot == null) return null;

            if (uIRoot != null && uIRoot.Next != null)
            {
                var target = HideAll(uIRoot.Next);
                target.Hide();
                if (uIRoot == _headUI)
                {
                    uIRoot.Hide();
                    _headUI = null;
                }
                return target.Prev;    
            }
            else
            {
                if (uIRoot == _headUI)
                {
                    uIRoot.Hide();
                    _headUI = null;
                }
                return uIRoot;
            }
        }

        private void OnLoadCompleted(IBaseUIView uIRoot, UIData uIData)
        {
            //Debug.Log($"OnLoadCompleted!{uIData.Name}");
            //目前不做UI Cache，但保留create跟initData流程，之後如要Cache OnCreate在載入會執行一次，之後每次開關InitData都會執行
            uIRoot.OnCreate();
            if (_headUI == null) _headUI = uIRoot;
            uIRoot?.InitData(_headUI, uIData);
#if UNITY_EDITOR
            //TestLog();
#endif
        }

        protected override void OnSingletonDestroy()
        {
            //ClearUICache(_headUI);
        }

        private IBaseUIView ClearUICache(IBaseUIView uIRoot)
        {
            if (uIRoot != null && uIRoot.Next != null)
            {
                var target = ClearUICache(uIRoot.Next);
                target.Hide();
                if (target.Prev == _headUI)
                {
                    target.Prev.Hide();
                    _headUI = null;
                }
                return target.Prev;
            }
            else
            {
                return uIRoot;
            }
        }

//#if UNITY_EDITOR
//        private void TestLog()
//        {
//            int i = 0;
//            StringBuilder stringBuilder = new StringBuilder();
//            stringBuilder.Append($"{i++}:{_headUI.name} ID:{_headUI.GetInstanceID()}");
//            IBaseUIView test = _headUI;
//            while (test.Next != null)
//            {
//                stringBuilder.Append($",{i++}:{test.Next.name} ID:{test.Next.GetInstanceID()}");
//                test = test.Next;
//                if (i > 100)
//                {
//                    Debug.LogError("You aleady died in UIManager!");
//                    break;
//                }
//            }
//            Debug.Log($"UILog={stringBuilder}");
//        }
//#endif
    }
}
