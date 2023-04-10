using System;

namespace ible.Foundation.UI
{
    interface IUILoader
    {
        void Load(UIData data, Action<IBaseUIView, UIData> OnLoadComplete);
    }
}