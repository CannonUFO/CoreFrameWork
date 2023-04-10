using ible.Foundation.EventSystem;
using ible.Foundation.UI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static ible.Foundation.UI.IBaseUIView;

namespace ible.GameModule.UI
{
    public abstract class BaseUIView : MonoBehaviour, IBaseUIView
    {
        public IBaseUIView Prev { get; set; }
        public IBaseUIView Next { get; set; }
        public UIData Data { get; private set; }

        //public delegate bool TravelAction(BaseUIView uIRoot);

        public Canvas Canvas { get; private set; }

        private List<IUIActionProcessor> _actionProcessors = new List<IUIActionProcessor>();

        private static int s_defaultSortOrder = -1000;
        public EventSystem EventSystem
        { 
            get
            {
                if(_eventSystem == null)
                    _eventSystem = GameAppImp.Instance.EventSystem;
                return _eventSystem;
            }
        }
        private EventSystem _eventSystem;

        public void OnCreate()
        {
            foreach (var processor in _actionProcessors)
            {
                processor?.DoOnCreate(this);
            }
            DontDestroyOnLoad(this);
            Canvas = GetComponent<Canvas>();
        }

        public void InitData(IBaseUIView headUI, UIData data)
        {
            Data = data;
            AddSelfLink(headUI as BaseUIView);
            SetSortOrder();
            Show();
        }
        private void AddSelfLink(BaseUIView headUI)
        {
            if (headUI != this)
                headUI.AddToLink(this);
        }
        private void SetSortOrder()
        {

            if (Prev != null)
            {
                var prevOrder = Prev.GetSortOrder();
                Canvas.sortingOrder = prevOrder < 0 ? prevOrder + 1 : prevOrder - 1000 + 1;
            }
            else
                Canvas.sortingOrder = s_defaultSortOrder;
        }
        public void Show()
        {
            foreach (var processor in _actionProcessors)
            {
                processor?.DoBeforeShow(this);
            }
            OnShow();
        }
        public void Hide()
        {
            foreach (var processor in _actionProcessors)
            {
                processor?.DoAfterHide(this);
            }
            RemoveSelfFromLink();
            OnHide();
        }
        /// <summary>
        /// 發送事件
        /// </summary>
        /// <param name="obj">事件物件</param>
        /// <param name="isForceNow">是否強制立刻發送，不會進入序列造成延遲</param>
        public void SendEvent(IEvent obj, bool isForce = false)
        {
            _eventSystem?.SendEvent(obj, isForce);
        }

        /// <summary>
        /// 註冊本體的Listener
        /// </summary>
        /// <typeparam name="TListen">Listener型態</typeparam>
        /// <param name="callback">呼叫對象</param>
        /// <returns>Listener</returns>
        public IEventListener<TListen> RegisterListener<TListen>(Action<TListen> callback) where TListen : class, IEvent
        {
            return _eventSystem?.RegisterListenerWithObject(callback, this);
        }
        /// <summary>
        /// 註冊本體的Listener (標籤)
        /// </summary>
        /// <typeparam name="TListen">Listener型態</typeparam>
        /// <param name="callback">呼叫對象</param>
        /// <param name="tag">標籤</param>
        /// <returns>Listener</returns>
        public IEventListener<TListen> RegisterListener<TListen>(Action<TListen> callback, EventTagData tag) where TListen : class, IEvent
        {
            return _eventSystem?.RegisterListenerWithTagAndObject(callback, this, tag);
        }

        public IBaseUIView TraverseLast(Traverser action)
        {
            if (action.Invoke(this))
                return this;
            else
                return Prev == null ? null : Prev.TraverseLast(action);
        }
        public IBaseUIView TraverseNext(Traverser action)
        {
            if (action.Invoke(this))
                return this;
            else
                return Next == null ? null : Next.TraverseNext(action);
        }
        /// <summary>
        /// 從本體往後找，找最近的
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IBaseUIView TraverseNext(UIName name)
        {
            if (name == Data.Name)
                return this;
            else
                return Next == null ? null : Next.TraverseNext(name);
        }
        /// <summary>
        /// 從最後一個開始找
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public IBaseUIView GetLast(UIName name)
        {
            if (Next == null)
                return this;
            else
            {
                var last = Next.GetLast(name);
                if (name == last.Data.Name)
                    return last;
                return Prev;
            } 
        }

        internal bool IsActionExist(IUIActionProcessor actionProcessor)
        {
            foreach (var processor in _actionProcessors)
            {
                if (processor.GetType() == actionProcessor.GetType())
                    return true;
            }
            return false;
        }

        protected void AddActionProcessor(IUIActionProcessor actionProcessor)
        {
            _actionProcessors.Add(actionProcessor);
        }

        protected virtual void Awake() 
        {
            var rootObjs = GetComponentsInChildren<IUIActionProcessor>();
            foreach (var processor in rootObjs)
            {
                if(processor != null) AddActionProcessor(processor);
            }

            //OnAwake();
        }
        //protected virtual void OnAwake()
        //{
        //}

        protected virtual void OnShow() 
        {
            gameObject.SetActive(true);
        }
        protected virtual void OnHide() 
        {
            gameObject?.SetActive(false);
        }

        protected virtual void OnDestroy() 
        {
            Release();
        }

        private void AddToLink(BaseUIView uIRoot)
        {
            if (Next == null)
            {
                Next = uIRoot;
                uIRoot.Prev = this;
                //Debug.Log($"AddToLick Pre:{uIRoot.Prev?.Data.InstanceId}, uIRoot:{uIRoot.Data.InstanceId}, ");
            }
            else
                (Next as BaseUIView)?.AddToLink(uIRoot);
        }
        private void RemoveSelfFromLink()
        {
            //check if the Last one
            if (Next == null && Prev != null)
            {
                Prev.Next = null;
                Release();
            }
            else if (Next == null &&Prev == null) // mean the first UI
            {
                Release();
            }
        }

        private void Release()
        {
            if(Data != null )
            {
                //Debug.Log($"BaseUIRoot Release InstanceId:{Data.InstanceId}");
                Data.Release();
                Data = null;
            }
            
            Addressables.ReleaseInstance(gameObject);
        }

        public int GetSortOrder()
        {
            return Canvas.sortingOrder;
        }
        public void SetSortOrder(int order)
        {
            Canvas.sortingOrder = order;
        }

#if UNITY_EDITOR
        private void OnApplicationQuit()
        {

        }
#endif
    }
}
