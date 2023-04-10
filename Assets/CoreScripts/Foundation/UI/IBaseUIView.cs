
using System;

namespace ible.Foundation.UI
{
    public interface IBaseUIView
    {
        IBaseUIView Next { get; set; }
        IBaseUIView Prev { get; set; }
        UIData Data { get; }

        void OnCreate();
        void InitData(IBaseUIView headUI, UIData uIData);

        void Show();
        void Hide();
        
        IBaseUIView GetLast(UIName name);
        IBaseUIView TraverseNext(UIName name);
        void SetSortOrder(int value);
        int GetSortOrder();

        IBaseUIView TraverseLast(Traverser action);
        IBaseUIView TraverseNext(Traverser action);

        delegate bool Traverser(IBaseUIView baseView);
    }
}
