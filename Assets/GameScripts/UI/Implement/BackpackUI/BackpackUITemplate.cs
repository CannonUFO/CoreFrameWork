﻿// <auto-generated>

using UnityEngine;
using UnityEngine.UI;
using ible.Foundation.UI;

namespace ible.GameModule.UI.BackpackUI
{
    public abstract class BackpackUITemplate : BaseUIView
    {
        [SerializeField]
        protected Button showChangeScene;
        [SerializeField]
        protected Button showHero;
        [SerializeField]
        protected Text changeSceneBtnText;
        
        protected sealed override void Awake()
        {   
            base.Awake();
            showChangeScene.onClick.AddListener(OnClickShowChangeScene);
            showHero.onClick.AddListener(OnClickShowHero);
        }
        
        protected abstract void OnClickShowChangeScene();
        protected abstract void OnClickShowHero();
    }
}
