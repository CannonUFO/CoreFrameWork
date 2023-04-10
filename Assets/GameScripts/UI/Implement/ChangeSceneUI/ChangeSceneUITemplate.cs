﻿// <auto-generated>

using UnityEngine;
using ible.Foundation.UI;
using ible.Foundation;
using UnityEngine.UI;

namespace ible.GameModule.UI.ChangeSceneUI
{
    public abstract class ChangeSceneUITemplate : BaseUIView
    {
        [SerializeField]
        protected Button homeScene;
        [SerializeField]
        protected Button initScene;
        [SerializeField]
        protected Button propertyTest;
        [SerializeField]
        protected Button socialScene;
        
        protected sealed override void Awake()
        {   
            base.Awake();
            homeScene.onClick.AddListener(OnClickHomeScene);
            initScene.onClick.AddListener(OnClickInitScene);
            propertyTest.onClick.AddListener(OnClickPropertyTest);
            socialScene.onClick.AddListener(OnClickSocialScene);
        }
        
        protected abstract void OnClickHomeScene();
        protected abstract void OnClickInitScene();
        protected abstract void OnClickPropertyTest();
        protected abstract void OnClickSocialScene();
    }
}
