#if UNITY_EDITOR

using System;
using System.Collections.Generic;

namespace ible.Foundation.UI.AutoGen
{
    public class AutoUISetting
    {
        public static readonly List<Type> IgnoreTypes = new List<Type>()
        {
            typeof(AutoUIReference),
            typeof(AutoUIGenerator),
            //typeof(AutoUIListScrollRectGenerator),
        };

        public static readonly List<Type> PriorityTypes = new List<Type>()
        {
            //typeof(Localization.LocalizedText),
            //typeof(Utility.AmountText),
            //typeof(UIButton),
            typeof(UnityEngine.UI.Button),
            //typeof(TMPro.TMP_InputField),
            typeof(UnityEngine.UI.Toggle),
            typeof(UnityEngine.UI.ScrollRect),
            typeof(UnityEngine.UI.Slider),
            typeof(UnityEngine.UI.Scrollbar),
            typeof(UnityEngine.UI.Dropdown),
        };
    }
}

#endif