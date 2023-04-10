using UnityEngine;
using ible.Foundation.Scene;
using ible.Foundation.EventSystem.Property;
using ible.Foundation.EventSystem.Property.PropertyObserve;
using ible.Foundation.EventSystem;
using ible.Foundation.UI;
using ible.GameModule.Scene;
using ible.GameModule.UI.SpriteAtlasUI;
using System;

namespace ible.GameModule.UI.ChangeSceneUI
{
    public class ChangeSceneUIData : UIData<ChangeSceneUIData>
    {
        //public ChangeSceneUIDataInfo Info { get; private set; }
        public int CreateIndex { get; set; }

        public ChangeSceneUIData() : base(UIName.ChangeSceneUI)
        {

        }

        public static ChangeSceneUIData Allocate(int v)
        {
            var ret = Allocate();
            ret.CreateIndex = v;
            return ret;
        }
    }

    public class TestEvent : EventBase<TestEvent>
    {
        public string HelloString;
        public override void Reset()
        {
            
        }

        internal static IEvent Allocate(string str)
        {
            var ret = Allocate();
            ret.HelloString = str;
            return ret;
        }
    }

#if UNITY_EDITOR
    [ible.Foundation.UI.AutoGen.UINameAttribute(TypeName = "ChangeSceneUI.ChangeSceneUI")]
#endif
    public class ChangeSceneUI : ChangeSceneUITemplate
    {
        //protected Button homeScene;
        //protected Button initScene;
        //protected Button socialScene;
        private SubscribableField<int> _subscribableField;

        private void Start()
        {
            AddActionProcessor(new IncreaseSortOrderAction());
        }

        protected override void OnShow()
        {
            base.OnShow();
            if(_subscribableField ==  null)
                _subscribableField = new SubscribableField<int>(EventSystem);
            PropertyManager.Instance.AddProperty(PropertyKey.IntProperty.GetEnumName(), _subscribableField);
            _subscribableField.AddValueChangeListener(OnSubscribeValueChange);

            ChangeSceneUIData customData = Data as ChangeSceneUIData;

            RegisterListener<TestEvent>(OnRecvTestEvent);
        }

        private void OnRecvTestEvent(TestEvent obj)
        {
            Debug.Log($"OnRecvEventStr:{obj.HelloString}");
        }

        protected override void OnHide()
        {
            base.OnHide();
            PropertyManager.Instance.RemoveProperty(PropertyKey.IntProperty.GetEnumName());
        }

        protected override void OnClickHomeScene()
        {
            SceneManager.Instance.LoadScene(SceneName.HomeScene);
        }
        protected override void OnClickInitScene()
        {
            SceneManager.Instance.LoadScene(SceneName.InitialScene);
        }

        protected override void OnClickSocialScene()
        {
            UIManager.Instance.Show(SpriteAtlasUIData.Allocate());
            //SceneManager.Instance.LoadScene(SceneName.SocialScene);
        }

        private new void OnDestroy()
        {
            base.OnDestroy();
        }

        private void OnSubscribeValueChange(int arg1, int arg2)
        {
            Debug.Log("OnSubscribeValueChange");
        }

        protected override void OnClickPropertyTest()
        {
            _subscribableField.Value += 1;
            SendEvent(TestEvent.Allocate("HelloEvent!"));
        }
    }
}
