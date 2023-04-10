using ible.Foundation.UI;

namespace ible.GameModule
{
    public class IncreaseSortOrderAction : IUIActionProcessor
    {
        public void DoAfterHide(IBaseUIView baseUIRoot)
        {
            
        }

        public void DoBeforeShow(IBaseUIView baseUIRoot)
        {
            baseUIRoot.SetSortOrder(baseUIRoot.GetSortOrder() + 1000) ;
        }

        public void DoOnCreate(IBaseUIView baseUIRoot)
        {
            
        }
    }
}