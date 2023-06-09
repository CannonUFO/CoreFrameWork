﻿// <auto-generated>

using UnityEngine;
using ible.Foundation.UI;
using UnityEngine.UI;

namespace ible.GameModule.UI.SpriteAtlasUI
{
    public abstract class SpriteAtlasUITemplate : BaseUIView
    {
        [SerializeField]
        protected UnityEngine.UI.Image[] lateImages;
        [SerializeField]
        protected UnityEngine.UI.Image[] preImages;
        [SerializeField]
        protected Button button;
        
        protected override void Awake()
        {   
            base.Awake();
            button.onClick.AddListener(OnClickButton);
        }
        
        protected abstract void OnClickButton();
    }
}
