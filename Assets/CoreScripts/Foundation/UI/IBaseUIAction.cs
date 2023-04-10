
namespace ible.Foundation.UI
{
    public interface IUIActionProcessor
    {
        void DoOnCreate(IBaseUIView baseUIRoot);
        void DoBeforeShow(IBaseUIView baseUIRoot);
        void DoAfterHide(IBaseUIView baseUIRoot);
    }
}