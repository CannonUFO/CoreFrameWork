
using ible.Foundation;
using ible.Foundation.ObjectPool;
using UnityEngine;

namespace ible.Foundation.UI
{
    public class UIData : ShareObject<UIData>
    {
        public UIName Name { get; set; }

        public UIData()
        {
            Name = UIName.Undefine;
        }

        public override void Reset()
        {
            //UIData的Name會改變，每次回收就reset Name
            Name = UIName.Undefine;
        }
    }

    public class UIData<T> : UIData where T : UIData, new()
    {
        private new static SharedObjectPool<T> s_objectPool;


        static UIData()
        {
            s_objectPool = SharedObjectPool<T>.Instance;
        }
        protected UIData(UIName name) : base()
        {
            Name = name;
        }

        public new static T Allocate()
        {
            var ret = s_objectPool.Allocate();
#if DEBUG
            var trace = ret as UIData<T>;
            if (trace != null && trace.InstanceId == 0)
            {
                trace.InstanceId = s_objectPool.CreatedObjectCount;

                if (trace.InstanceId > s_objectPool.Size)
                {
                    Debug.LogWarningFormat($"**** Object Pool of UIData {typeof(T).Name} is too small for instance Id {trace.InstanceId}, or did not recycle object correctly!");
                }
            }
#endif
            return ret;
        }

        public override void Release()
        {
            var releaseObj = this as T;
            if (releaseObj != null)
                s_objectPool.Free(releaseObj);
        }
        
        public override void Reset()
        {
            //UIData<T>的Name不會改變，不reset Name
        }
    }
}
