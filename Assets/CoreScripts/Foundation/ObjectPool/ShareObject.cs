

using ible.Foundation;
using UnityEngine;

namespace ible.Foundation.ObjectPool
{
    public interface IShareObject
    {
        int InstanceId { get; }
        void Reset();
        void Release();
    }
    public abstract class ShareObject<T> : IShareObject where T : ShareObject<T>, new()
    {
        protected static SharedObjectPool<T> s_objectPool;

        public int InstanceId { get; protected set; }

        public static T Allocate()
        {
            if(s_objectPool == null)
            {
                s_objectPool = SharedObjectPool<T>.Instance;
            }

            var ret = s_objectPool.Allocate();
#if DEBUG
            var trace = ret as ShareObject<T>;
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

        public virtual void Release()
        {
            var releaseObj = this as T;
            if (releaseObj != null)
                s_objectPool.Free(releaseObj);
        }

        public abstract void Reset();
    }
}
