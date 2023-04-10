
using ible.Foundation;
using System.Collections.Generic;


namespace ible.Foundation.ObjectPool
{
    public class SharedObjectPool<T> : CoreSingleton<SharedObjectPool<T>> where T : class, new()
    {
        private ObjectPool<T> _objectPool;
        private ObjectPool<T>.Factory _factory;

        public SharedObjectPool()
        {
            _factory = () => new T();
            _objectPool = new ObjectPool<T>(_factory, 10);
        }

        public SharedObjectPool(ObjectPool<T>.Factory factory, int size)
        {
            _factory = factory;
            _objectPool = new ObjectPool<T>(_factory, size);
        }

        public static SharedObjectPool<T> InitializeInstance(ObjectPool<T>.Factory factory, int size)
        {
            return Instance = new SharedObjectPool<T>(factory, size);
        }

        public SharedObjectPool<T> SetSize(int size)
        {
            _objectPool.Size = size;
            return this;
        }

        public T Allocate()
        {
            var ret = _objectPool.Allocate();
            return ret;
        }

        public void Free(T obj)
        {
            IShareObject reset = obj as IShareObject;
            reset?.Reset();
            _objectPool.Free(obj);
        }

        public void AllocateList(IList<T> fill, int count)
        {
            if (fill != null)
            {
                for (int i = 0; i < count; ++i)
                {
                    fill.Add(_objectPool.Allocate());
                }
            }
        }

        public void FreeList(IList<T> list)
        {
            if (list?.Count > 0)
            {
                for (int i = 0; i < list.Count; ++i)
                {
                    _objectPool.Free(list[i]);
                }

                list.Clear();
            }
        }

        public void FreeListRange(IList<T> list, int index, int count)
        {
            if (list?.Count > index)
            {
                int len = list.Count - index;
                if (len > count) len = count;

                for (int i = index, c = 0; i < list.Count && c < len; ++i, c++)
                {
                    _objectPool.Free(list[i]);
                }

                for (int i = index + len - 1; i >= index; --i)
                {
                    list.RemoveAt(i);
                }
            }
        }

        public void FreeCollection(ICollection<T> collection)
        {
            if (collection?.Count > 0)
            {
                var it = collection.GetEnumerator();
                while (it.MoveNext())
                {
                    _objectPool.Free(it.Current);
                }
                it.Dispose();
                collection.Clear();
            }
        }

        public void FreeDictionary<U>(IDictionary<U, T> dict)
        {
            if (dict?.Count > 0)
            {
                var it = dict.GetEnumerator();
                while (it.MoveNext())
                {
                    _objectPool.Free(it.Current.Value);
                }
                it.Dispose();
                dict.Clear();
            }
        }

        public int UsableCount => _objectPool.UsableCount;
        public int CreatedObjectCount => _objectPool.CreatedObjectCount;
        public int AllocateCount => _objectPool.AllocateCount;
        public int FreeCount => _objectPool.FreeCount;

        public int Size
        {
            get
            {
                return _objectPool.Size;
            }
            set
            {
                _objectPool.Size = value;
            }
        }

        public static new SharedObjectPool<T> Instance
        {
            get
            {
                if (!HasInstance)
                {
                    CoreSingleton<SharedObjectPool<T>>.Instance = InitializeInstance(() => new T(), 10);
                }
                return CoreSingleton<SharedObjectPool<T>>.Instance;
            }
            set
            {
                CoreSingleton<SharedObjectPool<T>>.Instance = value;
            }
        }
    }
}
