//using System;
//using UnityEngine;

//namespace ible.Foundation.UI
//{
//    public abstract class ListScrollRectBase : BaseMonoBehaviour, IContentFiller
//    {
//        public Func<int> ItemCountEvent;
//        public Func<int, int> ItemTypeEvent;

//        public int GetItemCount()
//        {
//            if (ItemCountEvent != null)
//                return ItemCountEvent();
//            return 0;
//        }

//        public int GetItemType(int index)
//        {
//            if (ItemTypeEvent != null)
//                return ItemTypeEvent(index);
//            return 0;
//        }

//        public abstract GameObject GetListItem(int index, int itemType, GameObject obj);
//    }

//    public class ListScrollRectBase<T> : ListScrollRectBase where T : MonoBehaviour
//    {
//        [SerializeField]
//        protected ListScrollRect listScrollRect;

//        [SerializeField]
//        protected T content;

//        public Action<int, int, T> ItemShowEvent;

//        public override GameObject GetListItem(int index, int itemType, GameObject obj)
//        {
//            if (obj == null)
//            {
//                obj = Instantiate(content.gameObject);
//                obj.SetActive(true);
//            }

//            if (ItemShowEvent != null)
//                ItemShowEvent(index, itemType, obj.GetComponent<T>());
//            return obj;
//        }

//        public void ScrollToListItem(int index)
//        {
//            if (listScrollRect != null)
//                listScrollRect.ScrollToListItem(index);
//        }

//        public void GoToListItem(int index)
//        {
//            if (listScrollRect != null)
//                listScrollRect.GoToListItem(index);
//        }

//        public void StopMovement()
//        {
//            if (listScrollRect != null)
//                listScrollRect.StopMovement();
//        }

//        public void RefreshScrollRect()
//        {
//            if (listScrollRect != null)
//                listScrollRect.RefreshContent();
//        }

//        public void RebuildScrollRect()
//        {
//            if (listScrollRect != null)
//                listScrollRect.RebuildContent();
//        }

//        public void VerticalAnimInitSet(int itemIdx, bool generateItem = true)
//        {
//            listScrollRect.VerticalAniInitSet(itemIdx, generateItem);
//        }

//        public Transform GetAnimatingItem()
//        {
//            return listScrollRect.GetAnimatingItem();
//        }

//        public int GetVisibleItemCounts()
//        {
//            return listScrollRect.GetVisibleItemCounts();
//        }

//        public System.Collections.Generic.List<GameObject> GetVisibleItemsAfterIndex(int index)
//        {
//            return listScrollRect.GetVisibleItemsAfterIndex(index);
//        }

//        public void ResetAnimItem()
//        {
//            listScrollRect.ResetAnimItem();
//        }

//    }
//}